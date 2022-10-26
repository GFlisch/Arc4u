using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Arc4u.OAuth2.TokenProvider
{
    public abstract class AdalOboTokenProvider : ITokenProvider
    {
        public const string ProviderName = "Obo";

        private OAuthConfig oAuthConfig;
        protected readonly ILogger<AdalOboTokenProvider> _logger;
        protected readonly IServiceProvider Container;
        private readonly IApplicationContext _applicationContext;


        public AdalOboTokenProvider(OAuthConfig oAuthConfig, ILogger<AdalOboTokenProvider> logger, IServiceProvider container, IApplicationContext applicationContext)
        {
            _logger = logger;
            Container = container;
            this.oAuthConfig = oAuthConfig;
            _applicationContext = applicationContext;
        }

        // Request a token based on another one.
        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            var result = await AuthenticationResultAsync(settings);

            return result.ToTokenInfo();
        }


        private async Task<AuthenticationResult> AuthenticationResultAsync(IKeyValueSettings settings)
        {
            // Have a bootstrap token?
            var identity = _applicationContext.Principal.Identity as ClaimsIdentity;

            var accessToken = identity?.BootstrapContext?.ToString() ?? (await GetOpenIdTokenAsync(settings, identity)).AccessToken;

            var authContext = GetOAuthContext(settings, identity,
                                  out string serviceApplicationId,
                                  out ClientCredential credential);

            if (null == credential)
                throw new AppException("No client credential was created. This is needed for an on behalf of scenario.");

            _logger.Technical().LogDebug("Acquire a token on behal of.");

            return await authContext.AcquireTokenAsync(serviceApplicationId, credential, new UserAssertion(accessToken));
        }

        private async Task<TokenInfo> GetOpenIdTokenAsync(IKeyValueSettings settings, ClaimsIdentity identity)
        {
            // Check the information.
            var messages = new Messages();

            if (null == settings)
            {
                throw new AppException(new Message(ServiceModel.MessageCategory.Technical,
                                                   MessageType.Error,
                                                   "Settings parameter cannot be null."));
            }

            var settingsProviderName = settings.Values.ContainsKey("OpenIdSettingsReader") ? settings.Values["OpenIdSettingsReader"] : "OpenID";

            if (Container.TryGetService<IKeyValueSettings>(settingsProviderName, out var openIdSettings))
            {
                if (!openIdSettings.Values.ContainsKey(TokenKeys.ProviderIdKey))
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "No Provider defined in OpenId Settings."));
                else
                {
                    var tokenProviderName = openIdSettings.Values[TokenKeys.ProviderIdKey];

                    if (Container.TryGetService<ITokenProvider>(tokenProviderName, out var openIdTokenProvider))
                    {
                        return await openIdTokenProvider.GetTokenAsync(openIdSettings, identity);
                    }
                    else
                        messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, $"Cannot resolve a token provider with name {tokenProviderName}."));
                }
            }
            else
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, $"Cannot resolve the KeyValues settings with name {settingsProviderName}."));


            messages.LogAndThrowIfNecessary(_logger);
            messages.Clear();

            return null;
        }

        private AuthenticationContext GetOAuthContext(IKeyValueSettings settings,
                                                      ClaimsIdentity identity,
                                                      out string serviceApplicationId,
                                                      out ClientCredential credential)
        {
            // Check the information.
            var messages = new Messages();

            if (null == settings)
            {
                throw new AppException(new Message(ServiceModel.MessageCategory.Technical,
                                                   MessageType.Error,
                                                   "Settings parameter cannot be null."));
            }

            // Retrieve information from the OAuth section.
            var oauthSettingsName = settings.Values.ContainsKey("OAuthSettingsReader") ? settings.Values["OAuthSettingsReader"] : "OAuth";

            if (Container.TryGetService<IKeyValueSettings>(oauthSettingsName, out var oauthSettings))
            {
                // Valdate arguments.
                if (!oauthSettings.Values.ContainsKey(TokenKeys.AuthorityKey))
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "Authority is missing. Cannot process the request."));
                if (!oauthSettings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "ApplicationId is missing. Cannot process the request."));
                if (!oauthSettings.Values.ContainsKey(TokenKeys.ApplicationKey))
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "ApplicationKey is missing. Cannot process the request."));

                messages.LogAndThrowIfNecessary(_logger);
                messages.Clear();


                _logger.Technical().LogDebug($"Creating an authentication context for the request.");
                serviceApplicationId = settings.Values[TokenKeys.ServiceApplicationIdKey];
                var authority = oauthSettings.Values[TokenKeys.AuthorityKey];

                _logger.Technical().LogDebug($"ServiceApplicationId = {serviceApplicationId}.");
                _logger.Technical().LogDebug($"Authority = {authority}.");

                if (!oauthSettings.Values.TryGetValue(TokenKeys.ClientIdKey, out var clientId)) // ClientId exists only for AzureAD.
                    clientId = oauthSettings.Values[TokenKeys.ServiceApplicationIdKey];

                if (oauthSettings.Values.TryGetValue(TokenKeys.ApplicationKey, out var applicationKey))
                {
                    credential = new ClientCredential(clientId, applicationKey);
                }
                else
                {
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "No application key is defined to identify the application in the STS."));
                    credential = null;
                }

                var userObjectId = oAuthConfig.UserClaimIdentifier(identity);
                if (String.IsNullOrWhiteSpace(userObjectId))
                {
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "No user object identifier is found in the claims collection to identify the user."));
                }

                messages.LogAndThrowIfNecessary(_logger);

                var authContext = CreateAuthenticationContext(authority, serviceApplicationId + identity.AuthenticationType + userObjectId);
                _logger.Technical().LogDebug("Created the authentication context.");

                return authContext;
            }

            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, $"No OAuth Settings file found: {oauthSettingsName}."));
            messages.LogAndThrowIfNecessary(_logger);

            serviceApplicationId = string.Empty;
            credential = null;

            return null;
        }

        protected abstract AuthenticationContext CreateAuthenticationContext(String authority, string cacheIdentifier);


        public void SignOut(IKeyValueSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}