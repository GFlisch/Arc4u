using Arc4u.Diagnostics;
using System.Composition;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Token
{
    [Export(NullTokenProvider.ProviderName, typeof(ITokenProvider)), Shared]
    public class NullTokenProvider : ITokenProvider
    {

        public const string ProviderName = "null";

        public Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            Logger.Technical.From<NullTokenProvider>().System("Null token provide is invoked.").Log();
            return Task.FromResult<TokenInfo>(null);
        }

        public void SignOut(IKeyValueSettings settings)
        {
            Logger.Technical.From<NullTokenProvider>().System("Null token provider doesn't do anything.").Log();
            return;
        }
    }
}

