using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Arc4u.Caching;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.IdentityModel.Claims;
using Arc4u.Network.Connectivity;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Client.Security.Principal;

[Export(typeof(IAppPrincipalFactory))]
public class AppPrincipalFactory : IAppPrincipalFactory
{
    public const string ProviderKey = "ProviderId";
    public const string DefaultSettingsResolveName = "OAuth2";
    public const string PlatformParameters = "platformParameters";

    public static readonly string tokenExpirationClaimType = "exp";
    public static readonly string[] ClaimsToExclude = { "exp", "aud", "iss", "iat", "nbf", "acr", "aio", "appidacr", "ipaddr", "scp", "sub", "tid", "uti", "unique_name", "apptype", "appid", "ver", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant", "http://schemas.microsoft.com/identity/claims/scope" };

    private readonly IServiceProvider _container;
    private readonly INetworkInformation _networkInformation;
    private readonly ICache _claimsCache;
    private readonly ICacheKeyGenerator _cacheKeyGenerator;
    private readonly IApplicationContext _applicationContext;
    private readonly ILogger<AppPrincipalFactory> _logger;

    public AppPrincipalFactory(IServiceProvider container, INetworkInformation networkInformation, ISecureCache claimsCache, ICacheKeyGenerator cacheKeyGenerator, IApplicationContext applicationContext, ILogger<AppPrincipalFactory> logger)
    {
        _container = container;
        _networkInformation = networkInformation;
        _claimsCache = claimsCache;
        _cacheKeyGenerator = cacheKeyGenerator;
        _applicationContext = applicationContext;
        _logger = logger;
    }

    public async Task<AppPrincipal> CreatePrincipalAsync(Messages messages, object? parameter = null)
    {
        return await CreatePrincipalAsync(DefaultSettingsResolveName, messages, parameter).ConfigureAwait(true);
    }

    public async Task<AppPrincipal> CreatePrincipalAsync(string settingsResolveName, Messages messages, object? parameter = null)
    {
        var settings = _container.GetKeyedService<IKeyValueSettings>(settingsResolveName);

        if (settings == null)
        {
            throw new InvalidOperationException($"No section {settingsResolveName} was found.");
        }
        return await CreatePrincipalAsync(settings, messages, parameter).ConfigureAwait(false);
    }

    public async Task<AppPrincipal> CreatePrincipalAsync(IKeyValueSettings settings, Messages messages, object? parameter = null)
    {
        var identity = new ClaimsIdentity("OAuth2Bearer", System.Security.Claims.ClaimTypes.Upn, ClaimsIdentity.DefaultRoleClaimType);

        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(messages);

        // when we have no internet connectivity may be we have claims in cache.
        if (NetworkStatus.None == _networkInformation.Status)
        {
            // In a scenario where the claims cached are always for one user like a UI, the identity is not used => so retrieving the claims in the cache is possible!
            var emptyIdentity = new ClaimsIdentity();
            var cachedClaims = GetClaimsFromCache(emptyIdentity);
            identity.AddClaims(cachedClaims.Select(p => new Claim(p.ClaimType, p.Value)));
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Information, "Create the principal from the cache due to no network connectivity."));
        }
        else
        {
            await BuildTheIdentityAsync(identity, settings, messages, parameter).ConfigureAwait(false);
        }

        var authorization = BuildAuthorization(identity, messages);
        var profile = BuildProfile(identity, messages);

        var principal = new AppPrincipal(authorization, identity, "S-1-0-0")
        {
            Profile = profile
        };
        _applicationContext.SetPrincipal(principal);

        return principal;
    }

    private async Task BuildTheIdentityAsync(ClaimsIdentity identity, IKeyValueSettings settings, Messages messages, object? parameter = null)
    {
        // Check if we have a provider registered.
        if (!_container.TryGetService(settings.Values[ProviderKey], out ITokenProvider? provider))
        {
            throw new NotSupportedException($"The principal cannot be created. We are missing an account provider: {settings.Values[ProviderKey]}");
        }

        // Check the settings contains the service url.
        TokenInfo? token = null;
        try
        {
            token = await provider!.GetTokenAsync(settings, parameter).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
        }

        if (null != token)
        {
            // The token has claims filled by the STS.
            // We can fill the new federated identity with the claims from the token.
            var jwtToken = new JwtSecurityToken(token.Token);
            var expTokenClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals(tokenExpirationClaimType, StringComparison.InvariantCultureIgnoreCase));
            long expTokenTicks = 0;
            if (null != expTokenClaim)
            {
                long.TryParse(expTokenClaim.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out expTokenTicks);
            }

            // The key for the cache is based on the claims from a ClaimsIdentity => build a dummy identity with the claim from the token.
            var dummyIdentity = new ClaimsIdentity(jwtToken.Claims);
            var cachedClaims = GetClaimsFromCache(dummyIdentity);

            identity.BootstrapContext = token.Token;

            // if we have a token "cached" from the system, we can take the authorization claims from the cache (if exists)...
            // so we avoid too many backend calls for nothing.
            // But every time we have a token that has been refreshed, we will call the backend (if available and reload the claims).
            var cachedExpiredClaim = cachedClaims.FirstOrDefault(c => c.ClaimType.Equals(tokenExpirationClaimType, StringComparison.InvariantCultureIgnoreCase));
            long cachedExpiredTicks = 0;

            if (null != cachedExpiredClaim)
            {
                long.TryParse(cachedExpiredClaim.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cachedExpiredTicks);
            }

            // we only call the backend if the ticks are not the same.
            var copyClaimsFromCache = cachedExpiredTicks > 0 && expTokenTicks > 0 && cachedClaims.Count > 0 && cachedExpiredTicks == expTokenTicks;

            if (copyClaimsFromCache)
            {
                identity.AddClaims(cachedClaims.Select(p => new Claim(p.ClaimType, p.Value)));
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Information, "Create the principal from the cache, token has not been refreshed."));
            }
            else
            {
                // Fill the claims based on the token and the backend call
                identity.AddClaims(jwtToken.Claims.Where(c => !ClaimsToExclude.Any(arg => arg.Equals(c.Type))).Select(c => new Claim(c.Type, c.Value)));
                if (null != expTokenClaim)
                {
                    identity.AddClaim(expTokenClaim);
                }

                if (_container.TryGetService(out IClaimsFiller? claimFiller)) // Fill the claims with more information.
                {
                    try
                    {
                        // Get the claims and clean any technical claims in case of.
                        var claims = (await claimFiller!.GetAsync(identity, new List<IKeyValueSettings> { settings }, parameter).ConfigureAwait(false))
                                        .Where(c => !ClaimsToExclude.Any(arg => arg.Equals(c.ClaimType))).ToList();

                        // We copy the claims from the backend but the exp claim will be the value of the token (front end definition) and not the backend one. Otherwhise there will be always a difference.
                        identity.AddClaims(claims.Where(c => !identity.Claims.Any(c1 => c1.Type == c.ClaimType)).Select(c => new Claim(c.ClaimType, c.Value)));

                        messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Information, $"Add {claims.Count} claims to the principal."));
                        messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Information, $"Save claims to the cache."));
                    }
                    catch (Exception e)
                    {
                        messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, e.ToString()));
                    }
                }

                SaveClaimsToCache(identity);
            }
        }
        else
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Warning, "The call to identify the user has failed. Token is null!"));
        }
    }

    /// <summary>
    /// Based on the token provider Id, the method will call the token provider and build a claimPrincipal!
    /// The provider id is the string used by the Composition library to register the type and not the provider Id used by the token provider itself (Microsoft, google, or other...).
    /// Today only the connected scenario is covered!
    /// </summary>
    private Authorization BuildAuthorization(ClaimsIdentity identity, Messages messages)
    {
        var authorization = new Authorization();
        // We need to fill the authorization and user profile from the provider!
        if (_container.TryGetService(out IClaimAuthorizationFiller? claimAuthorizationFiller))
        {
            authorization = claimAuthorizationFiller!.GetAuthorization(identity);
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Information, $"Fill the authorization information to the principal."));
        }
        else
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Warning, $"No class waw found to fill the authorization to the principal."));
        }

        return authorization;
    }

    private UserProfile BuildProfile(ClaimsIdentity identity, Messages messages)
    {
        var profile = UserProfile.Empty;
        if (_container.TryGetService(out IClaimProfileFiller? profileFiller))
        {
            profile = profileFiller!.GetProfile(identity);
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Information, $"Fill the profile information to the principal."));
        }
        else
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Warning, $"No class was found to fill the principal profile."));
        }

        return profile;
    }

    // must be refactored => Take this based on a strategy based on the .Net code.
    private List<ClaimDto> GetClaimsFromCache(ClaimsIdentity identity)
    {
        try
        {
            var secureClaims = _claimsCache.Get<List<ClaimDto>>(_cacheKeyGenerator.GetClaimsKey(identity));
            return secureClaims ?? new List<ClaimDto>();
        }
        catch (Exception)
        {
            return new List<ClaimDto>();
        }
    }

    private void SaveClaimsToCache(ClaimsIdentity identity)
    {
        var claimsDto = identity.Claims.Select(c => new ClaimDto(c.Type, c.Value)).ToList();

        try
        {
            _claimsCache.Put(_cacheKeyGenerator.GetClaimsKey(identity), claimsDto);
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
        }
    }

    private void RemoveClaimsCache()
    {
        try
        {
            // In a scenario where the claims cached are always for one user like a UI, the identity is not used
            var emptyIdentity = new ClaimsIdentity();
            _claimsCache.Remove(_cacheKeyGenerator.GetClaimsKey(emptyIdentity));
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
        }
    }

    public ValueTask SignOutUserAsync(CancellationToken cancellationToken)
    {
        var settings = _container.GetKeyedService<IKeyValueSettings>(DefaultSettingsResolveName);

        if (null == settings)
        {
            throw new InvalidOperationException($"No section {DefaultSettingsResolveName} was found.");
        }

        return SignOutUserAsync(settings, cancellationToken);
    }

    public async ValueTask SignOutUserAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        RemoveClaimsCache();
        if (_container.TryGetService(settings.Values[ProviderKey], out ITokenProvider? provider))
        {
            await provider!.SignOutAsync(settings, cancellationToken).ConfigureAwait(false);
        }
    }
}
