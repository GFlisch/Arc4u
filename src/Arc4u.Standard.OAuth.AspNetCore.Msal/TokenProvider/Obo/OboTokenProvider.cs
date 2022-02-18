using Arc4u.OAuth2.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    public class OboTokenProvider : ITokenProvider
    {
        public Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            throw new NotImplementedException();
        }

        public void SignOut(IKeyValueSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
