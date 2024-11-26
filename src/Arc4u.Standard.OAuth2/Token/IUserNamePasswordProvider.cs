using Arc4u.OAuth2.Security.Principal;

namespace Arc4u.OAuth2.Token;

public delegate Task<bool> CheckCredentialsAsync(string upn, string password);

public interface IUserNamePasswordProvider
{
    /// <summary>
    /// Used to provide the user name and password.
    /// </summary>
    /// <param name="upn">The current know upn of the user. Can be null if unknown.</param>
    /// <returns>The <see cref="CredentialsResult"/> containing the upn and password of the user.</returns>
    Task<CredentialsResult> GetCredentials(string upn, CheckCredentialsAsync checkCredentials);
}
