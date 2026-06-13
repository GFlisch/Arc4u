using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.TokenProvider;

/// <summary>
/// <c>grant_type=client_credentials</c> scenario: a ClientId + ClientSecret pair, optionally with
/// extra request parameters such as the Logto.io <c>resource</c> indicator, carried through the
/// open <see cref="ClientTokenSettingsOptions.ExtraParameters"/> bag.
/// </summary>
public sealed class ClientCredentialsScenario : IClientTokenScenario
{
    /// <summary>The discriminator under which this scenario is registered.</summary>
    public const string Name = "ClientCredentials";

    /// <summary>The token provider (ProviderId) that consumes settings produced by this scenario.</summary>
    public const string ProviderName = "ClientCredentials";

    public void Validate(string optionKey, ClientTokenSettingsOptions options)
    {
        string? errors = null;

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            errors += $"[{optionKey}] ClientId must be filled." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            errors += $"[{optionKey}] ClientSecret must be filled." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(options.AuthenticationType))
        {
            errors += $"[{optionKey}] AuthenticationType must be filled." + System.Environment.NewLine;
        }

        if (errors is not null)
        {
            throw new ConfigurationException(errors);
        }
    }

    public void WriteTo(string optionKey, ClientTokenSettingsOptions options, SimpleKeyValueSettings settings)
    {
        settings.Add(TokenKeys.ProviderIdKey, ProviderName);
        ClientTokenScenarioHelper.WriteCommon(options, settings, optionKey);

        settings.Add(TokenKeys.ClientSecret, options.ClientSecret!);
    }
}
