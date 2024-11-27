using Arc4u.Configuration;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.TokenProvider;

[Export(RemoteClientSecretTokenProvider.ProviderName, typeof(ITokenProvider)), Shared]
public class RemoteClientSecretTokenProvider : ITokenProvider
{
    public const string ProviderName = "RemoteSecret";

    public Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object? _)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Read the settings to extract the data:
        // HeaderKey => default = SecretKey
        // ClientSecret: the encrypted username/password.

        if (!settings.Values.ContainsKey(TokenKeys.ClientSecretHeader))
        {
            throw new ConfigurationException("Client secret Header is missing. Cannot process the request.");
        }

        if (!settings.Values.ContainsKey(TokenKeys.ClientSecret))
        {
            throw new ConfigurationException("Client secret is missing. Cannot process the request.");
        }

        var clientSecret = settings.Values[TokenKeys.ClientSecret];

        return Task.FromResult(new TokenInfo(settings.Values[TokenKeys.ClientSecretHeader], clientSecret, DateTime.UtcNow + TimeSpan.FromHours(1)));

    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
