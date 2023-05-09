namespace Arc4u.OAuth2.Options;

public class OnBehalfOfAuthenticationSectionOptions : OnBehalfOfAuthenticationOptions
{
    public string SettingsPath { get; set; } = "Authentication:OnBehalfOf:Settings";
}
