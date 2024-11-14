using System;
using System.Net.Http;
#if NET8_0_OR_GREATER
using System.Net.Http.Json;
#else
using System.Text.Json;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Options;

public class AuthorityOptions
{
    /// <summary>
    /// For Serialization.
    /// </summary>
    public AuthorityOptions()
    {
    }

    public AuthorityOptions(Uri url, Uri? tokenEndpoint, Uri? metadataAddress)
    {
        Url = url;
        TokenEndpoint = tokenEndpoint;
        MetaDataAddress = metadataAddress;
    }

    public void SetData(Uri url, Uri? tokenEndpoint, Uri? metadataAddress)
    {
        Url = url;
        TokenEndpoint = tokenEndpoint;
        MetaDataAddress = metadataAddress;
    }

    /// <summary>
    /// Only define the properties that are being used when calling the well-known Oidc endpoint.
    /// </summary>
    private sealed class OpenIdConfiguration
    {
        public /*required*/ Uri token_endpoint { get; set; } = default!;
    }

    public /*required*/ Uri Url { get; set; } = default!;

    public Uri? TokenEndpoint { get; set; }

    public Uri? MetaDataAddress { get; set; }

    /// <summary>
    /// Will retrieve the v2.0 openid connect discovery.
    /// If you want another one, just provide the full metadata address!
    /// </summary>
    /// <returns>Thetoken_endpoint to use!</returns>
    public Uri GetMetaDataAddress()
    {
        if (MetaDataAddress == null)
        {
            var uriBuilder = new UriBuilder(Url);
            // See section 4 of https://openid.net/specs/openid-connect-discovery-1_0.html
            uriBuilder.Path += "/.well-known/openid-configuration";
            uriBuilder.Path = uriBuilder.Path.Replace("//", "/");
            MetaDataAddress = uriBuilder.Uri;
        }
        return MetaDataAddress;
    }


    public async Task<Uri> GetEndpointAsync(CancellationToken cancellationToken)
    {
        if (TokenEndpoint is null)
        {
            using var client = new HttpClient();
            OpenIdConfiguration? openIdConfiguration;
#if NET8_0_OR_GREATER
            openIdConfiguration = await client.GetFromJsonAsync<OpenIdConfiguration>(GetMetaDataAddress(), cancellationToken).ConfigureAwait(false);
#else
            using var stream = await client.GetStreamAsync(GetMetaDataAddress()).ConfigureAwait(false);
            openIdConfiguration = await JsonSerializer.DeserializeAsync<OpenIdConfiguration>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
#endif
            TokenEndpoint = openIdConfiguration!.token_endpoint;
        }
        return TokenEndpoint;
    }
}
