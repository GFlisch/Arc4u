using Arc4u.OAuth2.TokenProvider;

namespace Arc4u.Standard.OAuth2;

/// <summary>
/// Contains the user id/password of the user that will be used to retrieve the access token.
/// Or the information is filled via the user/password or via the credential as a basic authentication
/// user:password.
/// </summary>
public class SecretBasicSettingsOptions : BasicSettingsOptions
{
    /// <summary>
    /// Override the ProviderId to set this as ClientSecret
    /// </summary>
    public SecretBasicSettingsOptions()
    {
        ProviderId = "ClientSecret";
    }

    /// <summary>
    /// The basic providerId that will be used to perform the real call.
    /// </summary>
    public string BasicProviderId { get; set; } = CredentialTokenCacheTokenProvider.ProviderName;

    public string User { get; set; }

    public string Password { get; set; }

    public string Credential { get; set; }
}
