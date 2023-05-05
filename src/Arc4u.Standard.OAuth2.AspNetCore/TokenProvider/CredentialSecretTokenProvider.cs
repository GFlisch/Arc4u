using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Arc4u.Standard.OAuth2;
using Arc4u.Standard.OAuth2.Middleware;
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

        var messages = new Messages();
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
        var basicSettings = CredentialSecretTokenProvider.Transform(options =>
        {
            options.ProviderId = settings.Values[BasicProviderId];
            options.ClientId = settings.Values[TokenKeys.ClientIdKey];
            options.Audience = settings.Values[TokenKeys.Audience];
            options.Authority = settings.Values[TokenKeys.AuthorityKey];
            options.Scope = settings.Values[TokenKeys.Scope];
            options.AuthenticationType = settings.Values[TokenKeys.AuthenticationTypeKey];
        });

        return await credentialToken.GetTokenAsync(basicSettings, credential).ConfigureAwait(false);
    }

    public void SignOut(IKeyValueSettings settings)
    {
        throw new System.NotImplementedException();
    }

    private static SimpleKeyValueSettings Transform(Action<BasicSettingsOptions> options)
    {
        return options.BuildBasics();
    }
}
