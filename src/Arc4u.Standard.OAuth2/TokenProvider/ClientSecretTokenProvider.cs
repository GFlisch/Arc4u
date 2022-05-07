using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    /// <summary>
    /// Read the settings and will generate a Token based on a:
    /// - Base64 clientSecret string (user:password).
    /// - Encrypted clientSecret string (user:password).
    /// </summary>
    [Export(ClientSecretTokenProvider.ProviderName, typeof(ITokenProvider))]
    public sealed class ClientSecretTokenProvider : ITokenProvider
    {
        public ClientSecretTokenProvider(ITokenCache tokenCache, IContainerResolve container, ILogger<ClientSecretTokenProvider> logger)
        {
            _tokenCache = tokenCache;
            _container = container;
            _logger = logger;
        }

        private readonly ITokenCache _tokenCache;
        private readonly IContainerResolve _container;
        private readonly ILogger<ClientSecretTokenProvider> _logger;

        public const string ProviderName = "ClientSecret";

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings)
                throw new NullReferenceException("settings");

            return await AuthenticationResultAsync(settings);
        }

        private async Task<TokenInfo> AuthenticationResultAsync(IKeyValueSettings settings)
        {
            ValidateKeys(settings, out string clientSecret, out string serviceApplicationId, out var certificateInfo);

            var cacheKey = "Secret_" + serviceApplicationId.ToLowerInvariant() + clientSecret.GetHashCode().ToString();

            var tokenInfo = _tokenCache.Get<TokenInfo>(cacheKey);

            if (null == tokenInfo || tokenInfo.ExpiresOnUtc < DateTime.UtcNow.AddMinutes(-1))
            {
                CredentialsResult credential;
                if (null != certificateInfo)
                {
                    credential = GetCredential(clientSecret, certificateInfo);
                }
                else
                {
                    // extract from the client secret the upn and password.
                    credential = GetCredential(clientSecret);
                }

                if (!credential.CredentialsEntered)
                    return null;

                tokenInfo = await CreateBasicTokenInfoAsync(settings, credential);

                _tokenCache.Put(cacheKey, tokenInfo);
            }

            return tokenInfo;
        }

        private void ValidateKeys(IKeyValueSettings settings, out string clientSecret, out string serviceApplicationId, out CertificateInfo certificateInfo)
        {
            var messages = new Messages();

            if (!settings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
                throw new ArgumentException("ServiceApplicationId is missing. Cannot process the request.");

            serviceApplicationId = settings.Values[TokenKeys.ServiceApplicationIdKey];

            if (!settings.Values.ContainsKey(TokenKeys.ClientSecret))
                throw new ArgumentException("Client secret is missing. Cannot process the request.");

            clientSecret = settings.Values[TokenKeys.ClientSecret];

            if (String.IsNullOrWhiteSpace(clientSecret))
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"{TokenKeys.ClientSecret} is not defined in the configuration file."));
            else
                _logger.Technical().System($"{TokenKeys.ClientSecret} = {clientSecret}.").Log();

            var certificateName = settings.Values.ContainsKey(TokenKeys.CertificateName) ? settings.Values[TokenKeys.CertificateName] : null;
            var findType = settings.Values.ContainsKey(TokenKeys.FindType) ? settings.Values[TokenKeys.FindType] : null;
            var storeLocation = settings.Values.ContainsKey(TokenKeys.StoreLocation) ? settings.Values[TokenKeys.StoreLocation] : null;
            var storeName = settings.Values.ContainsKey(TokenKeys.StoreName) ? settings.Values[TokenKeys.StoreName] : null;

            if (null == certificateName)
            {
                _logger.Technical().System("No Certificate, Base64 encoding of the username password.").Log();
                certificateInfo = null;
            }
            else
            {
                certificateInfo = new CertificateInfo
                {
                    Name = certificateName
                };

                if (Enum.TryParse(findType, out X509FindType x509FindType))
                    certificateInfo.FindType = x509FindType;
                if (Enum.TryParse(storeLocation, out StoreLocation storeLocation_))
                    certificateInfo.Location = storeLocation_;
                if (Enum.TryParse(storeName, out StoreName storeName_))
                    certificateInfo.StoreName = storeName_;
            }

            messages.LogAndThrowIfNecessary(typeof(ClientSecretTokenProvider));
        }

        private CredentialsResult GetCredential(string clientSecret)
        {
            string pair = Encoding.UTF8.GetString(Convert.FromBase64String(clientSecret));

            return ExtractCredential(pair);
        }

        private CredentialsResult GetCredential(string clientSecret, CertificateInfo certificateInfo)
        {
            try
            {
                var certificate = Certificate.FindCertificate(
                                    certificateInfo.Name,
                                    certificateInfo.FindType,
                                    certificateInfo.Location,
                                    certificateInfo.StoreName);

                string pair = Certificate.Decrypt(clientSecret, certificate);

                return ExtractCredential(pair);
            }
            catch (KeyNotFoundException)
            {
                _logger.Technical().Error($"No certificate found with {certificateInfo.FindType} = {certificateInfo.Name} in location = {certificateInfo.Location}.")
                    .Add("FindType", certificateInfo.FindType.ToString())
                    .Add("Name", certificateInfo.Name)
                    .Add("Location", certificateInfo.Location.ToString())
                    .Log();
            }
            catch (Exception ex)
            {
                Logger.Technical.From(this).Exception(ex).Log();
            }

            return new CredentialsResult(false);
        }

        private CredentialsResult ExtractCredential(string formatedCredential)
        {
            var ix = formatedCredential.IndexOf(':');
            if (ix == -1)
            {
                Logger.Technical.From(this).Warning("Basic authentication is not well formed.").Log();
                return new CredentialsResult(false);
            }

            var username = formatedCredential.Substring(0, ix);
            var pwd = formatedCredential.Substring(ix + 1);

            return new CredentialsResult(true, username, pwd);
        }
        protected async Task<TokenInfo> CreateBasicTokenInfoAsync(IKeyValueSettings settings, CredentialsResult credential)
        {
            var basicTokenProvider = _container.Resolve<ICredentialTokenProvider>(CredentialTokenProvider.ProviderName);

            return await basicTokenProvider.GetTokenAsync(settings, credential);
        }

        public void SignOut(IKeyValueSettings settings)
        {
            ValidateKeys(settings, out string clientSecret, out string serviceApplicationId, out var certificateInfo);

            var cacheKey = "Secret_" + serviceApplicationId.ToLowerInvariant() + clientSecret.GetHashCode().ToString();

            _tokenCache.DeleteItem(cacheKey);
        }


    }
}
