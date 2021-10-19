using System.Threading.Tasks;

namespace Arc4u.OAuth2.Token
{
    public interface ITokenProvider
    {
        Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters);

        void SignOut(IKeyValueSettings settings);
    }
}
