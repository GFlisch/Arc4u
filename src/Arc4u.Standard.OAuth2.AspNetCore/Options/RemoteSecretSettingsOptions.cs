using Arc4u.OAuth2.TokenProvider;

namespace Arc4u.OAuth2.Options;
public class RemoteSecretSettingsOptions
{
    public string ProviderId { get; set; } = RemoteClientSecretTokenProvider.ProviderName;

    public string HeaderKey { get; set; } = "SecretKey";

    public string ClientSecret { get; set; }
}
