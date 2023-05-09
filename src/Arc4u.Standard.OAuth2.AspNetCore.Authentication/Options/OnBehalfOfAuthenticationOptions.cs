using System;

namespace Arc4u.OAuth2.Options;

public class OnBehalfOfAuthenticationOptions
{
    public string Authority { get; set; } = string.Empty;

    public string TokenEndpoint { get; set; } = "/oauth2/v2.0/token";

    public Uri GetEndpoint()
    {
        var uriBuilder = new UriBuilder(Authority);
        uriBuilder.Path += TokenEndpoint;
        uriBuilder.Path = uriBuilder.Path.Replace("//", "/");
        return uriBuilder.Uri;
    }

}
