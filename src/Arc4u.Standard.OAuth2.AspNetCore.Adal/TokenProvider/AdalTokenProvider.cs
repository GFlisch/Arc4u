using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Arc4u.OAuth2.TokenProvider
{
    public abstract class AdalTokenProvider : ITokenProvider
    {
        public const string ProviderName = "Adal";
        private readonly OAuthConfig oAuthConfig;
        private ClaimsIdentity identity;
        protected readonly ILogger Logger;
        protected readonly IContainerResolve Container;

        public AdalTokenProvider(OAuthConfig oAuthConfig, ILogger logger, IContainerResolve container)
        {
            Logger = logger;
            Container = container;
            this.oAuthConfig = oAuthConfig;
        }

        /// <summary>
        /// Request a token in a backend scenario.
        /// As this is a backend token provider, platformParameter is not used as ADAL expect it in a UI app.
        /// </summary>
        /// <param name="settings">The settings to retrieve the token.</param>
        /// <param name="platformParameters">null</param>
        /// <returns></returns>
        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            // give the identity to platform parameter when used in the BE which is the purpose of this ITokenProvide.
            identity = platformParameters as ClaimsIdentity;

            if (null == identity && Container.TryResolve<IApplicationContext>(out var applicationContext))
            {
                identity = applicationContext.Principal?.Identity as ClaimsIdentity;
            }

            if (null != identity?.BootstrapContext)
            {
                var tokenInfo = TokenIsValid(settings, identity.BootstrapContext.ToString());

                if (null != tokenInfo)
                    return tokenInfo;
            }

            var result = await AuthenticationResultAsync(settings);

            return result.ToTokenInfo();
        }


        private async Task<AuthenticationResult> AuthenticationResultAsync(IKeyValueSettings settings)
        {
            var authContext = GetContext(settings,
                                         out string serviceApplicationId,
                                         out string userObjectId,
                                         out ClientCredential credential,
                                         out string clientId,
                                         out Uri redirectUri);

            AuthenticationResult result = null;
            if (null != credential)
            {
                Logger.Technical().From(this).System("Acquire a token silently for an application identified by his application key.").Log();

                if (Enum.TryParse<UserIdentifierType>(oAuthConfig.User.Identifier, out var identifier))
                    result = await authContext.AcquireTokenSilentAsync(serviceApplicationId, credential, new UserIdentifier(userObjectId, identifier));
            }

            if (null != result)
            {
                // Dump no sensitive information.
                Logger.Technical().From(this).System($"Token information for user {result.UserInfo.DisplayableId}.").Log();
                Logger.Technical().From(this).System($"Token expiration = {result.ExpiresOn.ToString("dd-MM-yyyy HH:mm:ss")}.").Log();
            }

            return result;
        }


        /// <summary>
        /// Get a context for the different scenarios.
        /// 1) An applicationKey exists.
        /// 2) No applicationKey and no
        /// </summary>
        /// <param name="serviceUriId"></param>
        /// <param name="serviceId"></param>
        /// <param name="userObjectId"></param>
        /// <param name="credential"></param>
        /// <param name="bootstrapContext"></param>
        /// <returns></returns>
        private AuthenticationContext GetContext(IKeyValueSettings settings,
                                                 out string serviceApplicationId,
                                                 out string userObjectId,
                                                 out ClientCredential credential,
                                                 out string clientId,
                                                 out Uri redirectUri)
        {
            // Check the information.
            var messages = new Messages();

            if (null == settings)
            {
                messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical,
                                         Arc4u.ServiceModel.MessageType.Error,
                                         "Settings parameter cannot be null."));
                messages.LogAndThrowIfNecessary(this);
                messages.Clear();
            }


            // Valdate arguments.
            if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
                throw new ArgumentException("Authority is missing. Cannot process the request.");
            if (!settings.Values.ContainsKey(TokenKeys.ClientIdKey))
                throw new ArgumentException("ClientId is missing. Cannot process the request.");
            if (!settings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
                throw new ArgumentException("ApplicationId is missing. Cannot process the request.");
            if (!settings.Values.ContainsKey(TokenKeys.AuthenticationTypeKey))
                throw new ArgumentException("AuthenticationType is missing. Cannot process the request.");

            Logger.Technical().From(this).System($"Creating an authentication context for the request.").Log();
            clientId = settings.Values[TokenKeys.ClientIdKey];
            serviceApplicationId = settings.Values[TokenKeys.ServiceApplicationIdKey];

            var authority = settings.Values[TokenKeys.AuthorityKey];

            if (settings.Values.TryGetValue(TokenKeys.RedirectUrl, out var redirectUrl))
                redirectUri = new Uri(redirectUrl);
            else
                redirectUri = null;

            Logger.Technical().From(this).System($"ClientId = {clientId}.").Log();
            Logger.Technical().From(this).System($"ServiceApplicationId = {serviceApplicationId}.").Log();
            Logger.Technical().From(this).System($"Authority = {authority}.").Log();

            if (settings.Values.TryGetValue(TokenKeys.ApplicationKey, out var applicationKey))
            {
                credential = new ClientCredential(clientId, applicationKey);
            }
            else
            {
                Logger.Technical().From(this).System("No application key is defined to identify the application in the STS.").Log();
                credential = null;
            }

            if (null == credential) // we create an account based on the current user running (here a service account).
            {
                if (WindowsIdentity.GetCurrent().IsAuthenticated)
                {
                    userObjectId = WindowsIdentity.GetCurrent().Name;
                }
                else userObjectId = "svc_user";
            }
            else
            {
                userObjectId = oAuthConfig.UserClaimIdentifier(this.identity);
                if (String.IsNullOrWhiteSpace(userObjectId))
                {
                    messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, "No user object identifier is found in the claims collection to identify the user."));
                    messages.LogAndThrowIfNecessary(typeof(AdalTokenProvider));
                }
            }

            var authContext = CreateAuthenticationContext(authority, serviceApplicationId + settings.Values[TokenKeys.AuthenticationTypeKey] + userObjectId);
            Logger.Technical().From(this).System("Created the authentication context.").Log();


            return authContext;
        }

        protected abstract AuthenticationContext CreateAuthenticationContext(String authority, string cacheIdentifier);


        static TokenInfo TokenIsValid(IKeyValueSettings settings, string token)
        {
            // Check we have all the data available: a token, settings and the ServiceApplicationId.
            if (null == settings)
                return null;

            if (String.IsNullOrWhiteSpace(token))
                return null;

            if (!settings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
                return null;

            var audience = settings.Values[TokenKeys.ServiceApplicationIdKey];

            var jwtToken = new JwtSecurityToken(token);

            if (jwtToken.Audiences.Any(a => a.StartsWith(audience, StringComparison.InvariantCultureIgnoreCase)))
            {
                return new TokenInfo("Bearer", token, String.Empty, jwtToken.ValidTo);
            }

            return null;
        }

        public void SignOut(IKeyValueSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
