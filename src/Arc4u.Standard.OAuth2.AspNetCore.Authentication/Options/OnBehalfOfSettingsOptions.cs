using Arc4u.OAuth2.TokenProviders;

namespace Arc4u.OAuth2.Options;

public class OnBehalfOfSettingsOptions
{
    public string ProviderId { get; set; } = AzureADOboTokenProvider.ProviderName;
    public string ClientId { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new List<string>();
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthenticationType { get; set; } = Constants.InjectAuthenticationType;

}
