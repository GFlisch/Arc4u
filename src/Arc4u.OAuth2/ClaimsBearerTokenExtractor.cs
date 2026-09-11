using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json.Serialization;
using Arc4u.Configuration;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.IdentityModel.Claims;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Security.Principal;

[JsonSerializable(typeof(IEnumerable<ClaimDto>))]
internal partial class ClaimsBearerTokenContext : JsonSerializerContext
{
}

[Export(typeof(IClaimsFiller))]
public class ClaimsBearerTokenExtractor : IClaimsFiller
{
    public ClaimsBearerTokenExtractor(IOptionsMonitor<SimpleKeyValueSettings> settings, IServiceProvider serviceProvider, ILogger<ClaimsBearerTokenExtractor> logger)
    {
        _settings = settings;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private readonly IOptionsMonitor<SimpleKeyValueSettings> _settings;
    private readonly ILogger<ClaimsBearerTokenExtractor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public async Task<IEnumerable<ClaimDto>> GetAsync(IIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var result = new List<ClaimDto>();

        if (identity is not ClaimsIdentity claimsIdentity)
        {
            _logger.Technical().LogError($"The identity received is not of type ClaimsIdentity.");
            return result;
        }

        if (null == claimsIdentity.BootstrapContext && _settings.Get(identity.AuthenticationType).Values.Count == 0)
        {
            _logger.Technical().LogSkipFetchingClaims(identity.AuthenticationType ?? "No AuthenticationType.");
            return result;
        }

        try
        {
            JwtSecurityToken? bearerToken = null;
            if (null != claimsIdentity.BootstrapContext)
            {
                bearerToken = new JwtSecurityToken(claimsIdentity.BootstrapContext.ToString());
            }
            else
            {
                // find the Provider for the AuthenticationType!
                var providerSettings = _settings.Get(identity.AuthenticationType).Values;

                var provider = _serviceProvider.GetKeyedService<ITokenProvider>(providerSettings[TokenKeys.ProviderIdKey]);

                if (null == provider)
                {
                    throw new InvalidOperationException($"No token provider named: {providerSettings[TokenKeys.ProviderIdKey]} is registered.");
                }

                _logger.Technical().LogRequestingAuthenticationToken();
                var tokenInfoResult = await provider.GetTokenAsync(new SimpleKeyValueSettings(providerSettings), claimsIdentity).ConfigureAwait(false);

                if (tokenInfoResult.IsFailed)
                {
                    _logger.Technical().LogNoToken();
                    tokenInfoResult.Log();
                    return result;
                }

                bearerToken = new JwtSecurityToken(tokenInfoResult.Value.Token);
            }

            result.AddRange(bearerToken.Claims.Select(c => new ClaimDto(c.Type, c.Value)));

        }
        catch (Exception exception)
        {
            _logger.Technical().LogException(exception);
        }

        return result;
    }
}

