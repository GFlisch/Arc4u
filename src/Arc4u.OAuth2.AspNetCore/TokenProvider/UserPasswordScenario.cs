using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.TokenProvider;

/// <summary>
/// <c>grant_type=password</c> style scenario: a user/password (or its base64 <c>Credential</c>
/// encoding), forwarded to the STS through the <see cref="CredentialSecretTokenProvider"/> and its
/// inner <see cref="ClientTokenSettingsOptions.BasicProviderId"/>. Also supports the
/// user+password+clientSecret variant.
/// </summary>
public sealed class UserPasswordScenario : IClientTokenScenario
{
    /// <summary>The discriminator under which this scenario is registered.</summary>
    public const string Name = "UserPassword";

    public void Validate(string optionKey, ClientTokenSettingsOptions options)
    {
        string? errors = null;

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            errors += $"[{optionKey}] ClientId must be filled." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.AuthenticationType))
        {
            errors += $"[{optionKey}] AuthenticationType must be filled." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.BasicProviderId))
        {
            errors += $"[{optionKey}] BasicProviderId must be filled." + System.Environment.NewLine;
        }

        var hasUser = !string.IsNullOrWhiteSpace(options.User);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);
        var hasCredential = !string.IsNullOrWhiteSpace(options.Credential);

        if (!hasUser && !hasPassword && !hasCredential)
        {
            errors += $"[{optionKey}] User/Password or Credential must be filled." + System.Environment.NewLine;
        }

        if (hasPassword && hasCredential)
        {
            errors += $"[{optionKey}] Password and Credential cannot be filled at the same time." + System.Environment.NewLine;
        }

        if (hasPassword && !hasUser)
        {
            errors += $"[{optionKey}] User must be filled when Password is used." + System.Environment.NewLine;
        }

        if (errors is not null)
        {
            throw new ConfigurationException(errors);
        }
    }

    public void WriteTo(string optionKey, ClientTokenSettingsOptions options, SimpleKeyValueSettings settings)
    {
        settings.Add(TokenKeys.ProviderIdKey, CredentialSecretTokenProvider.ProviderName);
        ClientTokenScenarioHelper.WriteCommon(options, settings, optionKey);

        settings.AddifNotNullOrEmpty("User", options.User);
        settings.AddifNotNullOrEmpty("Password", options.Password);
        settings.AddifNotNullOrEmpty("Credential", options.Credential);
        // Some STS expect a client secret in combination with the user/password (kind of 2FA).
        settings.AddifNotNullOrEmpty(TokenKeys.ClientSecret, options.ClientSecret);
        settings.Add("BasicProviderId", options.BasicProviderId);
    }
}
