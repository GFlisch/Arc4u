using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.TokenProvider.Scenarios;

/// <summary>
/// Shared projection logic for <see cref="IClientTokenScenario"/> implementations: writes the keys
/// common to every scenario so each scenario only has to emit its distinguishing keys. The
/// scenario-specific values and the extra-parameter passthrough are handled elsewhere.
/// </summary>
internal static class ClientTokenScenarioHelper
{
    public static void WriteCommon(ClientTokenSettingsOptions options, SimpleKeyValueSettings settings, string optionKey)
    {
        settings.Add(TokenKeys.AuthenticationTypeKey, options.AuthenticationType);
        settings.Add(TokenKeys.Scope, options.Scopes.Count > 0 ? string.Join(' ', options.Scopes) : "openid");

        // The authority is registered under the option key only when an Authority section is provided.
        if (options.Authority is not null)
        {
            settings.Add(TokenKeys.AuthorityKey, optionKey);
        }
    }
}
