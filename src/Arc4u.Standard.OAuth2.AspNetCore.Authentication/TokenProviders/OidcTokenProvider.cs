using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TokenProviders;

[Export(OidcTokenProvider.ProviderName, typeof(ITokenProvider))]
public class OidcTokenProvider : ITokenProvider
{
    public const string ProviderName = "Oidc";

    public OidcTokenProvider(ILogger<OidcTokenProvider> logger, TokenRefreshInfo tokenRefreshInfo, IOptions<OidcAuthenticationOptions> oidcOptions, ITokenRefreshProvider refreshTokenProvider)
    {
        _logger = logger;
        _tokenRefreshInfo = tokenRefreshInfo;
        _oidcOptions = oidcOptions.Value;
        _refreshTokenProvider = refreshTokenProvider;
    }

    private readonly ILogger<OidcTokenProvider> _logger;
    private readonly TokenRefreshInfo _tokenRefreshInfo;
    private readonly OidcAuthenticationOptions _oidcOptions;
    private readonly ITokenRefreshProvider _refreshTokenProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="platformParameters"></param>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException" />
    /// <returns><see cref="TokenInfo"/></returns>
    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var timeRemaining = _tokenRefreshInfo.AccessToken.ExpiresOnUtc.Subtract(DateTime.UtcNow);

        if (timeRemaining > _oidcOptions.ForceRefreshTimeoutTimeSpan)
        {
            return _tokenRefreshInfo.AccessToken;
        }

        return await _refreshTokenProvider.GetTokenAsync(settings, null).ConfigureAwait(false);
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
