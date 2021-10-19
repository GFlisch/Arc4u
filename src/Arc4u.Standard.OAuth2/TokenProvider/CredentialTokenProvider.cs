using Arc4u.Diagnostics;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    [System.Composition.Export(CredentialTokenProvider.ProviderName, typeof(ICredentialTokenProvider)), System.Composition.Shared]
    public class CredentialTokenProvider : ICredentialTokenProvider
    {
        public const string ProviderName = "CredentialDirect";

        private readonly ILogger Logger;

        [ImportingConstructor]
        public CredentialTokenProvider(ILogger logger)
        {
            Logger = logger;
        }

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential)
        {
            var messages = GetContext(settings, out string clientId, out string authority, out string serviceApplicationId);

            if (String.IsNullOrWhiteSpace(credential.Upn))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, "No Username is provided."));

            if (String.IsNullOrWhiteSpace(credential.Password))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, "No password is provided."));

            messages.LogAndThrowIfNecessary(this);
            messages.Clear();

            // no cache, do a direct call on every calls.
            Logger.Technical().From<CredentialTokenProvider>().System($"Call STS: {authority} for user: {credential.Upn}").Log();
            return await GetTokenInfoAsync(serviceApplicationId, clientId, authority, credential.Upn, credential.Password);

        }

        private Messages GetContext(IKeyValueSettings settings, out string clientId, out string authority, out string serviceApplicationId)
        {
            // Check the information.
            var messages = new Messages();

            if (null == settings)
            {
                messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                                         ServiceModel.MessageType.Error,
                                         "Settings parameter cannot be null."));
                clientId = null;
                authority = null;
                serviceApplicationId = null;

                return messages;
            }

            // Valdate arguments.
            if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                         ServiceModel.MessageType.Error,
                         "Authority is missing. Cannot process the request."));
            if (!settings.Values.ContainsKey(TokenKeys.ClientIdKey))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                         ServiceModel.MessageType.Error,
                         "ClientId is missing. Cannot process the request."));
            if (!settings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                         ServiceModel.MessageType.Error,
                         "ApplicationId is missing. Cannot process the request."));

            Logger.Technical().From<CredentialTokenProvider>().System($"Creating an authentication context for the request.").Log();
            clientId = settings.Values[TokenKeys.ClientIdKey];
            serviceApplicationId = settings.Values[TokenKeys.ServiceApplicationIdKey];
            authority = settings.Values[TokenKeys.AuthorityKey];

            Logger.Technical().From<CredentialTokenProvider>().System($"ClientId = {clientId}.").Log();
            Logger.Technical().From<CredentialTokenProvider>().System($"ServiceApplicationId = {serviceApplicationId}.").Log();
            Logger.Technical().From<CredentialTokenProvider>().System($"Authority = {authority}.").Log();

            return messages;

        }


        private async Task<TokenInfo> GetTokenInfoAsync(string serviceId, string clientId, string authority, string upn, string pwd)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                try
                {
                    using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "resource", serviceId },
                        { "client_id", clientId },
                        { "grant_type", "password" },
                        { "username", upn.Trim() },
                        { "password", pwd.Trim() },
                        { "scope", "openid" }
                    }))


                    using (var response = await client.PostAsync(authority + "/oauth2/token", content).ConfigureAwait(true))
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;

                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Logger.Technical().From<CredentialTokenProvider>().Error("A bad request was received.").Log();
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
                            Logger.Technical().From<CredentialTokenProvider>().Error("You are unauthorized.").Log();
                            JObject error = JObject.Parse(responseBody);

                            var message = error.ContainsKey("error_description") ? error["error_description"].Value<String>() : "No error descrption.";
                            throw new AppException(new Arc4u.ServiceModel.Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "unauthorized", "unauthorized_client", message));

                        }

                        var responseValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                        Logger.Technical().From<CredentialTokenProvider>().System($"Token is received for user {upn}.").Log();

                        var accessToken = responseValues["access_token"];
                        var idToken = responseValues["id_token"];
                        var tokenType = "Bearer"; //  responseValues["token_type"]; Issue on Adfs return bearer and not Bearer (ok in AzureAD).
                        var expiresIn = responseValues["expires_in"];

                        // expires in is in ms.
                        Int64.TryParse(expiresIn, out var offset);
                        var dateUtc = DateTime.UtcNow.AddSeconds(offset);

                        Logger.Technical().From<CredentialTokenProvider>().System($"Access token will expire at {dateUtc} utc.").Log();

                        return new TokenInfo(tokenType, accessToken, idToken, dateUtc);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Technical().From<CredentialTokenProvider>().Exception(ex).Log();
                    throw new AppException(new Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "Trust", "Rejected", ex.Message));
                }
            }
        }

    }
}
