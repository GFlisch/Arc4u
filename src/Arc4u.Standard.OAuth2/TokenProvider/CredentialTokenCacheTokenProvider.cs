using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    [System.Composition.Export(CredentialTokenCacheTokenProvider.ProviderName, typeof(ICredentialTokenProvider))]
    public class CredentialTokenCacheTokenProvider : ICredentialTokenProvider
    {
        public const string ProviderName = "Credential";
        private readonly ITokenCache TokenCache;
        private readonly IContainerResolve Container;
        private readonly ILogger Logger;


        [System.Composition.ImportingConstructor]
        public CredentialTokenCacheTokenProvider(ITokenCache tokenCache, ILogger logger, IContainerResolve container)
        {
            TokenCache = tokenCache;
            Logger = logger;
            Container = container;
        }

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential)
        {
            var messages = GetContext(settings, out string clientId, out string authority, out string authenticationType, out string serviceApplicationId);

            if (String.IsNullOrWhiteSpace(credential.Upn))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, "No Username is provided."));

            if (String.IsNullOrWhiteSpace(credential.Password))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, "No password is provided."));

            messages.LogAndThrowIfNecessary(this);
            messages.Clear();

            if (null != TokenCache)
            {
                // Get a HashCode from the password so a second call with the same upn but with a wrong password will not be impersonated due to
                // the lack of password check.
                var cacheKey = BuildKey(credential, authority, serviceApplicationId);
                Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Check if the cache contains a token for {cacheKey}.").Log();
                var tokenInfo = TokenCache.Get<TokenInfo>(cacheKey);
                var hasChanged = false;

                if (null != tokenInfo)
                {
                    Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Token loaded from the cache for {cacheKey}.").Log();

                    if (tokenInfo.ExpiresOnUtc < DateTime.UtcNow.AddMinutes(1))
                    {
                        Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Token is expired for {cacheKey}.").Log();

                        // We need to refresh the token.
                        tokenInfo = await CreateBasicTokenInfoAsync(settings, credential);
                        hasChanged = true;
                    }
                }
                else
                {
                    Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Contact the STS to create an access token for {cacheKey}.").Log();
                    tokenInfo = await CreateBasicTokenInfoAsync(settings, credential);
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    try
                    {
                        Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Save the token in the cache for {cacheKey}, will expire at {tokenInfo.ExpiresOnUtc} Utc.").Log();
                        TokenCache.Put(cacheKey, tokenInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Technical().From<CredentialTokenCacheTokenProvider>().Exception(ex).Log();
                    }

                }

                return tokenInfo;
            }

            // no cache, do a direct call on every calls.
            Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"No cache is defined. STS is called for every call.").Log();
            return await CreateBasicTokenInfoAsync(settings, credential);

        }

        protected async Task<TokenInfo> CreateBasicTokenInfoAsync(IKeyValueSettings settings, CredentialsResult credential)
        {
            var basicTokenProvider = Container.Resolve<ICredentialTokenProvider>(CredentialTokenProvider.ProviderName);

            return await basicTokenProvider.GetTokenAsync(settings, credential);
        }

        private static string BuildKey(CredentialsResult credential, string authority, string serviceApplicationId)
        {
            return authority + "_" + serviceApplicationId + "_Password_" + credential.Upn + "_" + credential.Password.GetHashCode().ToString();
        }

        private Messages GetContext(IKeyValueSettings settings, out string clientId, out string authority, out string authenticationType, out string serviceApplicationId)
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
                authenticationType = null;
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
            if (!settings.Values.ContainsKey(TokenKeys.AuthenticationTypeKey))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                         ServiceModel.MessageType.Error,
                         "Authentication type key is missing. Cannot process the request."));

            Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Creating an authentication context for the request.").Log();
            clientId = settings.Values[TokenKeys.ClientIdKey];
            serviceApplicationId = settings.Values[TokenKeys.ServiceApplicationIdKey];
            authority = settings.Values[TokenKeys.AuthorityKey];
            authenticationType = settings.Values[TokenKeys.AuthenticationTypeKey];

            Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"ClientId = {clientId}.").Log();
            Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"ServiceApplicationId = {serviceApplicationId}.").Log();
            Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Authority = {authority}.").Log();
            Logger.Technical().From<CredentialTokenCacheTokenProvider>().System($"Authentication type = {authenticationType}.").Log();

            return messages;

        }
    }
}
