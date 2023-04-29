using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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

    private async Task<TokenInfo> GetTokenInfoAsync(string audience, string clientId, string authority,string scope, string upn, string pwd)
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


                using (var response = await client.PostAsync(authority + "/oauth2/token", content).ConfigureAwait(true))
                {
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        _logger.Technical().Error("A bad request was received.").Log();
                        JObject error = JObject.Parse(responseBody);
                        if (error.ContainsKey("error"))
                        {
                            if (error["error"].Value<String>().Equals("invalid_grant", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var message = error.ContainsKey("error_description") ? error["error_description"].Value<String>() : "No error descrption.";
                                throw new AppException(new Arc4u.ServiceModel.Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "invalid_grant", "Rejected", message));
                            }
                            if (error["error"].Value<String>().Equals("unauthorized_client", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var message = error.ContainsKey("error_description") ? error["error_description"].Value<String>() : "No error descrption.";
                                throw new AppException(new Arc4u.ServiceModel.Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "invalid_grant", "unauthorized_client", message));
                            }

                        }

                        throw new AppException(new Arc4u.ServiceModel.Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "Error", "Rejected", "Unknown"));
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.Technical().Error("You are unauthorized.").Log();
                        JObject error = JObject.Parse(responseBody);

                        var message = error.ContainsKey("error_description") ? error["error_description"].Value<String>() : "No error descrption.";
                        throw new AppException(new Arc4u.ServiceModel.Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "unauthorized", "unauthorized_client", message));

                    }

                    var responseValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                    _logger.Technical().System($"Token is received for user {upn}.").Log();

                    var accessToken = responseValues["access_token"];
                    var tokenType = "Bearer"; //  responseValues["token_type"]; Issue on Adfs return bearer and not Bearer (ok in AzureAD).
                    var expiresIn = responseValues["expires_in"];

                    // expires in is in ms.
                    Int64.TryParse(expiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var offset);
                    var dateUtc = DateTime.UtcNow.AddSeconds(offset);

                    _logger.Technical().System($"Access token will expire at {dateUtc} utc.").Log();

                    return new TokenInfo(tokenType, accessToken, dateUtc);
                }
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();
                throw new AppException(new Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "Trust", "Rejected", ex.Message));
            }
        }
    }

}
