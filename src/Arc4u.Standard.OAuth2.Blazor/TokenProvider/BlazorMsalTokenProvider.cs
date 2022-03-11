using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(ProviderName, typeof(ITokenProvider))]
    public class BlazorMsalTokenProvider : ITokenProvider
    {
        public const string ProviderName = "blazor";

        public BlazorMsalTokenProvider(IAccessTokenProviderAccessor accessor)
        {
            _accessor = accessor;
        }

        private readonly IAccessTokenProviderAccessor _accessor;

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings) throw new ArgumentNullException(nameof(settings));

            var requestAccessToken = await _accessor.TokenProvider.RequestAccessToken();

            if (requestAccessToken.TryGetToken(out var accessToken))
            {
                var token = accessToken.Value;

                JwtSecurityToken jwt = new(token);
                return new TokenInfo("Bearer", token, String.Empty, jwt.ValidTo);
            }

            return null;
        }




        public void SignOut(IKeyValueSettings settings)
        {
        }
    }
}
