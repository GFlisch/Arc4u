using Arc4u.Blazor;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(ProviderName, typeof(ITokenProvider))]
    public class BlazorTokenProvider : ITokenProvider
    {
        public const string ProviderName = "blazor";

        public BlazorTokenProvider(ILocalStorageService localStorageService, IJSRuntime jsRuntime, ITokenWindowInterop windowInterop)
        {
            _localStorage = localStorageService;
            _jsRuntime = jsRuntime;
            _windowInterop = windowInterop;
        }

        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private readonly ITokenWindowInterop _windowInterop;

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings) throw new ArgumentNullException(nameof(settings));

            if (null == settings.Values) throw new ArgumentNullException(nameof(settings.Values));

            var token = await GetToken();

            if (null != token) return token;

            var authority = settings.Values.ContainsKey(TokenKeys.AuthorityKey) ? settings.Values[TokenKeys.AuthorityKey] : throw new ArgumentNullException(TokenKeys.AuthorityKey);
            var redirectUrl = settings.Values.ContainsKey(TokenKeys.RedirectUrl) ? settings.Values[TokenKeys.RedirectUrl] : throw new ArgumentNullException(TokenKeys.RedirectUrl);

            var result = Uri.TryCreate(redirectUrl, UriKind.Absolute, out Uri redirectUri);

            if (!result) throw new UriFormatException($"RedirectUrl {redirectUrl} is not a valid url.");

            var redirectTo = WebUtility.UrlEncode(redirectUri.Authority + redirectUri.LocalPath.TrimEnd(new[] { '/' }));

            await _windowInterop.OpenWindowAsync(_jsRuntime, _localStorage, $"{authority}/redirectto/{redirectTo}");

            return await GetToken() ?? throw new Exception("No token found!");
        }


        private async Task<TokenInfo> GetToken()
        {
            var accessToken = await _localStorage.GetItemAsStringAsync("token");

            if (!String.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var token = new JwtSecurityToken(accessToken);
                    if (token.ValidTo > DateTime.UtcNow.AddMinutes(-5))
                        return new TokenInfo("Bearer", accessToken, "", token.ValidTo);

                    // ensure the token is removed before we try to have a new one.
                    await _localStorage.RemoveItemAsync("token");
                }
                catch (Exception)
                {
                    await _localStorage.RemoveItemAsync("token");
                }

            }

            return null;
        }

        public void SignOut(IKeyValueSettings settings)
        {
            _localStorage.RemoveItemAsync("token").AsTask().Wait();
        }
    }
}