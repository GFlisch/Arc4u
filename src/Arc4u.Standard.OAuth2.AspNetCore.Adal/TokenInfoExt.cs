using Arc4u.OAuth2.Token;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace Arc4u.OAuth2
{
    public static class TokenInfoExt
    {
        public static TokenInfo ToTokenInfo(this AuthenticationResult authenticationResult)
        {
            if (null == authenticationResult) throw new ArgumentNullException(nameof(authenticationResult));

            return new TokenInfo(authenticationResult.AccessTokenType,
                                 authenticationResult.AccessToken,
                                 authenticationResult.ExpiresOn.UtcDateTime);
        }
    }
}
