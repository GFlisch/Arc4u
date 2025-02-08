using System.Text.Json;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.Results.Validation;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TokenProvider;

[Export(CredentialTokenProvider.ProviderName, typeof(ICredentialTokenProvider)), Shared]
public class CredentialTokenProvider(ILogger<CredentialTokenProvider> logger, IOptionsMonitor<AuthorityOptions> authorityOptions) : ICredentialTokenProvider
{
    public const string ProviderName = "CredentialDirect";

    private readonly ILogger<CredentialTokenProvider> _logger = logger;

    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var result = GetContext(settings, out var clientId, out var authority, out var scope, out var clientSecret);

        if (null == authority)
        {
            throw new NullReferenceException(nameof(authority));
        }
        var tokenEndpoint = await authority.GetEndpointAsync(CancellationToken.None).ConfigureAwait(false);

        _logger.Technical().Add("ClientId", clientId)
                           .Add("Scope", scope)
                           .Add("Authority", tokenEndpoint)
                           .LogGetEndpoint();

        if (string.IsNullOrWhiteSpace(credential.Upn))
        {
            result.WithValidationError("No Username is provided.");
        }

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            result.WithValidationError("No password is provided.");
        }

        result.LogIfFailed();

        // no cache, do a direct call on every calls.
        _logger.Technical().LogStsAndUser(authority.Url.ToString(), credential.Upn);
        return await GetTokenInfoAsync(clientSecret, clientId, tokenEndpoint, scope, credential.Upn!, credential.Password!).ConfigureAwait(false);

    }

    private Result GetContext(IKeyValueSettings settings, out string clientId, out AuthorityOptions? authority, out string scope, out string clientSecret)
    {
        // Check the information.
        if (null == settings)
        {
            clientId = string.Empty;
            authority = null;
            scope = string.Empty;
            clientSecret = string.Empty;

            return ValidationError.Create("Settings parameter cannot be null.");
        }

        // Valdate arguments.
        if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
        {
            authority = authorityOptions.Get("Default");
        }
        else
        {
            authority = authorityOptions.Get(settings.Values[TokenKeys.AuthorityKey]);
        }

        if (!settings.Values.ContainsKey(TokenKeys.ClientIdKey))
        {
            clientId = string.Empty;
            authority = null;
            scope = string.Empty;
            clientSecret = string.Empty;

            return ValidationError.Create("ClientId is missing. Cannot process the request.");
        }

        _logger.Technical().LogCreatingAuthenticationContext();
        clientId = settings.Values[TokenKeys.ClientIdKey];
        clientSecret = settings.Values.ContainsKey(TokenKeys.ClientSecret) ? settings.Values[TokenKeys.ClientSecret] : string.Empty;
        // More for backward compatibility! We should throw an error message if scope is not defined...
        scope = !settings.Values.ContainsKey(TokenKeys.Scope) ? "openid" : settings.Values[TokenKeys.Scope];
        return Result.Ok();
    }

    private async Task<Result<TokenInfo>> GetTokenInfoAsync(string? clientSecret, string clientId, Uri tokenEndpoint, string scope, string upn, string pwd)
    {
        using var handler = new HttpClientHandler { UseDefaultCredentials = true };
        using var client = new HttpClient(handler);
        try
        {
            var parameters = new Dictionary<string, string>
                    {
                        { "client_id", clientId },
                        { "grant_type", "password" },
                        { "username", upn.Trim() },
                        { "password", pwd.Trim() },
                        { "scope", scope }
                    };
            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                parameters.Add("client_secret", clientSecret!);
            }
            using var content = new FormUrlEncodedContent(parameters);

            using var response = await client.PostAsync(tokenEndpoint, content).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // We model this after https://www.rfc-editor.org/rfc/rfc6749#section-5.2
            // Identity providers usually reply with wither HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized, but in practice they can also reply with other
            // status codes that signal failure. We want to write as much information as possible in the logs in any case, but throw exceptions with minimal information for security.
            if (!response.IsSuccessStatusCode)
            {
                // To avoid overflowing the log with a large response body, we make sure that we limit its length. This should be a rare occurrence.
                var loggedResponseBody = responseBody;
                const int MaxResponseBodyLength = 256;  // arbitrary
                if (loggedResponseBody != null && loggedResponseBody.Length > MaxResponseBodyLength)
                {
                    loggedResponseBody = $"{responseBody.Substring(0, MaxResponseBodyLength)}...(response truncated, {loggedResponseBody.Length} total characters)";
                }

                var logger = _logger.Technical();

                // In case of error, any extra information should be in Json with string values, but we can't assume this is always the case!
                Dictionary<string, string>? dictionary = null;
                try
                {
                    dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
                }
                catch
                {
                    // the response body was not Json (it happens)
                }
                // we cannot any any more meaningful information to the log if this is not a dictionary
                if (dictionary == null)
                {
                    logger.LogCredentialToken(upn, response.StatusCode.ToString(), loggedResponseBody); ;
                }
                else
                {
                    // add the key/values are properties of the structured log
                    foreach (var kv in dictionary)
                    {
                        logger.Add(kv.Key, kv.Value);
                    }
                    logger.LogCredentialToken(upn, response.StatusCode.ToString(), loggedResponseBody);

                    if (dictionary.TryGetValue("error", out var tokenErrorCode))
                    {
                        // error description is optional. So is error_uri, but we don't use it.
                        if (!dictionary.TryGetValue("error_description", out var error_description))
                        {
                            error_description = "No error description";
                        }

                        return ValidationError.Create($"{error_description} ({upn})").WithCode(tokenErrorCode ?? string.Empty);
                    }
                }
                // if we can't write a better exception, issue a more general one
                return ValidationError.Create($"{response.StatusCode} occured while requesting a token for {upn}").WithCode("TokenError");
            }

            // at this point, we *must* have a valid Json response. The values are a mixture of strings and numbers, so we deserialize the JsonElements
            var responseValues = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody)!;

            _logger.Technical().LogDebug($"Token is received for user {upn}.");

            var accessToken = responseValues["access_token"].GetString()!;
            var tokenType = "Bearer"; //  responseValues["token_type"]; Issue on Adfs return bearer and not Bearer (ok in AzureAD).
                                      // expires in is in ms.
            var offset = responseValues["expires_in"].GetInt64();

            // expiration lifetime in is in seconds.
            var dateUtc = DateTime.UtcNow.AddSeconds(offset);

            _logger.Technical().LogDebug($"Access token will expire at {dateUtc} utc.");

            return new TokenInfo(tokenType, accessToken, dateUtc);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return ValidationError.Create(ex.Message).WithCode("Rejected");
        }
    }
}
