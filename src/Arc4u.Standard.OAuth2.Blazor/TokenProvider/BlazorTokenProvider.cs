using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Arc4u.Blazor;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(ProviderName, typeof(ITokenProvider))]
    public class BlazorTokenProvider : ITokenProvider
    {
        public const string ProviderName = "blazor";

        public BlazorTokenProvider(ILocalStorageService localStorageService, IJSRuntime jsRuntime)
        {
            _localStorage = localStorageService;
            _jsRuntime = jsRuntime;
        }

        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings) throw new ArgumentNullException(nameof(settings));

            var token = await GetToken();

            if (null != token) return token;

            var authority = settings.Values.ContainsKey(TokenKeys.AuthorityKey) ? settings.Values[TokenKeys.AuthorityKey] : throw new ArgumentNullException(TokenKeys.AuthorityKey);
            var redirectUrl = settings.Values.ContainsKey(TokenKeys.RedirectUrl) ? settings.Values[TokenKeys.RedirectUrl] : throw new ArgumentNullException(TokenKeys.RedirectUrl);

            var interop = new WindowInterop();
            await interop.OpenWindowAsync(_jsRuntime, _localStorage, UriHelper.Encode(new Uri($"{authority}?redirectUrl={redirectUrl}")));

            return await GetToken() ?? throw new Exception("No token found!");
        }


        private async Task<TokenInfo> GetToken()
        {
            var accessToken = await _localStorage.GetItemAsStringAsync("token");

            if (!String.IsNullOrEmpty(accessToken))
            {
                var token = new JwtSecurityToken(accessToken);
                if (token.ValidTo > DateTime.UtcNow.AddMinutes(-5))
                    return new TokenInfo("Bearer", accessToken, "", token.ValidTo);
            }

            return null;
        }

        public void SignOut(IKeyValueSettings settings)
        {
            _localStorage.RemoveItemAsync("token").AsTask().Wait();
        }
    }
}
