using System.Security.Principal;

namespace Arc4u.Security.Principal;

/// <summary>
/// Authorization filler is an interface used to custom the framework by a specific authorization authority returning an authorization data.
/// </summary>
public interface IAuthorizationFiller
{
    /// <summary>
    /// Process returning the Authorization data for a specific authorization authority.
    /// </summary>
    /// <returns></returns>
    Authorization GetAuthorization(IIdentity identity);
}
