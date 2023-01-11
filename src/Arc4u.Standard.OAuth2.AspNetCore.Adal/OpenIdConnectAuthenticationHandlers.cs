using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Security;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Token.Adal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Adal;

public static class OpenIdConnectAuthenticationHandlers
{
    public static async Task<string> AuthorizationCodeReceived(IServiceProvider serviceProvider, IKeyValueSettings openIdSettings, ClaimsPrincipal principal, string code, string redirectUri, bool validateAuthority = false)
    {
        IContainerResolve container = serviceProvider.GetService(typeof(IContainerResolve)) as IContainerResolve;
        if (null == container)
            throw new NullReferenceException("No container resolve is defined.");


        container.TryResolve<ILogger>(out var logger);

        if (!container.TryResolve<IUserObjectIdentifier>(out var keyGen))
        {
            logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers)).Error($"No Cache key generator is registered. Unable to uniquely identify a user.").Log();
            throw new NullReferenceException("IUserObjectIdentifier is not regeistered in the DI container.");
        }

        try
        {
            var userObjectId = keyGen.GetIdentifer(principal.Identity as ClaimsIdentity);
            var authority = openIdSettings.Values[TokenKeys.AuthorityKey];
            var resource = openIdSettings.Values[TokenKeys.ServiceApplicationIdKey];

            logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers))
                    .System($"Get authenticationContext for authority: {authority}, resource: {resource}, user identifier: {userObjectId}.")
                    .Add("Authority", authority)
                    .Add("Resource", resource)
                    .Add("UserID", userObjectId)
                    .Log();

            var credential = new ClientCredential(openIdSettings.Values[TokenKeys.ClientIdKey], openIdSettings.Values[TokenKeys.ApplicationKey]);


            var authContext = new AuthenticationContext(authority, validateAuthority, new Cache(logger, container, resource + openIdSettings.Values[TokenKeys.AuthenticationTypeKey] + userObjectId));
            logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers)).System($"Acquire a token based on the OpenID code received for redirectUri: {redirectUri}, credential.ClientId {credential.ClientId}, resource: {resource}.")
                    .Add("RedirectUri", redirectUri)
                    .Add("ClientId", credential.ClientId)
                    .Add("Resource", resource)
                    .Log();



            var result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                                code,
                                new Uri(redirectUri, UriKind.RelativeOrAbsolute),
                                credential,
                                credential.ClientId);
            logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers)).System($"Token is acquired and expired at {result.ExpiresOn}.").Log();
            if (null != result.UserInfo)
            {
                logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers)).System($"Token uniqueId = {result.UserInfo.UniqueId}").Log();
                logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers)).System($"Token displayableId = {result.UserInfo.DisplayableId}").Log();
            }
            else
                logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers))
                        .Warning("No OpenId Token was received based on an code.")
                        .Add("Code", code)
                        .Log();

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            logger?.Technical().From(typeof(OpenIdConnectAuthenticationHandlers)).Exception(ex).Log();
            throw;
        }

    }
}