namespace Arc4u.OAuth2.Options
{
    public class HybridAuthenticationSectionOptions : OidcAuthenticationSectionOptions
    {
        public string OAuth2SettingsSectionPath { get; set; } = "Authentication:OAuth2.Settings";

        public string OAuth2SettingsKey { get; set; } = Constants.BearerAuthenticationType;

        public string BasicAuthenticationSectionPath { get; set; } = "Authentication:Basic";

    }
}

