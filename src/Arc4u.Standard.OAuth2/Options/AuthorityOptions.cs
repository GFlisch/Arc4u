using System;

namespace Arc4u.OAuth2.Options;

public class AuthorityOptions
{
    public string Url { get; set; } = string.Empty;

    public string TokenEndpoint { get; set; } = "/oauth2/v2.0/token";

    public Uri GetEndpoint()
    {
        var uriBuilder = new UriBuilder(Url);
        uriBuilder.Path += TokenEndpoint;
        uriBuilder.Path = uriBuilder.Path.Replace("//", "/");

        return uriBuilder.Uri;
    }
}
