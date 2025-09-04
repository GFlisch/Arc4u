namespace Arc4u.OAuth2.Options
{
    public class HybridAuthenticationOptions : OidcAuthenticationOptions
    {
        public string OAuth2SettingsKey { get; set; } = Constants.OAuth2OptionsName;

        public Action<OAuth2SettingsOption> OAuth2SettingsOptions { get; set; } = default!;
    }
}
