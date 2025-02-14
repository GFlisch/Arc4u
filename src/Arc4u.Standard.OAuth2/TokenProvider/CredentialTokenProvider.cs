using System.Diagnostics;
using System.Text.Json;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TokenProvider;

[Export(ProviderName, typeof(ICredentialTokenProvider)), Shared]
public class CredentialTokenProvider : ICredentialTokenProvider
{
    public const string ProviderName = "CredentialDirect";

    private static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromSeconds(90);

    private readonly ILogger<CredentialTokenProvider> _logger;
    private readonly IOptionsMonitor<AuthorityOptions> _authorityOptions;

    public CredentialTokenProvider(ILogger<CredentialTokenProvider> logger, IOptionsMonitor<AuthorityOptions> authorityOptions)
    {
        _logger = logger;
        _authorityOptions = authorityOptions;
    }

    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var messages = GetContext(settings, out var clientId, out var authority, out var scope, out var clientSecret);

        _logger.Technical().LogDebug($"ClientId = {clientId}.");
        _logger.Technical().LogDebug($"Scope = {scope}.");

        if (string.IsNullOrWhiteSpace(credential.Upn))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "No Username is provided."));
        }

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Warning, "No password is provided."));
        }

        messages.LogAndThrowIfNecessary(_logger);
        messages.Clear();

        var tokenEndpoint = await authority!.GetEndpointAsync(CancellationToken.None).ConfigureAwait(false);

        _logger.Technical().LogDebug($"Authority = {tokenEndpoint}.");   // this should be called TokenEndpoint in the logs...

        // no cache, do a direct call on every calls.
        _logger.Technical().Debug($"Call STS: {authority} for user: {credential.Upn}").Log();
        return await GetTokenInfoAsync(clientSecret, clientId, tokenEndpoint, scope, credential.Upn!, credential.Password!, authority.RetryInterval ?? DefaultRetryInterval).ConfigureAwait(false);

    }

    private Messages GetContext(IKeyValueSettings settings, out string clientId, out AuthorityOptions? authority, out string scope, out string clientSecret)
    {
        // Check the information.
        var messages = new Messages();

        if (null == settings)
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                                     MessageType.Error,
                                     "Settings parameter cannot be null."));
            clientId = string.Empty;
            authority = null;
            scope = string.Empty;
            clientSecret = string.Empty;

            return messages;
        }

        // Valdate arguments.
        if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
        {
            authority = _authorityOptions.Get("Default");
        }
        else
        {
            authority = _authorityOptions.Get(settings.Values[TokenKeys.AuthorityKey]);
        }

        if (!settings.Values.ContainsKey(TokenKeys.ClientIdKey))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                     MessageType.Error,
                     "ClientId is missing. Cannot process the request."));
        }
        _logger.Technical().Debug($"Creating an authentication context for the request.").Log();
        clientId = settings.Values[TokenKeys.ClientIdKey];
        clientSecret = settings.Values.ContainsKey(TokenKeys.ClientSecret) ? settings.Values[TokenKeys.ClientSecret] : string.Empty;
        // More for backward compatibility! We should throw an error message if scope is not defined...
        scope = !settings.Values.ContainsKey(TokenKeys.Scope) ? "openid" : settings.Values[TokenKeys.Scope];
        return messages;
    }

    /// <summary>
    /// If a request could not be made, we need to track the number of retries and the delay between them.
    /// This is used in the logs and in the exception message, to allow for better diagnostics.
    /// </summary>
    private sealed class RetryInformation
    {
        public int RetryCount;
        public TimeSpan Delay;

        public override string ToString()
        {
            return RetryCount == 0 ? "No retries" : $"Retried {RetryCount} times over {Delay}";
        }
    }

    /// <summary>
    /// We do this without Polly since this will need to be integrated in Arc4u at some point.
    /// </summary>
    private sealed class HttpRetryMessageHandler : DelegatingHandler
    {
        private readonly TimeSpan _retryInterval;
        private readonly RetryInformation _retryInformation;

        public HttpRetryMessageHandler(HttpMessageHandler innerHandler, TimeSpan retryInterval, RetryInformation retryInformation)
            : base(innerHandler)
        {
            _retryInterval = retryInterval;
            _retryInformation = retryInformation;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _retryInformation.RetryCount = 0;
            var sw = Stopwatch.StartNew();
            var random = new Random();
            for (; ; )
            {
                HttpResponseMessage? response = null;
                var delay = TimeSpan.FromMilliseconds(Math.Pow(4, random.Next(1, 6)));
                try
                {
                    response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    // we always return the response if it is successful or we have reached the retry timespan.
                    if (response.IsSuccessStatusCode || sw.Elapsed >= _retryInterval)
                    {
                        sw.Stop();
                        _retryInformation.Delay = sw.Elapsed;
                        return response;
                    }

                    // Use "Retry-After" value, if available. Typically, this is sent with either a 503 (Service Unavailable) or 429 (Too Many Requests):
                    // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Retry-After
                    if (response.Headers.RetryAfter is not null)
                    {
                        if (response.Headers.RetryAfter.Date.HasValue)
                        {
                            delay = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                        }
                        else if (response.Headers.RetryAfter.Delta.HasValue)
                        {
                            delay = response.Headers.RetryAfter.Delta.Value;
                        }
                    }

                    response.Dispose();
                }
                catch when (sw.Elapsed < _retryInterval)
                {
                    // Ignore the exception if we have retries left. But we need to dispose the response even though it's most likely null.
                    response?.Dispose();
                }
                catch
                {
                    sw.Stop();
                    _retryInformation.Delay = sw.Elapsed;
                }
                ++_retryInformation.RetryCount;
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<TokenInfo> GetTokenInfoAsync(string? clientSecret, string clientId, Uri tokenEndpoint, string scope, string upn, string pwd, TimeSpan retryInterval)
    {
        var retryInformation = new RetryInformation();
        using var handler = new HttpClientHandler { UseDefaultCredentials = true };
        using var client = new HttpClient(new HttpRetryMessageHandler(handler, retryInterval, retryInformation));

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

                var logger = _logger.Technical().Error($"Token endpoint for {upn} returned {response.StatusCode} after {retryInformation}: {loggedResponseBody}");

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
                    logger.Log();
                }
                else
                {
                    // add the key/values are properties of the structured log
                    foreach (var kv in dictionary)
                    {
                        logger.Add(kv.Key, kv.Value);
                    }
                    logger.Log();

                    if (dictionary.TryGetValue("error", out var tokenErrorCode))
                    {
                        // error description is optional. So is error_uri, but we don't use it.
                        string? error_description;
                        if (!dictionary.TryGetValue("error_description", out error_description))
                        {
                            error_description = "No error description";
                        }

                        throw new AppException(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, tokenErrorCode, response.StatusCode.ToString(), $"{error_description} ({upn}, {retryInformation}"));
                    }
                }
                // if we can't write a better exception, issue a more general one
                throw new AppException(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "TokenError", response.StatusCode.ToString(), $"{response.StatusCode} occured while requesting a token for {upn} ({retryInformation})"));
            }

            // at this point, we *must* have a valid Json response. The values are a mixture of strings and numbers, so we deserialize the JsonElements
            var responseValues = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody)!;

            _logger.Technical().LogDebug($"Token is received for user {upn} after {retryInformation}.");

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
            _logger.Technical().Exception(ex).Log();
            throw new AppException(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "Trust", "Rejected", ex.Message));
        }
    }
}
