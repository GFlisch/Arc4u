namespace Arc4u.OAuth2.Security.Principal;

/// <summary>
/// Return the Upn and Password implemented with the IUsernamePasswordProvider.
/// </summary>
public class CredentialsResult
{
    public bool CredentialsEntered { get; }
    public string Upn { get; }
    public string Password { get; }
    public CredentialsResult(bool credentialsEntered, string upn = null, string passwd = null)
    {
        CredentialsEntered = credentialsEntered;
        Upn = upn;
        Password = passwd;
    }
}
