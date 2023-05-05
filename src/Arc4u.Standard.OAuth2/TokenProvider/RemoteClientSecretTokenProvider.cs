using System;
using System.Threading;
using System.Threading.Tasks;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(RemoteClientSecretTokenProvider.ProviderName, typeof(ITokenProvider)), Shared]
    public class RemoteClientSecretTokenProvider : ITokenProvider
    {
        public const string ProviderName = "RemoteSecret";


        public Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings)
                throw new ArgumentNullException(nameof(settings));

            // Read the settings to extract the data:
            // HeaderKey => default = SecretKey
            // ClientSecret: the encrypted username/password.

            var headerKey = "SecretKey";
            if (settings.Values.ContainsKey(TokenKeys.ClientSecretHeader))
                headerKey = settings.Values[TokenKeys.ClientSecretHeader];

            if (!settings.Values.ContainsKey(TokenKeys.ClientSecret))
                throw new ArgumentException("Client secret is missing. Cannot process the request.");

            var clientSecret = settings.Values[TokenKeys.ClientSecret];

            return Task.FromResult(new TokenInfo(headerKey, clientSecret, DateTime.UtcNow + TimeSpan.FromHours(1)));

        }

        public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
        {
            // No real sign out.
#if NET6_0_OR_GREATER
            return ValueTask.CompletedTask;
#else
            return default;
#endif
        }
    }
}
