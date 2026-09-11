namespace Arc4u.OAuth2.Options
{
    public class HybridAuthenticationOptions : OidcAuthenticationOptions
    {
        public Action<OAuth2SettingsOption> OAuth2SettingsOptions { get; set; } = default!;
    }
}
