using Arc4u.Caching;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Exceptions;
using Arc4u.Network.Connectivity;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.TokenProvider;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Token;

[Export(UsernamePasswordTokenProvider.ProviderName, typeof(ITokenProvider))]
public class UsernamePasswordTokenProvider : ITokenProvider
{
    public UsernamePasswordTokenProvider(ISecureCache secureCache, INetworkInformation networkStatus, ILogger<UsernamePasswordTokenProvider> logger, IContainerResolve container)
    {
        this.secureCache = secureCache;
        this.networkStatus = networkStatus;
        Container = container;
        _logger = logger;
    }

    public const string ProviderName = "usernamePassword";

    private readonly ICache secureCache;
    private readonly INetworkInformation networkStatus;
    private readonly IContainerResolve Container;
    private readonly ILogger<UsernamePasswordTokenProvider> _logger;

    private string userkey;
    private string pwdkey;
    private string serviceId;
    private string authority;
    private string passwordStoreKey;
    private IKeyValueSettings Settings;

    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
    {
        // Take settings info.
        GetSettings(settings, out serviceId, out authority, out passwordStoreKey);

        Settings = settings;

        secureCache.TryGetValue<TokenInfo>(serviceId, out var tokenInfo);

        // Make the token expired 1 minute before a usage so we will not given back a token close to the expiration.
        if (null != tokenInfo && tokenInfo.ExpiresOnUtc > DateTime.UtcNow.AddMinutes(1))
            return tokenInfo;

        // Check if we have the username and password.
        // The username and password is stored also in the secureCache. 
        // If more than one password must be stored, the key used to identify the good user/password 
        // must be specified in the settings. Otherwhise the default 'secret' key is used.
        userkey = passwordStoreKey + "_upn";
        pwdkey = passwordStoreKey + "_pwd";

        secureCache.TryGetValue<string>(userkey, out var upn);
        secureCache.TryGetValue<string>(pwdkey, out var pwd);

        if (String.IsNullOrWhiteSpace(upn) || String.IsNullOrWhiteSpace(pwd))
        {
            if (!Container.TryResolve<IUserNamePasswordProvider>(out var usernamePasswordProvider))
                throw new AppException("No Token provider found in the container.");

            // Ask for the credentials and the await is blocked until the user has entered the information.
            // The page must be a modal one.
            var hasCredential = await usernamePasswordProvider.GetCredentials(upn, CheckCredentialsAsync).ConfigureAwait(false);
            if (!hasCredential.CredentialsEntered)
                return null;

            // We know we have a valid Upn and Password.
            upn = hasCredential.Upn;
            pwd = hasCredential.Password;
            // Store the new User and password in the cache.
            secureCache.Put(userkey, upn);
            secureCache.Put(pwdkey, pwd);
        }

        // Check before requesting a token we have a network connectivity!
        if (networkStatus.Status == NetworkStatus.None)
            throw new NetworkException(networkStatus.Status);

        try
        {
            Network.Handler.OnCalling?.Invoke(new Uri(authority));

            tokenInfo = await CreateBasicTokenInfoAsync(settings, new CredentialsResult(true, upn, pwd)).ConfigureAwait(false);

            // Store the tokenInfo
            secureCache.Put(serviceId, tokenInfo);

            return tokenInfo;
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);

            throw;
        }

    }

    protected async Task<TokenInfo> CreateBasicTokenInfoAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var basicTokenProvider = Container.Resolve<ICredentialTokenProvider>(CredentialTokenProvider.ProviderName);

        return await basicTokenProvider.GetTokenAsync(settings, credential).ConfigureAwait(false);
    }

    // This method is called by the page receiving the user name and password. To be sure we have a valid one!
    // No two factor authentication is allowed in this scenario!
    public async Task<bool> CheckCredentialsAsync(string upn, string password)
    {
        try
        {
            Network.Handler.OnCalling?.Invoke(new Uri(authority));

            var tokenInfo = await CreateBasicTokenInfoAsync(Settings, new CredentialsResult(true, upn, password)).ConfigureAwait(false);

            // Store the tokenInfo
            secureCache.Put(serviceId, tokenInfo);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
            return false;
        }
    }

    private void GetSettings(IKeyValueSettings settings, out string serviceId, out string authority, out string passwordStoreKey)
    {
        // Validate arguments.
        if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
            throw new ArgumentException("Authority is missing. Cannot process the request.");
        if (!settings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
            throw new ArgumentException("ApplicationId is missing. Cannot process the request.");

        serviceId = settings.Values[TokenKeys.ServiceApplicationIdKey];
        authority = settings.Values[TokenKeys.AuthorityKey];

        // Check the information.
        var messages = new Arc4u.ServiceModel.Messages();

        if (String.IsNullOrWhiteSpace(serviceId))
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"No information from the application settings section about an entry: {TokenKeys.ServiceApplicationIdKey}."));

        if (String.IsNullOrWhiteSpace(authority))
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Warning, $"{TokenKeys.AuthorityKey} is not defined in the configuration file."));

        messages.LogAndThrowIfNecessary(_logger);
        messages.Clear();

        passwordStoreKey = "secret";
        if (settings.Values.ContainsKey(TokenKeys.PasswordStoreKey))
        {
            passwordStoreKey = String.IsNullOrWhiteSpace(settings.Values[TokenKeys.PasswordStoreKey]) ? passwordStoreKey : settings.Values[TokenKeys.PasswordStoreKey];
        }

    }

    public void SignOut(IKeyValueSettings settings)
    {
        GetSettings(settings, out var serviceId, out _, out var passwordStoreKey);

        try
        {
            // remove the passord information.
            var pwdkey = passwordStoreKey + "_pwd";
            secureCache.Remove(pwdkey);
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
        }

        try
        {
            // remove de access token information.
            secureCache.Remove(serviceId);
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
        }

    }
}
