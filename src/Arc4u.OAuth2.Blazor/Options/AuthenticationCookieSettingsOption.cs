namespace Arc4u.Blazor.Options;

public class AuthenticationCookieSettingsOption
{
    /// <summary>
    /// The name of the HttpClient used to call the authentication server, from an instance of IHttpClientFactory.
    /// </summary>
    public string HttpClientName { get; set; } = "Authentication";

    /// <summary>
    /// The base uri of the Blazor SSR application.
    /// </summary>
    public Uri BaseUri { get; set; } = new("https://localhost");

    /// <summary>
    /// The url on the Blazor SSR application to call to get the token.
    /// </summary>
    public string TokenRequestUrl { get; set; } = "/authentication/token";

    /// <summary>
    /// The id of the provider performing the authentication.
    /// </summary>
    public string ProviderId { get; set; } = "Client";
}

