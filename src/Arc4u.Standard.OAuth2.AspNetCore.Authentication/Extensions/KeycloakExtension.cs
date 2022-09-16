using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Extensions
{
    public static partial class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddKeycloakAuthentication(this IServiceCollection services, AuthenticationOptions authenticationOptions)
        {
            ArgumentNullException.ThrowIfNull(authenticationOptions);

            ArgumentNullException.ThrowIfNull(authenticationOptions.OAuthSettings);

            ArgumentNullException.ThrowIfNull(authenticationOptions.OpenIdSettings);

            services.AddAuthorization();
            services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
            });

            // Defined the scheme used to choose between an OpenIdConnect or Bearer.
            services.AddAuthentication(auth =>
             {
                 auth.DefaultAuthenticateScheme = Constants.ChallengePolicyScheme;
                 auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                 auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
             })
            // JwtBearer.
            .AddJwtBearer(option =>
            {
                // Check the parameters received in the AuthenticationOptions for jwtBearer.

                option.RequireHttpsMetadata = false;
                option.Authority = "http://localhost:8080/realms/Test/protocol/openid-connect/auth";
                option.MetadataAddress = "http://localhost:8080/realms/Test/.well-known/openid-configuration";
                option.SaveToken = true;
                option.TokenValidationParameters.SaveSigninToken = false;
                option.TokenValidationParameters.AuthenticationType = "OAuth2Bearer";
                option.TokenValidationParameters.ValidateIssuer = false;
                option.TokenValidationParameters.ValidateAudience = false;
                //option.TokenValidationParameters.ValidAudiences = new[] { authenticationOptions.OAuthSettings.Values[TokenKeys.ServiceApplicationIdKey] };
                option.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = c =>
                    {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Return that due to the issue regarding the authentication. The person is not authorized.
                        // Log the information.
                        context.Response.Clear();
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnChallenge = c =>
                    {
                        return Task.CompletedTask;
                    },
                    OnForbidden = c =>
                    {
                        return Task.CompletedTask;
                    }
                };
            })

            ;
            return services.AddAuthentication();
        }
    }
}
