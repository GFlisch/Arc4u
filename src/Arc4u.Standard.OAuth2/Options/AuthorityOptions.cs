using System;

namespace Arc4u.OAuth2.Options;

public class AuthorityOptions
{
    public string Url { get; set; } = string.Empty;

    public string TokenEndpointV2 { get; set; } = "/oauth2/v2.0/token";

    public string TokenEndpointV1 { get; set; } = "/oauth2/token";

    public Uri GetEndpointV2()
    {
        var uriBuilder = new UriBuilder(Url);
        uriBuilder.Path += TokenEndpointV2;
        uriBuilder.Path = uriBuilder.Path.Replace("//", "/");

        return uriBuilder.Uri;
    }

    public Uri GetEndpointV1()
    {
        var uriBuilder = new UriBuilder(Url);
        uriBuilder.Path += TokenEndpointV1;
        uriBuilder.Path = uriBuilder.Path.Replace("//", "/");

        return uriBuilder.Uri;
    }
}
