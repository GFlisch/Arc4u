using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public class ClientSecretAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ClientSecretAuthenticationOption _option;
        private readonly ILogger<ClientSecretAuthenticationMiddleware> _logger;
        private readonly ICredentialTokenProvider _provider;
        private readonly bool _hasProvider;
        private readonly bool _hasCertificate;
        private readonly X509Certificate2 _certificate;
        private readonly ActivitySource _activitySource;

        public ClientSecretAuthenticationMiddleware(RequestDelegate next, IContainerResolve container, ClientSecretAuthenticationOption option)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            if (null == container)
                throw new ArgumentNullException(nameof(container));

            if (null == option)
                throw new ArgumentNullException(nameof(option));

            _logger = container.Resolve<ILogger<ClientSecretAuthenticationMiddleware>>();
            _option = option;

            if (null == option.Settings)
            {
                _logger.Technical().Warning("No settings have been provided.").Log();
                return;
            }

            if (option.Settings.Values.ContainsKey(TokenKeys.ProviderIdKey))
            {
                _hasProvider = container.TryResolve(option.Settings.Values[TokenKeys.ProviderIdKey], out _provider);
            }

            if (!_hasProvider)
            {
                _logger.Technical().Warning("No ClientSecret provider found. Skip ClientSecret capability.").Log();
                return;
            }

            _hasCertificate = null != option.Certificate
                && !String.IsNullOrWhiteSpace(option.Certificate.SecretKey)
                && !String.IsNullOrWhiteSpace(option.Certificate.Name);

            // check the certificate exists.
            if (_hasCertificate)
            {
                try
                {
                    var certificateInfo = new CertificateInfo
                    {
                        Name = option.Certificate.Name
                    };

                    if (Enum.TryParse(option.Certificate.FindType, out X509FindType x509FindType))
                        certificateInfo.FindType = x509FindType;
                    if (Enum.TryParse(option.Certificate.Location, out StoreLocation storeLocation_))
                        certificateInfo.Location = storeLocation_;
                    if (Enum.TryParse(option.Certificate.StoreName, out StoreName storeName_))
                        certificateInfo.StoreName = storeName_;

                    _certificate = Certificate.FindCertificate(
                                    certificateInfo.Name,
                                    certificateInfo.FindType,
                                    certificateInfo.Location,
                                    certificateInfo.StoreName);

                    _logger.Technical().System($"Authentication with certificate secret activated.").Log();
                }
                catch (KeyNotFoundException)
                {
                    _hasCertificate = false;
                    _logger.Technical().Error($"No certificate found with {option.Certificate.FindType} = {option.Certificate.Name} in location = {option.Certificate.Location}.").Log();
                }
                catch (Exception ex)
                {
                    _logger.Technical().Exception(ex).Log();
                }

            }
            else
                _logger.Technical().System($"No authentication  with certificate secret.").Log();

            _activitySource = container.Resolve<IActivitySourceFactory>()?.GetArc4u();


        }

        public async Task Invoke(HttpContext context)
        {
            if (!_hasProvider)
            {
                await _next.Invoke(context);
                return;
            }

            try
            {
                // Add Telemetry.
                using (var activity = _activitySource?.StartActivity("Create Arc4u Principal", ActivityKind.Producer))
                {
                    var clientSecret = GetClientSecretIfExist(context, _option.SecretKey);

                    bool basicAuthenticationSecret = !String.IsNullOrWhiteSpace(clientSecret);

                    bool certificateAuthenticationSecret = false;
                    // Check for a secret key encrypted.
                    if (_hasCertificate && !basicAuthenticationSecret)
                    {
                        clientSecret = GetClientSecretIfExist(context, _option.Certificate.SecretKey);
                        certificateAuthenticationSecret = !String.IsNullOrWhiteSpace(clientSecret);
                    }

                    if (!String.IsNullOrWhiteSpace(clientSecret))
                    {
                        CredentialsResult credential = null;

                        if (basicAuthenticationSecret)
                            credential = GetCredential(clientSecret);

                        if (certificateAuthenticationSecret)
                            credential = GetCertificateCredential(clientSecret);

                        if (null != credential)
                        {
                            // Get an Access Token.
                            var tokenInfo = await _provider.GetTokenAsync(_option.Settings, credential);

                            // Replace the Basic Authorization by the access token in the header.
                            var authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token).ToString();
                            context.Request.Headers.Remove("Authorization");
                            context.Request.Headers.Add("Authorization", authorization);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();
            }

            await _next(context);
        }

        String GetClientSecretIfExist(HttpContext context, string key)
        {
            if (null == context) throw new ArgumentNullException("context");

            if (string.IsNullOrWhiteSpace(key)) return null;

            string clientSecret = context.Request.Headers.FirstOrDefault(header => header.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value.FirstOrDefault();

            // Check if this is in the url query string.
            if (String.IsNullOrWhiteSpace(clientSecret))
            {
                clientSecret = context.Request.Query.FirstOrDefault(p => p.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value.FirstOrDefault();
            }

            return clientSecret;
        }

        private CredentialsResult GetCertificateCredential(string clientSecret)
        {
            string pair = _certificate.Decrypt(clientSecret);

            return ExtractCredential(pair);
        }

        private CredentialsResult GetCredential(string clientSecret)
        {
            string pair = Encoding.UTF8.GetString(Convert.FromBase64String(clientSecret));

            return ExtractCredential(pair);
        }

        private CredentialsResult ExtractCredential(string pair)
        {
            var ix = pair.IndexOf(':');
            if (ix == -1)
            {
                _logger.Technical().Warning("Basic authentication is not well formed.").Log();
            }

            var username = pair.Substring(0, ix);
            var pwd = pair.Substring(ix + 1);

            return new CredentialsResult(true, username, pwd);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientSecretAuthenticationMiddleware middleware &&
                   EqualityComparer<ClientSecretAuthenticationOption>.Default.Equals(_option, middleware._option);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_option);
        }
    }

}
