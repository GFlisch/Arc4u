
using Arc4u.OAuth2.Options;

namespace Arc4u.OAuth2.Client.Authentication.Options;

    public class OidcClientAuthenticationOptions
    {
        public AuthorityOptions DefaultAuthority { get; set; } = default!;

        public bool ValidateAuthority { get; set; } = true;

        public Action<OidcClientSettingsOption>? OidcClientSettingsOption { get; set; }

        public Action<ClaimsIdentifierOption>? ClaimsIdentifierOptions { get; set; }

        public string CallbackPath { get; set; } = "/";
        public string PostLogoutRedirectUri { get; set; } = "/";

        public bool LoadProfile { get; set; }
        public TimeSpan ForceRefreshTimeoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(90);

        public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Define the claim type used to identify the name of the user.
        /// </summary>
        public string NameClaimType { get; set; } = "name";

        /// <summary>
        /// Define the claim type used to identify the role of the user.
        /// </summary>
        public string RoleClaimType { get; set; } = "role";
    }
