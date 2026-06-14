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

    /// <summary>
    /// Shared projection for the user/password scenarios (<see cref="UserPasswordScenario"/> and
    /// <see cref="BasicScenario"/>): both target the cache-backed
    /// <see cref="CredentialTokenCacheTokenProvider"/> and emit the same <c>User</c>/<c>Password</c>
    /// shape, differing only in how the credential is sourced.
    /// </summary>
    public static void WriteUserPassword(ClientTokenSettingsOptions options, SimpleKeyValueSettings settings, string optionKey,
        string clientId, string user, string password, string? clientSecret)
    {
        settings.Add(TokenKeys.ProviderIdKey, CredentialTokenCacheTokenProvider.ProviderName);
        WriteCommon(options, settings, optionKey);

        settings.Add(TokenKeys.ClientIdKey, clientId);
        settings.Add("User", user);
        settings.Add("Password", password);
        // Some STS expect a client secret in combination with the user/password (kind of 2FA).
        settings.AddifNotNullOrEmpty(TokenKeys.ClientSecret, clientSecret);
    }
}
