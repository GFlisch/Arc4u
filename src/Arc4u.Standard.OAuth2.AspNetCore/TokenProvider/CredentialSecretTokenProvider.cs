using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.TokenProvider;

[Export(CredentialSecretTokenProvider.ProviderName, typeof(ITokenProvider))]

public class CredentialSecretTokenProvider : ITokenProvider
{
    public const string ProviderName = "ClientSecret";

    private const string User = "User";
    private const string Password = "Password";
    private const string Credential = "Credential";
    private const string BasicProviderId = "BasicProviderId";

    public CredentialSecretTokenProvider(IContainerResolve container, ILogger<CredentialSecretTokenProvider> logger)
    {
        _containerResolve = container;
        _logger = logger;
    }

    private readonly IContainerResolve _containerResolve;
    private readonly ILogger<CredentialSecretTokenProvider> _logger;


    public async Task<TokenInfo> GetTokenAsync([DisallowNull] IKeyValueSettings settings, object _)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var credential = new CredentialsResult(false);

        if (settings.Values.ContainsKey(Password) && settings.Values.ContainsKey(Credential))
        {
            throw new ConfigurationException("User/Password or Credential must be filled in.");
        }

        if (settings.Values.ContainsKey(User) && settings.Values.ContainsKey(Password))
        {
            credential = new CredentialsResult(true, settings.Values[User], settings.Values[Password]);
        }
        else if (settings.Values.ContainsKey(Credential))
        {
            credential = new CredentialsResult(false).ExtractCredential(settings.Values[Credential], _logger);
        }

        if (!settings.Values.ContainsKey(BasicProviderId) || !_containerResolve.TryResolve<ICredentialTokenProvider>(settings.Values[BasicProviderId], out var credentialToken))
        {
            throw new ConfigurationException("No BasicProviderId exist to perform the request to the STS.");
        }

        // Switch to BasicToken provider.
        // AuthorityKey is the key used to retrieve the IOptionsMonitor<AuthorityOptions>()!
        var basicSettings = new SimpleKeyValueSettings();
        basicSettings.Add(TokenKeys.ProviderIdKey, settings.Values[BasicProviderId]);
        basicSettings.Add(TokenKeys.ClientIdKey, settings.Values[TokenKeys.ClientIdKey]);
        basicSettings.Add(TokenKeys.Scope, settings.Values[TokenKeys.Scope]);
        basicSettings.Add(TokenKeys.AuthenticationTypeKey, settings.Values[TokenKeys.AuthenticationTypeKey]);
        basicSettings.AddifNotNullOrEmpty(TokenKeys.AuthorityKey, settings.Values.ContainsKey(TokenKeys.AuthorityKey) ? settings.Values[TokenKeys.AuthorityKey] : string.Empty);

        return await credentialToken.GetTokenAsync(basicSettings, credential).ConfigureAwait(false);
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
