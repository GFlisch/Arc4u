using Arc4u.OAuth2.TokenProviders;

namespace Arc4u.OAuth2.Options;

public class OnBehalfOfSettingsOptions
{
    public string ProviderId { get; set; } = AzureADOboTokenProvider.ProviderName;
    public string ClientId { get; set; } = default!;
    public List<string> Scopes { get; set; } = [];
    public string ClientSecret { get; set; } = default!;
    public string AuthenticationType { get; set; } = Constants.InjectAuthenticationType;

}
