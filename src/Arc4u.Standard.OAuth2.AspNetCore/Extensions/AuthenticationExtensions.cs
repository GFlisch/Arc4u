using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;

namespace Arc4u.Standard.OAuth2.Extensions
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddOpenIdBearerAuthentication(this IServiceCollection services, AuthenticationOptions authenticationOptions)
        {
            if (null == authenticationOptions)
                throw new ArgumentNullException(nameof(authenticationOptions));

            if (null == authenticationOptions.OAuthSettings)
                throw new ArgumentNullException(nameof(authenticationOptions.OAuthSettings));

            if (null == authenticationOptions.OpenIdSettings)
                throw new ArgumentNullException(nameof(authenticationOptions.OpenIdSettings));

            services.AddAuthorization();
            services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.

            return services.AddAuthentication(auth =>
                    {
                        auth.DefaultAuthenticateScheme = Constants.ChallengePolicyScheme;
                        auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(option =>
                    {
                        option.RequireHttpsMetadata = false;
                        option.Authority = authenticationOptions.OAuthSettings.Values[TokenKeys.AuthorityKey];
                        option.TokenValidationParameters.SaveSigninToken = true;
                        option.TokenValidationParameters.AuthenticationType = Constants.BearerAuthenticationType;
                        option.TokenValidationParameters.ValidateIssuer = false;
                        option.TokenValidationParameters.ValidateAudience = true;
                        option.TokenValidationParameters.ValidAudiences = new[] { authenticationOptions.OAuthSettings.Values[TokenKeys.ServiceApplicationIdKey] };
                    })
                    .AddCookie(option =>
                    {
                        option.CookieManager = authenticationOptions.CookieManager;
                        option.Cookie.Name = authenticationOptions.CookieName;
                    })
                    .AddOpenIdConnect(option =>
                    {
                        option.MetadataAddress = authenticationOptions.MetadataAddress;
                        option.RequireHttpsMetadata = true;
                        option.ClientId = authenticationOptions.OpenIdSettings.Values[TokenKeys.ClientIdKey];
                        option.ClientSecret = authenticationOptions.OpenIdSettings.Values[TokenKeys.ApplicationKey];
                        option.Authority = authenticationOptions.OpenIdSettings.Values[TokenKeys.AuthorityKey];
                        option.TokenValidationParameters.SaveSigninToken = false;
                        option.TokenValidationParameters.AuthenticationType = Constants.CookiesAuthenticationType;
                        option.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                        option.Events.OnAuthorizationCodeReceived = async context =>
                        {
                            var accessToken = await authenticationOptions.OnAuthorizationCodeReceived(context.HttpContext.RequestServices,
                                                                               authenticationOptions.OpenIdSettings,
                                                                               context.Principal,
                                                                               context.TokenEndpointRequest.Code,
                                                                               context.TokenEndpointRequest.RedirectUri,
                                                                               authenticationOptions.ValidateAuthority);

                            context.HandleCodeRedemption(accessToken, context.ProtocolMessage.IdToken);
                        };
                    })
                    .AddPolicyScheme(Constants.ChallengePolicyScheme, "Authorization Bearer or OIDC", options =>
                    {
                        options.ForwardDefaultSelector = context =>
                        {
                            var authHeader = context.Request.Headers[HeaderNames.Authorization].FirstOrDefault();
                            if (authHeader?.StartsWith("Bearer ") == true)
                                return JwtBearerDefaults.AuthenticationScheme;

                            return OpenIdConnectDefaults.AuthenticationScheme;

                        };
                    });
        }
    }
}
