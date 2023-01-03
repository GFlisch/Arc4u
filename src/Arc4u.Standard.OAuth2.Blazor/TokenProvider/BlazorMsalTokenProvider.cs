using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(ProviderName, typeof(ITokenProvider))]
    public class BlazorMsalTokenProvider : ITokenProvider
    {
        public const string ProviderName = "blazor";

        public BlazorMsalTokenProvider(IAccessTokenProviderAccessor accessor, IApplicationContext applicationContext)
        {
            _accessor = accessor;
            _applicationContext = applicationContext;
        }

        private readonly IAccessTokenProviderAccessor _accessor;
        private readonly IApplicationContext _applicationContext;
        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings) throw new ArgumentNullException(nameof(settings));

            if (null != _applicationContext.Principal)
            {
                var identity = (ClaimsIdentity)_applicationContext.Principal.Identity;

                if (!String.IsNullOrWhiteSpace(identity.BootstrapContext.ToString()))
                {
                    var token = identity.BootstrapContext.ToString();

                    JwtSecurityToken jwt = new(token);

                    if (jwt.ValidTo > DateTime.UtcNow)
                        return new TokenInfo("Bearer", token, jwt.ValidTo);
                }

            }

            var requestAccessToken = await _accessor.TokenProvider.RequestAccessToken();

            if (requestAccessToken.TryGetToken(out var accessToken))
            {
                var token = accessToken.Value;

                JwtSecurityToken jwt = new(token);
                return new TokenInfo("Bearer", token, jwt.ValidTo);
            }

            return null;
        }




        public void SignOut(IKeyValueSettings settings)
        {
        }
    }
}
