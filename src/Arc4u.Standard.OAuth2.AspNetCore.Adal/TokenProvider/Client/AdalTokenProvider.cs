using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Arc4u.OAuth2.TokenProvider.Client
{
    public abstract class AdalTokenProvider : ITokenProvider
    {
        private Dictionary<string, AuthenticationResult> _resultCache = new Dictionary<string, AuthenticationResult>();
        protected readonly ILogger<AdalTokenProvider> _logger;
        protected readonly IContainerResolve Container;

        public AdalTokenProvider(ILogger<AdalTokenProvider> logger, IContainerResolve container)
        {
            _logger = logger;
            Container = container;
        }

        // Request a token 
        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings)
                throw new NullReferenceException(nameof(settings));

            if (null == platformParameters as IPlatformParameters)
                throw new ArgumentException(nameof(platformParameters));

            return await AuthenticationResultAsync(settings, (IPlatformParameters)platformParameters);
        }

        private async Task<TokenInfo> AuthenticationResultAsync(IKeyValueSettings settings, IPlatformParameters platformParameters)
        {
            var authContext = GetContext(settings, out string serviceId, out string clientId, out string authority);

            var redirectUri = new Uri(settings.Values[TokenKeys.RedirectUrl]);
            _logger.Technical().System($"{TokenKeys.RedirectUrl} = {redirectUri}.").Log();
            _logger.Technical().System("Acquire a token.").Log();

            // Start Vpn if needed.
            Network.Handler.OnCalling?.Invoke(new Uri(authority));

            AuthenticationResult result = null;
            // Check if we have an AuthenticationResult cached and still valid.
            if (_resultCache.ContainsKey(clientId))
            {
                result = _resultCache[clientId];

                // Is valid with a security margin of 1 minute.
                if (null == result || result.ExpiresOn.LocalDateTime.AddMinutes(-1) < DateTime.Now)
                {
                    _logger.Technical().System($"Token cached for clientId = {clientId} is expired. Is removed from the cache.").Log();
                    _resultCache.Remove(clientId);
                    result = null;
                }
            }

            if (null == result)
            {
                result = await authContext.AcquireTokenAsync(serviceId, clientId, redirectUri, platformParameters);
                _resultCache.Add(clientId, result);
                _logger.Technical().System($"Add the token in the cache for clientId = {clientId}.").Log();
            }

            if (null != result)
            {
                // Dump no sensitive information.
                _logger.Technical().System($"Token information for user {result.UserInfo.DisplayableId}.").Log();
                _logger.Technical().System($"Token expiration = {result.ExpiresOn.ToString("dd-MM-yyyy HH:mm:ss")}.").Log();

                return result.ToTokenInfo();
            }

            return null;
        }

        private AuthenticationContext GetContext(IKeyValueSettings settings, out string serviceId, out string clientId, out string authority)
        {
            // Valdate arguments.
            if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
                throw new ArgumentException("Authority is missing. Cannot process the request.");
            if (!settings.Values.ContainsKey(TokenKeys.ClientIdKey))
                throw new ArgumentException("ClientId is missing. Cannot process the request.");
            if (!settings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
                throw new ArgumentException("ApplicationId is missing. Cannot process the request.");

            _logger.Technical().System($"Creating an authentication context for the request.").Log();
            clientId = settings.Values[TokenKeys.ClientIdKey];
            serviceId = settings.Values[TokenKeys.ServiceApplicationIdKey];
            authority = settings.Values[TokenKeys.AuthorityKey];

            // Check the information.
            var messages = new ServiceModel.Messages();
            if (String.IsNullOrWhiteSpace(clientId))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"{TokenKeys.ClientIdKey} is not defined in the configuration file."));
            else
                _logger.Technical().System($"{TokenKeys.ClientIdKey} = {clientId}.").Log();

            if (String.IsNullOrWhiteSpace(serviceId))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"No information from the application settings section about an entry: {TokenKeys.ServiceApplicationIdKey}."));

            if (String.IsNullOrWhiteSpace(authority))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"{TokenKeys.AuthorityKey} is not defined in the configuration file."));
            else
                _logger.Technical().System($"{TokenKeys.AuthorityKey} = {authority}.").Log();

            messages.LogAndThrowIfNecessary(_logger);
            messages.Clear();

            // The cache is per user on the device and Application.
            var authContext = CreateAuthenticationContext(authority, serviceId);
            _logger.Technical().System("Authentication context is created.").Log();

            return authContext;
        }

        protected abstract AuthenticationContext CreateAuthenticationContext(String authority, string cacheIdentifier);

        public void SignOut(IKeyValueSettings settings)
        {
            var authContext = GetContext(settings, out string _, out string _, out string authority);

            authContext.TokenCache.Clear();
        }
    }
}
