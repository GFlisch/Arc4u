using Arc4u.Caching;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.IdentityModel.Claims;
using Arc4u.Network.Connectivity;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Arc4u.OAuth2.Client.Security.Principal
{
    [Export(typeof(IAppPrincipalFactory))]
    public class AppPrincipalFactory : IAppPrincipalFactory
    {
        public const string ProviderKey = "ProviderId";
        public const string DefaultSettingsResolveName = "OAuth";
        public const string PlatformParameters = "platformParameters";

        public static readonly string tokenExpirationClaimType = "exp";
        public static readonly string[] ClaimsToExclude = { "exp", "aud", "iss", "iat", "nbf", "acr", "aio", "appidacr", "ipaddr", "scp", "sub", "tid", "uti", "unique_name", "apptype", "appid", "ver", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant", "http://schemas.microsoft.com/identity/claims/scope" };


        private bool copyClaimsFromCache = false;
        private List<ClaimDto> cachedClaims;

        public AppPrincipalFactory(INetworkInformation networkInformation, ISecureCache claimsCache, ICacheKeyGenerator cacheKeyGenerator, IContainerResolve container, IApplicationContext applicationContext)
        {
            NetworkInformation = networkInformation;
            ClaimsCache = claimsCache;
            CacheKeyGenerator = cacheKeyGenerator;
            Container = container;
            ApplicationContext = applicationContext;
        }

        private INetworkInformation NetworkInformation { get; set; }

        private ICache ClaimsCache { get; set; }

        private ClaimsIdentity Identity { get; set; }

        private IContainerResolve Container { get; set; }

        private ICacheKeyGenerator CacheKeyGenerator { get; set; }

        private IApplicationContext ApplicationContext { get; set; }

        public async Task<AppPrincipal> CreatePrincipal(Messages messages, object parameter = null)
        {
            Identity = new ClaimsIdentity("OAuth2Bearer", System.Security.Claims.ClaimTypes.Upn, ClaimsIdentity.DefaultRoleClaimType);

            return await CreatePrincipal(DefaultSettingsResolveName, messages, parameter).ConfigureAwait(true);
        }

        public async Task<AppPrincipal> CreatePrincipal(string settingsResolveName, Messages messages, object parameter = null)
        {
            var settings = Container.Resolve<IKeyValueSettings>(settingsResolveName);

            return await CreatePrincipal(settings, messages, parameter);
        }

        public async Task<AppPrincipal> CreatePrincipal(IKeyValueSettings settings, Messages messages, object parameter = null)
        {
            Identity = new ClaimsIdentity("OAuth2Bearer", System.Security.Claims.ClaimTypes.Upn, ClaimsIdentity.DefaultRoleClaimType);

            if (null == settings) throw new ArgumentNullException(nameof(settings));

            if (null == messages) throw new ArgumentNullException(nameof(messages));

            /// when we have no internet connectivity may be we have claims in cache.
            if (NetworkStatus.None == NetworkInformation.Status)
            {
                // In a scenario where the claims cached are always for one user like a UI, the identity is not used => so retrieving the claims in the cache is possible!
                var identity = new ClaimsIdentity();
                cachedClaims = GetClaimsFromCache(identity);
                Identity.AddClaims(cachedClaims.Select(p => new Claim(p.ClaimType, p.Value)));
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Information, "Create the principal from the cache due to no network connectivity."));
            }
            else
                await BuildTheIdentity(settings, messages, parameter);

            var authorization = BuildAuthorization(settings, messages);
            var user = BuildProfile(settings, messages);

            var principal = new AppPrincipal(authorization, Identity, null)
            {
                Profile = user
            };
            ApplicationContext.SetPrincipal(principal);

            return principal;
        }

        private async Task BuildTheIdentity(IKeyValueSettings settings, Messages messages, object parameter = null)
        {
            // Check if we have a provider registered.
            if (!Container.TryResolve(settings.Values[ProviderKey], out ITokenProvider provider))
            {
                throw new NotSupportedException($"The principal cannot be created. We are missing an account provider: {settings.Values[ProviderKey]}");
            }

            // Check the settings contains the service url.
            TokenInfo token = null;
            try
            {
                token = await provider.GetTokenAsync(settings, parameter).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Logger.Technical.From<AppPrincipalFactory>().Exception(ex).Log();
            }

            if (null != token)
            {
                // The token has claims filled by the STS.
                // We can fill the new federated identity with the claims from the token.
                var jwtToken = new JwtSecurityToken(token.AccessToken);
                var expTokenClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals(tokenExpirationClaimType, StringComparison.InvariantCultureIgnoreCase));
                long expTokenTicks = 0;
                if (null != expTokenClaim)
                    long.TryParse(expTokenClaim.Value, out expTokenTicks);

                Identity.BootstrapContext = token.AccessToken;

                // The key for the cache is based on the claims from a ClaimsIdentity => build  a dummy identity with the claim from the token.
                var identity = new ClaimsIdentity(jwtToken.Claims);
                cachedClaims = GetClaimsFromCache(identity);
                // if we have a token "cached" from the system, we can take the authorization claims from the cache (if exists)...
                // so we avoid too many backend calls for nothing.
                // But every time we have a token that has been refreshed, we will call the backend (if available and reload the claims).
                var cachedExpiredClaim = cachedClaims.FirstOrDefault(c => c.ClaimType.Equals(tokenExpirationClaimType, StringComparison.InvariantCultureIgnoreCase));
                long cachedExpiredTicks = 0;

                if (null != cachedExpiredClaim)
                    long.TryParse(cachedExpiredClaim.Value, out cachedExpiredTicks);

                // we only call the backend if the ticks are not the same.
                copyClaimsFromCache = cachedExpiredTicks > 0 && expTokenTicks > 0 && cachedClaims.Count > 0 && cachedExpiredTicks == expTokenTicks;


                if (copyClaimsFromCache)
                {
                    Identity.AddClaims(cachedClaims.Select(p => new Claim(p.ClaimType, p.Value)));
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Information, "Create the principal from the cache, token has not been refreshed."));
                }
                else
                {
                    // Fill the claims based on the token and the backend call
                    Identity.AddClaims(jwtToken.Claims.Where(c => !ClaimsToExclude.Any(arg => arg.Equals(c.Type))).Select(c => new Claim(c.Type, c.Value)));
                    Identity.AddClaim(expTokenClaim);

                    if (Container.TryResolve(out IClaimsFiller claimFiller)) // Fill the claims with more information.
                    {
                        try
                        {
                            // Get the claims and clean any technical claims in case of.
                            var claims = (await claimFiller.GetAsync(Identity, new List<IKeyValueSettings> { settings }, parameter))
                                            .Where(c => !ClaimsToExclude.Any(arg => arg.Equals(c.ClaimType))).ToList();


                            // We copy the claims from the backend but the exp claim will be the value of the token (front end definition) and not the backend one. Otherwhise there will be always a difference.
                            Identity.AddClaims(claims.Where(c => !Identity.Claims.Any(c1 => c1.Type == c.ClaimType)).Select(c => new Claim(c.ClaimType, c.Value)));

                            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Information, $"Add {claims.Count()} claims to the principal."));
                            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Information, $"Save claims to the cache."));
                        }
                        catch (Exception e)
                        {
                            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Error, e.ToString()));
                        }
                    }

                    SaveClaimsToCache(Identity.Claims);
                }
            }
            else
            {
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, "The call to identify the user has failed. Token is null!"));
            }

        }

        /// <summary>
        /// Based on the token provider Id, the method will call the token provider and build a claimPrincipal!
        /// The provider id is the string used by the Composition library to register the type and not the provider Id used by the token provider itself (Microsoft, google, or other...).
        /// Today only the connected scenario is covered!
        /// </summary>
        /// <param name="IAppSettings">The settings needed to authenticate the user.</param>
        private Authorization BuildAuthorization(IKeyValueSettings settings, Messages messages)
        {
            var authorization = new Authorization();
            // We need to fill the authorization and user profile from the provider!
            if (Container.TryResolve(out IClaimAuthorizationFiller claimAuthorizationFiller))
            {
                authorization = claimAuthorizationFiller.GetAuthorization(Identity);
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Information, $"Fill the authorization information to the principal."));
            }
            else
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"No class waw found to fill the authorization to the principal."));

            return authorization;
        }

        private UserProfile BuildProfile(IKeyValueSettings settings, Messages messages)
        {
            UserProfile profile = UserProfile.Empty;
            if (Container.TryResolve(out IClaimProfileFiller profileFiller))
            {
                profile = profileFiller.GetProfile(Identity);
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Information, $"Fill the profile information to the principal."));
            }
            else
                messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"No class was found to fill the principal profile."));

            return profile;
        }


        // must be refactored => Take this based on a strategy based on the .Net code.
        private List<ClaimDto> GetClaimsFromCache(ClaimsIdentity identity)
        {
            try
            {
                var secureClaims = ClaimsCache.Get<List<ClaimDto>>(CacheKeyGenerator.GetClaimsKey(identity));
                return secureClaims ?? new List<ClaimDto>();
            }
            catch (Exception)
            {
                return new List<ClaimDto>();
            }
        }

        private void SaveClaimsToCache(IEnumerable<Claim> claims)
        {

            var dto = claims.Select(c => new ClaimDto(c.Type, c.Value)).ToList();

            try
            {
                ClaimsCache.Put(CacheKeyGenerator.GetClaimsKey(Identity), dto);
            }
            catch (Exception ex)
            {
                Logger.Technical.From<AppPrincipalFactory>().Exception(ex).Log();
            }
        }

        private void RemoveClaimsCache()
        {
            try
            {
                ClaimsCache.Remove(CacheKeyGenerator.GetClaimsKey(Identity));
            }
            catch (Exception ex)
            {
                Logger.Technical.From<AppPrincipalFactory>().Exception(ex).Log();
            }
        }


        public void SignOutUser()
        {
            RemoveClaimsCache();

            var settings = Container.Resolve<IKeyValueSettings>(DefaultSettingsResolveName);


            if (Container.TryResolve(settings.Values[ProviderKey], out ITokenProvider provider))
            {
                provider.SignOut(settings);
            }
        }

        public void SignOutUser(IKeyValueSettings settings)
        {
            RemoveClaimsCache();

            if (Container.TryResolve(settings.Values[ProviderKey], out ITokenProvider provider))
            {
                provider.SignOut(settings);
            }
        }
    }
}
