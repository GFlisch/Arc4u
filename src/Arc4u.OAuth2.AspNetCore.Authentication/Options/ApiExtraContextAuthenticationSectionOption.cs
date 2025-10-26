namespace Arc4u.OAuth2.Options;

public class ApiExtraContextAuthenticationSectionOption
{
    /// <summary>
    /// Define the path
    /// </summary>
    public string AuthorizationEndpointSectionPath { get; set; } = "Authentication:OpenId.Settings:AuthorizationEndpoint";

    public string TokenEndpointSectionPath { get; set; } = "Authentication:OpenId.Settings:TokenEndpoint";
}
