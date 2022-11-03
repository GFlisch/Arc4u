using Arc4u.OAuth2.Token;
using Arc4u.Standard.OAuth2.Events;
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
    public static partial class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, AuthenticationOptions authenticationOptions)
        {
            ArgumentNullException.ThrowIfNull(authenticationOptions);

            ArgumentNullException.ThrowIfNull(authenticationOptions.OAuthSettings);

            ArgumentNullException.ThrowIfNull(authenticationOptions.OpenIdSettings);

            services.AddAuthorization();
            services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
            //    // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
            //    options.HandleSameSiteCookieCompatibility();
            //});

            // OAuth.
            var (instance, tenantId) = ExtractFromAuthority(authenticationOptions.OAuthSettings);

            var authenticationBuilder = services
                    .AddAuthentication(auth =>
                    {
                        auth.DefaultAuthenticateScheme = Constants.ChallengePolicyScheme;
                        auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
                    })
                    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                    {
                        // cookie will not be limited in time by the life time of the access token.
                        options.UseTokenLifetime = false;
                        options.Authority = authenticationOptions.OpenIdSettings.Values[TokenKeys.AuthorityKey]; ;
                        options.RequireHttpsMetadata = false; // do we force? Docker image for testing...
                        options.MetadataAddress = authenticationOptions.MetadataAddress;
                        options.ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
                        options.ClientId = authenticationOptions.OpenIdSettings.Values[TokenKeys.ClientIdKey];
                        options.ClientSecret = authenticationOptions.OpenIdSettings.Values[TokenKeys.ApplicationKey];
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.TokenValidationParameters.SaveSigninToken = false;
                        options.TokenValidationParameters.AuthenticationType = Constants.CookiesAuthenticationType;
                        options.TokenValidationParameters.ValidateAudience = false;
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.SaveTokens = true;
                        options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                        options.EventsType = typeof(CustomOpenIdConnectEvents);
                    })
                    .AddJwtBearer(option =>
                    {
                        option.RequireHttpsMetadata = false;
                        option.Authority = "http://localhost:8080/realms/Test/protocol/openid-connect/auth";
                        option.MetadataAddress = "http://localhost:8080/realms/Test/.well-known/openid-configuration";
                        option.SaveToken = true;
                        option.TokenValidationParameters.SaveSigninToken = false;
                        option.TokenValidationParameters.AuthenticationType = "OAuth2Bearer";
                        option.TokenValidationParameters.ValidateIssuer = false;
                        option.TokenValidationParameters.ValidateAudience = true;
                        option.TokenValidationParameters.ValidAudiences = new[] { "account" };
                        option.EventsType = typeof(CustomBearerEvents);
                    });



            // OpenId Connect
            (instance, tenantId) = ExtractFromAuthority(authenticationOptions.OpenIdSettings);



            return authenticationBuilder;

        }

        private static (string instance, string tenantId) ExtractFromAuthority(IKeyValueSettings settings)
        {
            var authority = new Uri(settings.Values[TokenKeys.AuthorityKey]);

            var instance = authority.GetLeftPart(UriPartial.Authority);
            var tenantId = authority.AbsolutePath.Trim(new char[] { '/', ' ' });

            if (settings.Values.ContainsKey(TokenKeys.TenantIdKey))
                tenantId = settings.Values[TokenKeys.TenantIdKey];

            if (settings.Values.ContainsKey(TokenKeys.InstanceKey))
                instance = settings.Values[TokenKeys.InstanceKey];

            return (instance, tenantId);
        }
    }
}
