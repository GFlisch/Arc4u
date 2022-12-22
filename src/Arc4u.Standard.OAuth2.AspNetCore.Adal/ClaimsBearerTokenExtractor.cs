using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.IdentityModel.Claims;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Arc4u.Dependency.Attribute;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Security.Principal
{
    [Export(typeof(IClaimsFiller))]
    public class ClaimsBearerTokenExtractor : IClaimsFiller
    {
        
        public ClaimsBearerTokenExtractor(IServiceProvider container, ILogger<ClaimsBearerTokenExtractor> logger)
        {
            _container = container;

            _jsonSerializer = new DataContractJsonSerializer(typeof(IEnumerable<ClaimDto>));

            _logger = logger;
        }

        private readonly DataContractJsonSerializer _jsonSerializer;
        private readonly IServiceProvider _container;
        private readonly ILogger<ClaimsBearerTokenExtractor> _logger;

        public async Task<IEnumerable<ClaimDto>> GetAsync(IIdentity identity, IEnumerable<IKeyValueSettings> settings, object parameter)
        {
            var result = new List<ClaimDto>();

            if (null == identity)
            {
                _logger.Technical().LogError($"A null identity was received. No Claims will be generated.");
                return result;
            }

            if (!(identity is ClaimsIdentity claimsIdentity))
            {
                _logger.Technical().LogError($"The identity received is not of type ClaimsIdentity.");
                return result;
            }

            if (null == settings || settings.Count() == 0)
            {
                _logger.Technical().LogError($"We need token settings to call the backend.");
                return result;
            }
            if (null == claimsIdentity.BootstrapContext && !settings.Any(s => s.Values.ContainsKey(TokenKeys.AuthenticationTypeKey) && s.Values[TokenKeys.AuthenticationTypeKey].Equals(identity.AuthenticationType)))
            {
                _logger.Technical().System($"Skip fetching claims, no setting found for authentication type {identity.AuthenticationType}.").Log();
                return result;
            }

            try
            {
                JwtSecurityToken bearerToken = null;
                if (null != claimsIdentity.BootstrapContext)
                {
                    bearerToken = new JwtSecurityToken(claimsIdentity.BootstrapContext.ToString());
                }
                else
                {
                    // find the Provider for the AuthenticationType!
                    // exist because tested in the constructor!
                    var providerSettings = settings.First(s => s.Values[TokenKeys.AuthenticationTypeKey].Equals(identity.AuthenticationType));

                    ITokenProvider provider = _container.GetService<ITokenProvider>(providerSettings.Values[TokenKeys.ProviderIdKey]);

                    _logger.Technical().System("Requesting an authentication token.").Log();
                    var tokenInfo = await provider.GetTokenAsync(providerSettings, claimsIdentity);

                    bearerToken = new JwtSecurityToken(tokenInfo.AccessToken);
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
}

