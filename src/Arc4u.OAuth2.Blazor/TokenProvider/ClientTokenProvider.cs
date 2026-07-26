using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.TokenProvider;

[JsonSerializable(typeof(string))]
public partial class TokenJsonContext : JsonSerializerContext
{
}

[Export(ProviderName, typeof(ITokenProvider)), Shared]
public class ClientTokenProvider(IHttpClientFactory httpClientFactory, ILogger<ClientTokenProvider> logger)
    : ITokenProvider
{
    public const string ProviderName = "Client";

    private TokenInfo _token = new TokenInfo("bearer", string.Empty, DateTime.MinValue);

    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        if (_token.ExpiresOnUtc >= DateTime.UtcNow)
        {
            return Result.Ok(_token);
        }

        if (settings is null)
        {
            return Result.Fail("No settings provided.");
        }

        settings.Values.TryGetValue(TokenKeys.HttpClientName, out var httpClientName);
        if (string.IsNullOrWhiteSpace(httpClientName))
        {
            return Result.Fail("No HttpClientName provided.");
        }

        settings.Values.TryGetValue(TokenKeys.TokenRequestUrl, out var requestUrl);
        if (string.IsNullOrWhiteSpace(requestUrl))
        {
            return Result.Fail("No TokenRequestUrl provided.");
        }

        try
        {
            var httpClient = httpClientFactory.CreateClient(httpClientName);
            // Get the new token from the backend.
            var bearerToken = await httpClient.GetFromJsonAsync<string>(requestUrl, TokenJsonContext.Default.String)
                                                .ConfigureAwait(false);
            if (string.IsNullOrEmpty(bearerToken))
            {
                return Result.Fail($"Unable to get a token, HttpClientFactory({httpClientName}), endpoint is {httpClient.BaseAddress?.ToString()}.");
            }

            _token = new TokenInfo("Bearer", bearerToken);
            return Result.Ok(_token);
        }
        catch (Exception e)
        {
            logger.Technical().LogException(e);
        }

        return Result.Fail("Unable to get the token.");
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
