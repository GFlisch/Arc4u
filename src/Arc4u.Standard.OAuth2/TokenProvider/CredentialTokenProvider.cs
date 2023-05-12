using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TokenProvider;

[Export(CredentialTokenProvider.ProviderName, typeof(ICredentialTokenProvider)), Shared]
public class CredentialTokenProvider : ICredentialTokenProvider
{
    public const string ProviderName = "CredentialDirect";

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

        if (string.IsNullOrWhiteSpace(credential.Upn))
        {
            messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "No Username is provided."));
        }

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Warning, "No password is provided."));
        }

        messages.LogAndThrowIfNecessary(_logger);
        messages.Clear();

        // no cache, do a direct call on every calls.
        _logger.Technical().Debug($"Call STS: {authority} for user: {credential.Upn}").Log();
        return await GetTokenInfoAsync(clientSecret, clientId, authority, scope, credential.Upn, credential.Password).ConfigureAwait(false);

    }

    private Messages GetContext(IKeyValueSettings settings, out string clientId, out AuthorityOptions authority, out string scope, out string clientSecret)
    {
        // Check the information.
        var messages = new Messages();

        if (null == settings)
        {
            messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical,
                                     Arc4u.ServiceModel.MessageType.Error,
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
            messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical,
                     Arc4u.ServiceModel.MessageType.Error,
                     "ClientId is missing. Cannot process the request."));
        }

        if (!settings.Values.ContainsKey(TokenKeys.Audience))
        {
            messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical,
                     Arc4u.ServiceModel.MessageType.Error,
                     "Audience is missing. Cannot process the request."));
        }

        _logger.Technical().Debug($"Creating an authentication context for the request.").Log();
        clientId = settings.Values[TokenKeys.ClientIdKey];
        clientSecret = settings.Values.ContainsKey(TokenKeys.ClientSecret) ? settings.Values[TokenKeys.ClientSecret] : string.Empty;
        // More for backward compatibility! We should throw an error message if scope is not defined...
        scope = !settings.Values.ContainsKey(TokenKeys.Scope) ? "openid" : settings.Values[TokenKeys.Scope];

        _logger.Technical().Debug($"ClientId = {clientId}.").Log();
        _logger.Technical().Debug($"Authority = {authority.GetEndpoint()}.").Log();
        _logger.Technical().Debug($"Scope = {scope}.").Log();

        return messages;
    }

    private async Task<TokenInfo> GetTokenInfoAsync(string? clientSecret, string clientId, AuthorityOptions authority, string scope, string upn, string pwd)
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
                parameters.Add("client_secret", clientSecret);
            }
            using var content = new FormUrlEncodedContent(parameters);

            // strictly speaking, we should obtain the Url for the token_endpoint from the /.well-known/openid-configuration endpoint, but we hard-code it here.

            using var response = await client.PostAsync(authority.GetEndpoint(), content).ConfigureAwait(false);
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
                    loggedResponseBody = responseBody.Substring(0, MaxResponseBodyLength) + $"...(response truncated, {loggedResponseBody.Length} total characters)";
                }

                var logger = _logger.Technical().Error($"Token endpoint for {upn} returned {response.StatusCode}: {loggedResponseBody}");

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

                        throw new AppException(new Message(Arc4u.ServiceModel.MessageCategory.Technical, MessageType.Error, tokenErrorCode, response.StatusCode.ToString(), $"{error_description} ({upn})"));
                    }
                }
                // if we can't write a better exception, issue a more general one
                throw new AppException(new Message(Arc4u.ServiceModel.MessageCategory.Technical, MessageType.Error, "TokenError", response.StatusCode.ToString(), $"{response.StatusCode} occured while requesting a token for {upn}"));
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
            _logger.Technical().Exception(ex).Log();
            throw new AppException(new Message(Arc4u.ServiceModel.MessageCategory.Technical, MessageType.Error, "Trust", "Rejected", ex.Message));
        }
    }
}
