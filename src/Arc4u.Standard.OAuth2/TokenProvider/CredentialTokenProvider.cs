using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.TokenProvider;

[Export(CredentialTokenProvider.ProviderName, typeof(ICredentialTokenProvider)), Shared]
public class CredentialTokenProvider : ICredentialTokenProvider
{
    public const string ProviderName = "CredentialDirect";

    private readonly ILogger<CredentialTokenProvider> _logger;

    public CredentialTokenProvider(ILogger<CredentialTokenProvider> logger)
    {
        _logger = logger;
    }

    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var messages = GetContext(settings, out var clientId, out var authority, out var audience, out var scope);

        if (string.IsNullOrWhiteSpace(credential.Upn))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Error, "No Username is provided."));
        }

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, "No password is provided."));
        }

        messages.LogAndThrowIfNecessary(_logger);
        messages.Clear();

        // no cache, do a direct call on every calls.
        _logger.Technical().System($"Call STS: {authority} for user: {credential.Upn}").Log();
        return await GetTokenInfoAsync(audience, clientId, authority, scope, credential.Upn, credential.Password).ConfigureAwait(false);

    }

    private Messages GetContext(IKeyValueSettings settings, out string clientId, out string authority, out string audience, out string scope)
    {
        // Check the information.
        var messages = new Messages();

        if (null == settings)
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                                     ServiceModel.MessageType.Error,
                                     "Settings parameter cannot be null."));
            clientId = string.Empty;
            authority = string.Empty;
            audience = string.Empty;
            scope = string.Empty;

            return messages;
        }

        // Valdate arguments.
        if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                     ServiceModel.MessageType.Error,
                     "Authority is missing. Cannot process the request."));
        }

        if (!settings.Values.ContainsKey(TokenKeys.ClientIdKey))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                     ServiceModel.MessageType.Error,
                     "ClientId is missing. Cannot process the request."));
        }

        if (!settings.Values.ContainsKey(TokenKeys.Audience))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                     ServiceModel.MessageType.Error,
                     "Audience is missing. Cannot process the request."));
        }

        _logger.Technical().System($"Creating an authentication context for the request.").Log();
        clientId = settings.Values[TokenKeys.ClientIdKey];
        audience = settings.Values[TokenKeys.Audience];
        authority = settings.Values[TokenKeys.AuthorityKey];
        // More for backward compatibility! We should throw an error message if scope is not defined...
        scope = !settings.Values.ContainsKey(TokenKeys.Scope) ? "openid" : settings.Values[TokenKeys.Scope];

        _logger.Technical().System($"ClientId = {clientId}.").Log();
        _logger.Technical().System($"Audience = {audience}.").Log();
        _logger.Technical().System($"Authority = {authority}.").Log();
        _logger.Technical().System($"Scope = {scope}.").Log();

        return messages;
    }

    private async Task<TokenInfo> GetTokenInfoAsync(string audience, string clientId, string authority, string scope, string upn, string pwd)
    {
        using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
        using (var client = new HttpClient(handler))
        {
            try
            {
                using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "resource", audience },
                        { "client_id", clientId },
                        { "grant_type", "password" },
                        { "username", upn.Trim() },
                        { "password", pwd.Trim() },
                        { "scope", scope }
                    }))

                // strictly speaking, we should obtain the Url for the token_endpoint from the /.well-known/openid-configuration endpoint, but we hard-code it here.

                using (var response = await client.PostAsync(authority + "/oauth2/token", content).ConfigureAwait(false))
                {
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

                                throw new AppException(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, tokenErrorCode, response.StatusCode.ToString(), $"{error_description} ({upn})"));
                            }
                        }
                        // if we can't write a better exception, issue a more general one
                        throw new AppException(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "TokenError", response.StatusCode.ToString(), $"{response.StatusCode} occured while requesting a token for {upn}"));
                    }

                    // at this point, we *must* have a valid Json response. The values are a mixture of strings and numbers, so we deserialize the JsonElements
                    var responseValues = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody)!;

                    _logger.Technical().System($"Token is received for user {upn}.").Log();

                    var accessToken = responseValues["access_token"].GetString()!;
                    var tokenType = "Bearer"; //  responseValues["token_type"]; Issue on Adfs return bearer and not Bearer (ok in AzureAD).
                    var expiresIn = responseValues["expires_in"].GetInt64();

                    // expiration lifetime in is in seconds.
                    var dateUtc = DateTime.UtcNow.AddSeconds(expiresIn);

                    _logger.Technical().System($"Access token will expire at {dateUtc} utc.").Log();

                    return new TokenInfo(tokenType, accessToken, dateUtc);
                }
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();
                throw new AppException(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "Trust", "Rejected", ex.Message));
            }
        }
    }
}
