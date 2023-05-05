using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Msal.TokenProvider.Client
{
    [Export(ProviderName, typeof(ITokenProvider))]
    public class MsalTokenProvider : ITokenProvider
    {
        public const string ProviderName = "clientApplication";

        public MsalTokenProvider(PublicClientApp clientApp, IApplicationContext applicationContext, ILogger<MsalTokenProvider> logger)
        {
            _publicClientApplication = clientApp;
            _applicationContext = applicationContext;
            _logger = logger;
        }

        private readonly PublicClientApp _publicClientApplication;
        private readonly IApplicationContext _applicationContext;
        private readonly ILogger<MsalTokenProvider> _logger;

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            if (null == settings) throw new ArgumentNullException(nameof(settings));

            if (null != _applicationContext.Principal)
            {
                var identity = (ClaimsIdentity)_applicationContext.Principal.Identity;

                if (!String.IsNullOrWhiteSpace(identity.BootstrapContext.ToString()))
                {
                    var token = identity.BootstrapContext.ToString();

                    JwtSecurityToken jwt = new(token);

                    if (jwt.ValidTo > DateTime.UtcNow)
                        return new TokenInfo("Bearer", token, jwt.ValidTo);
                }

            }

            if (null == _publicClientApplication) return null;

            var accounts = await _publicClientApplication.PublicClient.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            var scopes = settings.Values[TokenKeys.Scopes].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            AuthenticationResult authResult = null;
            try
            {
                authResult = await _publicClientApplication.PublicClient.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync();

                JwtSecurityToken jwt = new(authResult.AccessToken);
                return new TokenInfo("Bearer", authResult.AccessToken, jwt.ValidTo);
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                _logger.Technical().System($"MsalUiRequiredException: {ex.Message}").Log();

                try
                {
                    var builder = _publicClientApplication.PublicClient
                                    .AcquireTokenInteractive(scopes)
                                    .WithAccount(accounts.FirstOrDefault())
                                    .WithPrompt(Prompt.SelectAccount);

                    if (_publicClientApplication.HasCustomWebUi)
                        builder.WithCustomWebUi(_publicClientApplication.CustomWebUi);

                    authResult = await builder.ExecuteAsync();

                    JwtSecurityToken jwt = new(authResult.AccessToken);
                    return new TokenInfo("Bearer", authResult.AccessToken, jwt.ValidTo);
                }
                catch (MsalException msalex)
                {
                    _logger.Technical().Exception(msalex).Log();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public void SignOut(IKeyValueSettings settings)
        {
            if (null != _publicClientApplication)
            {
                var accounts = _publicClientApplication.PublicClient.GetAccountsAsync().Result;

                if (accounts.Any())
                {
                    try
                    {
                        await _publicClientApplication.PublicClient.RemoveAsync(accounts.FirstOrDefault()).Wait();
                    }
                    catch (MsalException msalex)
                    {
                        _logger.Technical().Exception(msalex).Log();
                    }
                }

            }
        }
    }
}

