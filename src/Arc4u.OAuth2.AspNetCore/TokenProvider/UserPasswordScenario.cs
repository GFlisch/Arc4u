using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.TokenProvider;

/// <summary>
/// <c>grant_type=password</c> style scenario: a user/password (or its base64 <c>Credential</c>
/// encoding), forwarded to the STS through the <see cref="CredentialSecretTokenProvider"/> and its
/// inner <c>BasicProviderId</c>. Also supports the user+password+clientSecret variant.
/// <para>Reads its values from the <see cref="ClientTokenSettingsOptions.Settings"/> bag.</para>
/// </summary>
public sealed class UserPasswordScenario : IClientTokenScenario
{
    /// <summary>The discriminator under which this scenario is registered.</summary>
    public const string Name = "UserPassword";

    private const string ClientId = "ClientId";
    private const string User = "User";
    private const string Password = "Password";
    private const string Credential = "Credential";
    private const string ClientSecret = "ClientSecret";
    private const string BasicProviderId = "BasicProviderId";

    public IReadOnlyCollection<string> KnownKeys { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ClientId, User, Password, Credential, ClientSecret, BasicProviderId };

    public void Validate(string optionKey, ClientTokenSettingsOptions options)
    {
        new ClientTokenValidation(optionKey)
            .Require(options.Settings, ClientId)
            .AtLeastOne(options.Settings, User, Password, Credential)
            .MutuallyExclusive(options.Settings, Password, Credential)
            .RequiredTogether(options.Settings, Password, User)
            .ThrowIfInvalid();
    }

    public void WriteTo(string optionKey, ClientTokenSettingsOptions options, SimpleKeyValueSettings settings)
    {
        var s = options.Settings;

        settings.Add(TokenKeys.ProviderIdKey, CredentialSecretTokenProvider.ProviderName);
        ClientTokenScenarioHelper.WriteCommon(options, settings, optionKey);

        settings.Add(TokenKeys.ClientIdKey, s[ClientId]);
        settings.AddifNotNullOrEmpty(User, Get(s, User));
        settings.AddifNotNullOrEmpty(Password, Get(s, Password));
        settings.AddifNotNullOrEmpty(Credential, Get(s, Credential));
        // Some STS expect a client secret in combination with the user/password (kind of 2FA).
        settings.AddifNotNullOrEmpty(TokenKeys.ClientSecret, Get(s, ClientSecret));
        // The inner credential provider; defaults to the cache-backed credential provider when omitted.
        settings.Add(BasicProviderId, Get(s, BasicProviderId) ?? CredentialTokenCacheTokenProvider.ProviderName);
    }

    private static string? Get(IReadOnlyDictionary<string, string> settings, string key)
        => settings.TryGetValue(key, out var value) ? value : null;
}
