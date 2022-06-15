using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Token
{
    [Export(NullTokenProvider.ProviderName, typeof(ITokenProvider)), Shared]
    public class NullTokenProvider : ITokenProvider
    {
        public NullTokenProvider(ILogger<NullTokenProvider> logger)
        {
            _logger = logger;
        }

        public const string ProviderName = "null";

        private readonly ILogger<NullTokenProvider> _logger;

        public Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            _logger.Technical().System("Null token provide is invoked.").Log();
            return Task.FromResult<TokenInfo>(null);
        }

        public void SignOut(IKeyValueSettings settings)
        {
            _logger.Technical().System("Null token provider doesn't do anything.").Log();
            return;
        }
    }
}

