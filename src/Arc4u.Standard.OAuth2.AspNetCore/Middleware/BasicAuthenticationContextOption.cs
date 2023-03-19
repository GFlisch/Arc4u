using System;

namespace Arc4u.Standard.OAuth2.Middleware;

public class BasicAuthenticationContextOption
{
    public IKeyValueSettings Settings { get; set; }

    /// <summary>
    /// If no Domain exists, add the upn like @arc4u.net
    /// </summary>
    public String DefaultUpn { get; set; }
}
