using System;
using System.Net.Http;
#if NET6_0_OR_GREATER
using System.Net.Http.Json;
#else
using System.Text.Json;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Options;

public class AuthorityOptions
{
    private Uri? _metaDataUri;
    private Uri? _tokenEndpoint;

    /// <summary>
    /// Only define the properties that are being used when calling the well-known Oidc endpoint.
    /// </summary>
    private class OpenIdConfiguration
    {
        public /*required*/ Uri token_endpoint { get; set; } = default!;
    }


    public string Url { get; set; } = string.Empty;

    public string? TokenEndpoint { get; set; }

    public string? MetaDataAddress { get; set; }


    public Uri GetMetaDataAddress()
    {
        if (_metaDataUri == null)
        {
            var uriBuilder = new UriBuilder(Url);
            // See section 4 of https://openid.net/specs/openid-connect-discovery-1_0.html
            uriBuilder.Path += "/.well-known/openid-configuration";
            uriBuilder.Path = uriBuilder.Path.Replace("//", "/");
            _metaDataUri = uriBuilder.Uri;
        }
        return _metaDataUri;
    }


    public async Task<Uri> GetEndpointAsync(CancellationToken cancellationToken)
    {
        if (_tokenEndpoint == null)
        {
            if (string.IsNullOrEmpty(TokenEndpoint))
            {
                using var client = new HttpClient();
                OpenIdConfiguration? openIdConfiguration;
#if NET6_0_OR_GREATER
                openIdConfiguration = await client.GetFromJsonAsync<OpenIdConfiguration>(GetMetaDataAddress(), cancellationToken).ConfigureAwait(false);
#else
                using var stream = await client.GetStreamAsync(GetMetaDataAddress()).ConfigureAwait(false);
                openIdConfiguration = await JsonSerializer.DeserializeAsync<OpenIdConfiguration>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
#endif
                _tokenEndpoint = openIdConfiguration!.token_endpoint;
            }
            else
            {
                var uriBuilder = new UriBuilder(Url);
                uriBuilder.Path += TokenEndpoint;
                uriBuilder.Path = uriBuilder.Path.Replace("//", "/");
                _tokenEndpoint = uriBuilder.Uri;
            }
        }
        return _tokenEndpoint;
    }
}
