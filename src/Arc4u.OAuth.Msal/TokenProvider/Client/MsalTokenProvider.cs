using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.AspNetCore;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;

namespace Arc4u.OAuth2.Msal.TokenProvider.Client;

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

    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (null != _applicationContext.Principal)
        {
            var identity = _applicationContext.Principal.Identity as ClaimsIdentity;

            if (null != identity && !string.IsNullOrWhiteSpace(identity?.BootstrapContext?.ToString()))
            {
                var token = identity.BootstrapContext.ToString();

                JwtSecurityToken jwt = new(token);

                if (jwt.ValidTo > DateTime.UtcNow)
                {
                    return Result.Ok(new TokenInfo("Bearer", token!, jwt.ValidTo));
                }
            }
        }

        if (null == _publicClientApplication)
        {
            return Result.Fail("No public client defined.");
        }

        var accounts = await _publicClientApplication.PublicClient.GetAccountsAsync().ConfigureAwait(false);
        var firstAccount = accounts.FirstOrDefault();

        var scopes = settings.Values[TokenKeys.Scopes].Split([','], StringSplitOptions.RemoveEmptyEntries);
        AuthenticationResult authResult;
        try
        {
            authResult = await _publicClientApplication.PublicClient.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync().ConfigureAwait(false);

            JwtSecurityToken jwt = new(authResult.AccessToken);
            return Result.Ok(new TokenInfo("Bearer", authResult.AccessToken, jwt.ValidTo));
        }
        catch (MsalUiRequiredException ex)
        {
            // A MsalUiRequiredException happened on AcquireTokenSilent.
            // This indicates you need to call AcquireTokenInteractive to acquire a token
            _logger.Technical().LogMsalUiRequiredException(ex.Message);

            try
            {
                var builder = _publicClientApplication.PublicClient
                                                      .AcquireTokenInteractive(scopes)
                                                      .WithAccount(accounts.FirstOrDefault())
                                                      .WithPrompt(Prompt.SelectAccount);

                if (_publicClientApplication.HasCustomWebUi)
                {
                    builder.WithCustomWebUi(_publicClientApplication.CustomWebUi);
                }

                authResult = await builder.ExecuteAsync().ConfigureAwait(false);

                JwtSecurityToken jwt = new(authResult.AccessToken);
                return Result.Ok(new TokenInfo("Bearer", authResult.AccessToken, jwt.ValidTo));
            }
            catch (MsalException msalex)
            {
                return new ExceptionalError(msalex);
            }
        }
        catch (Exception ex)
        {
            return new ExceptionalError(ex);
        }
    }

    public async ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        if (null != _publicClientApplication)
        {
            var accounts = await _publicClientApplication.PublicClient.GetAccountsAsync().ConfigureAwait(false);

            if (accounts.Any())
            {
                try
                {
                    await _publicClientApplication.PublicClient.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(false);
                }
                catch (MsalException msalex)
                {
                    _logger.Technical().LogException(msalex);
                }
            }
        }
    }
}

