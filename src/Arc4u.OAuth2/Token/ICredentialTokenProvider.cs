using Arc4u.OAuth2.Security.Principal;

namespace Arc4u.OAuth2.Token;

public interface ICredentialTokenProvider
{
    Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential);
}
