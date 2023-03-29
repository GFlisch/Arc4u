using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Extensions
{
    public class AuthenticationOptions
    {
        public IKeyValueSettings OAuthSettings { get; set; }

        public IKeyValueSettings OpenIdSettings { get; set; }

        public string MetadataAddress { get; set; } = null;

        public Func<IServiceProvider, IKeyValueSettings, ClaimsPrincipal, string, string, bool, Task<string>> OnAuthorizationCodeReceived { get; set; }

        public ICookieManager CookieManager { get; set; }

        public string CookieName { get; set; } = Constants.CookieName;

        public bool ValidateAuthority { get; set; } = false;
    }
}
