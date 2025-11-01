using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arc4u.OAuth2.Options;

[JsonSerializable(typeof(OpenIdConfiguration))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class OpenIdConfigurationJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Only define the properties that are being used when calling the well-known Oidc endpoint.
/// </summary>
sealed class OpenIdConfiguration
{
    public required Uri Token_endpoint { get; set; }

    public required Uri issuer { get; set; }

    public Uri? access_token_issuer { get; set; }

    public Uri? end_session_endpoint { get; set; }
}

public class AuthorityOptions
{
    /// <summary>
    /// For Serialization.
    /// </summary>
    public AuthorityOptions()
    {
        Url = new Uri("about:blank");
    }

    public AuthorityOptions(Uri url, Uri? tokenEndpoint, Uri? issuer, Uri? metadataAddress)
    {
        Url = url;
        TokenEndpoint = tokenEndpoint;
        MetaDataAddress = metadataAddress;
        Issuer = issuer;
    }

    public void SetData(Uri url, Uri? tokenEndpoint, Uri? issuer, Uri? metadataAddress)
    {
        Url = url;
        TokenEndpoint = tokenEndpoint;
        MetaDataAddress = metadataAddress;
        Issuer = issuer;
    }

    public Uri Url { get; set; }

    public Uri? TokenEndpoint { get; set; }

    public Uri? Issuer { get; set; }

    public Uri? MetaDataAddress { get; set; }

    public Uri? EndSessionEndpoint { get; set; }

    public TimeSpan? RetryInterval { get; set; }

    /// <summary>
    /// Will retrieve the v2.0 openid connect discovery.
    /// If you want another one, provide the full metadata address!
    /// </summary>
    /// <returns>The token_endpoint to use!</returns>
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

    public bool IsLocalHost => Url.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                               Url.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                               Url.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);

    public string GetRelativeMetaDataAddress()
    {
        // some metadata addresses do not follow the standard.
        // Remove from the MetaDataAddress the Url path!
        if (MetaDataAddress == null)
        {
            GetMetaDataAddress();
        }
        return MetaDataAddress!.ToString().Replace(Url.ToString(), string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Uri> GetEndpointAsync(CancellationToken cancellationToken)
    {
        if (TokenEndpoint is null)
        {
            using var client = new HttpClient();

            var stream = await client.GetStreamAsync(GetMetaDataAddress(), cancellationToken).ConfigureAwait(false);
            var openIdConfiguration = JsonSerializer.Deserialize(stream, OpenIdConfigurationJsonContext.Default.OpenIdConfiguration);

            EndSessionEndpoint = openIdConfiguration!.end_session_endpoint;
            TokenEndpoint = openIdConfiguration!.Token_endpoint;
            Issuer = openIdConfiguration.access_token_issuer ?? openIdConfiguration.issuer;
        }
        return TokenEndpoint;
    }

    public async Task<Uri> GetEndSessionEndpointAsync(CancellationToken cancellationToken)
    {
        if (EndSessionEndpoint is null)
        {
            using var client = new HttpClient();

            var stream = await client.GetStreamAsync(GetMetaDataAddress(), cancellationToken).ConfigureAwait(false);
            var openIdConfiguration = JsonSerializer.Deserialize(stream, OpenIdConfigurationJsonContext.Default.OpenIdConfiguration);

            EndSessionEndpoint = openIdConfiguration!.end_session_endpoint;
            TokenEndpoint = openIdConfiguration!.Token_endpoint;
            Issuer = openIdConfiguration.access_token_issuer ?? openIdConfiguration.issuer;
        }
        return EndSessionEndpoint;
    }
    public async Task<Uri> GetIssuerAsync(CancellationToken cancellationToken)
    {
        if (Issuer is null)
        {
            using var client = new HttpClient();

            var stream = await client.GetStreamAsync(GetMetaDataAddress(), cancellationToken).ConfigureAwait(false);
            var openIdConfiguration = JsonSerializer.Deserialize(stream, OpenIdConfigurationJsonContext.Default.OpenIdConfiguration);

            EndSessionEndpoint = openIdConfiguration!.end_session_endpoint;
            TokenEndpoint = openIdConfiguration!.Token_endpoint;
            Issuer = openIdConfiguration.access_token_issuer ?? openIdConfiguration.issuer;
        }
        return Issuer;
    }
}
