using System.Collections.Generic;

namespace Arc4u.Standard.OAuth2.Middleware;

public class ForceOpenIdMiddleWareOptions
{
    public List<string> ForceAuthenticationForPaths { get; set; } = new();

    /// <summary>
    /// The url to redirect to the authority. If not set, the current url is used.
    /// </summary>
    public string RedirectUrlForAuthority { get; set; } = string.Empty;
}

