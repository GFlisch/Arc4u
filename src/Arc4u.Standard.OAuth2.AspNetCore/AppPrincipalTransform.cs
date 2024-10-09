using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Arc4u.Configuration;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.IdentityModel.Claims;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Arc4u.Standard.OAuth2.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2;

/// <summary>
/// Transform a <see cref="ClaimsPrincipal"/> to a <see cref="AppPrincipal"/>.
/// This needs to be injected with a scoped lifetime, because it relies on an <see cref="IApplicationContext"/>, which is also scoped.
/// </summary>
[Export(typeof(IClaimsTransformation)), Scoped]
public class AppPrincipalTransform : IClaimsTransformation
{
    private readonly IClaimProfileFiller _claimProfileFiller;
    private readonly IClaimAuthorizationFiller _claimAuthorizationFiller;
    private readonly ICacheHelper _cacheHelper;
    private readonly ActivitySource? _activitySource;
    private readonly IApplicationContext _applicationContext;
    private readonly ClaimsFillerOptions _options;
    private readonly ICacheKeyGenerator? _cacheKeyGenerator;
    private readonly IClaimsFiller? _claimsFiller;
    private readonly IOptionsMonitor<SimpleKeyValueSettings> _settings;
    private readonly ILogger<AppPrincipalTransform> _logger;
    private readonly TokenCacheOptions _cacheOptions;

    public AppPrincipalTransform(
        IApplicationContext applicationContext,
        IClaimProfileFiller claimProfileFiller,
        IClaimAuthorizationFiller claimAuthorizationFiller,
        IServiceProvider serviceProvider,
        IOptionsMonitor<SimpleKeyValueSettings> settings,
        IOptions<ClaimsFillerOptions> options,
        ICacheHelper cacheHelper,
        IActivitySourceFactory activitySourceFactory,
        IOptions<TokenCacheOptions> tokenCacheOptions,
        ILogger<AppPrincipalTransform> logger)
    {
        _claimProfileFiller = claimProfileFiller;
        _claimAuthorizationFiller = claimAuthorizationFiller;
        _applicationContext = applicationContext;
        _options = options.Value;
        _activitySource = activitySourceFactory.GetArc4u();
        _settings = settings;
        _logger = logger;
        _cacheHelper = cacheHelper;
        _cacheOptions = tokenCacheOptions.Value;

        if (_options.LoadClaimsFromClaimsFillerProvider)
        {
            _claimsFiller = serviceProvider.GetRequiredService<IClaimsFiller>();

            _cacheKeyGenerator = serviceProvider.GetRequiredService<ICacheKeyGenerator>();
        }
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        AppPrincipal appPrincipal;

        _logger.Technical().System("Create the principal.").Log();

        // Add Telemetry.
        using (var activity = _activitySource?.StartActivity("Create Arc4u Principal", ActivityKind.Producer))
        {
            // As the extension point can use some ITokenProvider based on the user.
            // A dummy Principal is created based on the context identity!
            // Must be registered as Scoped!
            _applicationContext.SetPrincipal(new AppPrincipal(new Authorization(), principal.Identity, "S-1-0-0"));

            // Load Claims from an external source if necessary!
            if (_options.LoadClaimsFromClaimsFillerProvider && principal.Identity is not null)
            {
                await LoadExtraClaimsAsync(principal.Identity as ClaimsIdentity).ConfigureAwait(false);
            }

            // Build an AppPrincipal.

            var authorization = _claimAuthorizationFiller.GetAuthorization(principal.Identity);
            var profile = _claimProfileFiller.GetProfile(principal.Identity);
            appPrincipal = new AppPrincipal(authorization, principal.Identity, profile.Sid) { Profile = profile };

            _applicationContext.SetPrincipal(appPrincipal);
        }

        return appPrincipal;
    }

    #region Handling extra claims 

    /// <summary>
    /// This code is similar to the code in AppPrincipalFactory where the claims are stored in a secureCache.
    /// The differences are:
    /// - The cache used is defined by the CacheContext.Principal.
    /// - Only the extra claims fetched are saved because on the server we will only have an identity if a network connectivity exist.
    ///     => we don't save the full claims identity like in a client where a disconnected scenario is possible!
    /// </summary>
    /// <param name="context"></param>
    private async Task LoadExtraClaimsAsync(ClaimsIdentity? identity)
    {
        if (identity is null)
        {
            _logger.Technical().LogWarning("Loading extra claims needs an identity!");
            return;
        }

        if (_claimsFiller is not null) // Fill the claims with more information.
        {
            var cacheKey = _cacheKeyGenerator?.GetClaimsKey(identity);

            // Something already in the cache? Avoid a expensive backend call.
            var cachedClaims = GetClaimsFromCache(cacheKey);

            // if cachedClaims is not null, it means that the information was in the cache and didn't expire yet.
            if (cachedClaims is not null)
            {
                // Add the new claims to the identity. Note that we need to merge the same way as in the case they are retrieved (see below)
                identity.MergeClaims(cachedClaims);
                return;
            }

            // Add Telemetry.
            using var activity = _activitySource?.StartActivity("Fetch extra claims.", ActivityKind.Producer);

            var settings = new List<IKeyValueSettings>();
            _options.SettingsKeys?.ToList().ForEach(key => settings.Add(_settings.Get(key)));

            // Should receive specific extra claims. This is the responsibility of the caller to provide the right claims.
            // We expect the exp claim to be present.
            // if not the persistence time will be the default one.
            var claims = (await _claimsFiller.GetAsync(identity, settings, null).ConfigureAwait(false)).Sanitize().ToList();

            // Load the claims into the identity but exclude the exp claim and the one already present.
            identity.MergeClaims(claims);

            TimeSpan timeout;

            // Check expiry claim explicitly returned in the call to _claimsFiller.GetAsync
            // If no expiry claim found in the call to _claimsFiller.GetAsync, try to locate one in the existing identity claims (which are normally obtained from the bearer token)
            if (claims.TryGetExpiration(out var ticks) || identity.Claims.TryGetExpiration(out ticks))
            {
                // there's an expiration claim in the claims returned by the claims filler or in the existing identity claims, so we can compute an expiration date
                timeout = DateTimeOffset.FromUnixTimeSeconds(ticks) - DateTimeOffset.UtcNow;
            }
            else
            {
                // fall back to the configured max time.
                timeout = _options.MaxTime;
            }

            SaveClaimsToCache(claims, cacheKey, timeout);
        }
    }

    private List<ClaimDto>? GetClaimsFromCache(string? cacheKey)
    {
        if (!string.IsNullOrWhiteSpace(cacheKey))
        {
            try
            {
                return _cacheHelper.GetCache().Get<List<ClaimDto>>(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();
            }
        }
        return null;
    }

    private void SaveClaimsToCache([DisallowNull] IEnumerable<ClaimDto> claims, string? cacheKey, TimeSpan timeout)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return;
        }

        // Only if the cache key is valid does it make sense to test the claims validity
        ArgumentNullException.ThrowIfNull(claims);

        if (timeout > TimeSpan.Zero)
        {
            try
            {
                _cacheHelper.GetCache().Put(cacheKey, timeout, claims);
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();
            }
        }
        else
        {
            _logger.Technical().LogWarning("The timeout is not valid to save the claims in the cache.");
        }
    }
    #endregion
}
