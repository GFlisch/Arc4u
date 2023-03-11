using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arc4u.Caching;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.IdentityModel.Claims;
using Arc4u.OAuth2;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;

namespace Arc4u.Standard.OAuth2.Middleware;


public class ClaimsPrincipalMiddleware
{
    private const string tokenExpirationClaimType = "exp";
    private static readonly string[] ClaimsToExclude = { "aud", "iss", "iat", "nbf", "acr", "aio", "appidacr", "ipaddr", "scp", "sub", "tid", "uti", "unique_name", "apptype", "appid", "ver", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant", "http://schemas.microsoft.com/identity/claims/scope" };

    private readonly RequestDelegate _next;
    private readonly ICacheContext _cacheContext;
    private readonly ClaimsPrincipalMiddlewareOption _option;
    private readonly ActivitySource _activitySource;

    public ClaimsPrincipalMiddleware(RequestDelegate next, IContainerResolve container, ClaimsPrincipalMiddlewareOption option)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        if (null == container)
            throw new ArgumentNullException(nameof(container));

        if (null == option)
            throw new ArgumentNullException(nameof(option));

        var logger = container.Resolve<ILogger<ClaimsPrincipalMiddleware>>();

        if (!container.TryResolve<ICacheContext>(out _cacheContext))
        {
            logger.Technical().Information("No cache context is available.").Log();
        }

        _option = option;

        _activitySource = container.Resolve<IActivitySourceFactory>()?.GetArc4u();

    }

    public async Task Invoke(HttpContext context)
    {
        // Get the scoped instance of the container!
        IContainerResolve container = (IContainerResolve)context.RequestServices.GetService(typeof(IContainerResolve));
        var logger = container.Resolve<ILogger<ClaimsPrincipalMiddleware>>();

        try
        {

            // if we have some part of the site not in MVC (like swagger) and we need to force
            // authentication. We can add the start of the path to check and in this case we force a login!
            if (null != context.User && !context.User.Identity.IsAuthenticated)
            {
                if (_option.OpenIdOptions.ForceAuthenticationForPaths.Any(r =>
                {
                    return r.Last().Equals('*') ?
                        context.Request.Path.Value.StartsWith(r.Remove(r.Length - 1), StringComparison.InvariantCultureIgnoreCase)
                        :
                         context.Request.Path.Value.Equals(r, StringComparison.InvariantCultureIgnoreCase);
                }))
                {
                    logger.Technical().System("Force an OpenId connection.").Log();
                    var cleanUri = new Uri(new Uri(context.Request.GetEncodedUrl()).GetLeftPart(UriPartial.Path));
                    if (Uri.TryCreate(_option.RedirectAuthority, UriKind.Absolute, out var authority))
                    {
                        cleanUri = new Uri(authority, cleanUri.AbsolutePath);
                    }
                    var properties = new AuthenticationProperties() { RedirectUri = cleanUri.ToString() };
                    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties).ConfigureAwait(false);
                    return;
                }
            }

            if (null != context.User && context.User.Identity.IsAuthenticated)
            {
                logger.Technical().System("Create the principal.").Log();

                // Add Telemetry.
                using (var activity = _activitySource?.StartActivity("Create Arc4u Principal", ActivityKind.Producer))
                {

                    // As the extension point can use some ITokenProvider based on the user.
                    // A dummy Principal is created based on the context identity!
                    // Must be registered as Scoped!
                    if (container.TryResolve<IApplicationContext>(out var applicationContext))
                    {
                        applicationContext.SetPrincipal(new AppPrincipal(new Authorization(), context.User.Identity, "S-1-0-0"));
                    }

                    // Load Claims from an external source if necessary!
                    if (_option.ClaimsFillerOptions.LoadClaimsFromClaimsFillerProvider)
                        await LoadExtraClaimsAsync(context, container, logger);

                    // Build an AppPrincipal.
                    AppPrincipal principal = null;
                    var profileFiller = container.Resolve<IClaimProfileFiller>();
                    var authorizationFiller = container.Resolve<IClaimAuthorizationFiller>();

                    var authorization = authorizationFiller.GetAuthorization(context.User.Identity);
                    var profile = profileFiller.GetProfile(context.User.Identity);
                    principal = new AppPrincipal(authorization, context.User.Identity, profile.Sid) { Profile = profile };

                    // Check if we have an ActivityID.
                    var activityIdHeader = context.Request?.Headers?.FirstOrDefault(h => h.Key.Equals("activityid", StringComparison.InvariantCultureIgnoreCase));

                    if (null == activityIdHeader || !activityIdHeader.HasValue || String.IsNullOrWhiteSpace(activityIdHeader.Value.Key) || StringValues.Empty == activityIdHeader.Value || activityIdHeader.Value.Value.Count == 0)
                        principal.ActivityID = Guid.NewGuid();
                    else
                    {
                        Guid activityId;
                        if (Guid.TryParse(activityIdHeader.Value.Value[0], out activityId) && activityId != Guid.Empty)
                            logger.Technical().Information($"Set the activity to the principal based on the caller information: {activityId}.").Log();
                        else
                        {
                            logger.Technical().Information($"The activity id given by the caller is not a valid Guid. A new one has been assigned.").Log();
                            activityId = Guid.NewGuid();
                        }

                        principal.ActivityID = activityId;
                    }

                    activity?.SetTag(LoggingConstants.ActivityId, principal.ActivityID);

                    // Check for a culture.
                    var cultureHeader = context.Request?.Headers?.FirstOrDefault(h => h.Key.Equals("culture", StringComparison.InvariantCultureIgnoreCase));
                    if (cultureHeader.HasValue && StringValues.Empty != cultureHeader.Value.Value && cultureHeader.Value.Value.Any())
                    {
                        try
                        {
                            principal.Profile.CurrentCulture = new CultureInfo(cultureHeader.Value.Value[0]);
                        }
                        catch (Exception ex)
                        {
                            logger.Technical().Exception(ex).Log();
                        }
                    }
                    logger.Technical().System("Set AppPrincipal to the HttpContext.User.").Log();
                    context.User = principal;

                    if (null != applicationContext)
                    {
                        logger.Technical().System("Set AppPrincipal to the ApplicationContext.").Log();
                        applicationContext.SetPrincipal(principal);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Technical().Exception(ex).Log();
        }

        await _next(context).ConfigureAwait(false);
    }

    /// <summary>
    /// This code is similar to the code in AppPrincipalFactory where the claims are stored in a secureCache.
    /// The differences are:
    /// - The cache used is defined by the CacheContext.Principal.
    /// - Only the extra claims fetched are saved because on the server we will only have an identity if a network connectivity exist.
    ///     => we don't save the full claims identity like in a client where a disconnected scenario is possible!
    /// </summary>
    /// <param name="context"></param>
    private async Task LoadExtraClaimsAsync(HttpContext context, IContainerResolve scope, ILogger<ClaimsPrincipalMiddleware> logger)
    {
        if (null == _option.ClaimsFillerOptions.Settings)
        {
            logger.Technical().System("No settings are defined in the ClaimPrincipalContext option definition! No extra claims will be fetched.").Log();
            return;
        }

        if (!scope.TryResolve<ICacheKeyGenerator>(out var keyGenerator))
        {
            logger.Technical().System("No user based cache key generator exist! Check your dependencies.").Log();
            return;
        }

        if (scope.TryResolve(out IClaimsFiller claimFiller)) // Fill the claims with more information.
        {
            var identity = context.User.Identity as ClaimsIdentity;
            var cacheKey = keyGenerator.GetClaimsKey(identity);

            // Something already in the cache? Avoid a expensive backend call.
            var cachedClaims = GetClaimsFromCache(cacheKey);

            // check expirity.
            var cachedExpiredClaim = cachedClaims.FirstOrDefault(c => c.ClaimType.Equals(tokenExpirationClaimType, StringComparison.InvariantCultureIgnoreCase));
            long cachedExpiredTicks = 0;

            if (null != cachedExpiredClaim && long.TryParse(cachedExpiredClaim.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cachedExpiredTicks))
            {
                var expDate = DateTimeOffset.FromUnixTimeSeconds(cachedExpiredTicks).UtcDateTime;
                if (expDate > DateTime.UtcNow)
                {
                    identity.AddClaims(cachedClaims.Where(c => !ClaimsToExclude.Any(arg => arg.Equals(c.ClaimType)))
                                                   .Where(c => !identity.Claims.Any(c1 => c1.Type == c.ClaimType))
                                                   .Select(c => new Claim(c.ClaimType, c.Value)));

                    return;
                }
            }

            // Add Telemetry.
            using (var activity = _activitySource?.StartActivity("Fetch extra claims.", ActivityKind.Producer))
            {
                // Call and check if we have to add claims.
                // Clean the claims received.
                var claims = (await claimFiller.GetAsync(identity, _option.ClaimsFillerOptions.Settings, null).ConfigureAwait(false)).Where(c => !ClaimsToExclude.Any(arg => arg.Equals(c.ClaimType))).ToList();


                // Load the claims into the identity.
                identity.AddClaims(claims.Where(c => !identity.Claims.Any(c1 => c1.Type == c.ClaimType))
                                         .Select(c => new Claim(c.ClaimType, c.Value)));

                // Add the expiration of the token.
                var expClaim = identity.Claims.FirstOrDefault(c => c.Type.Equals(tokenExpirationClaimType, StringComparison.InvariantCultureIgnoreCase));
                if (null != expClaim)
                {
                    claims.Add(new ClaimDto(expClaim.Type, expClaim.Value));

                    SaveClaimsToCache(claims, cacheKey, logger);
                }
            }
        }

    }

    private List<ClaimDto> GetClaimsFromCache(string cacheKey)
    {
        try
        {
            var claimsCache = _cacheContext[_cacheContext.Principal.CacheName];

            var claims = claimsCache.Get<List<ClaimDto>>(cacheKey);
            return claims ?? new List<ClaimDto>();
        }
        catch (Exception)
        {
            return new List<ClaimDto>();
        }
    }

    private void SaveClaimsToCache(IEnumerable<ClaimDto> claims, string cacheKey, ILogger<ClaimsPrincipalMiddleware> logger)
    {
        try
        {
            var claimsCache = _cacheContext[_cacheContext.Principal.CacheName];

            // Add Telemetry.
            claimsCache.Put(cacheKey, claims);

        }
        catch (Exception ex)
        {
            logger.Technical().Exception(ex).Log();
        }

    }
}
