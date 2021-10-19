using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.Identity.Web;
using System;
using System.Linq;
using AuthenticationOptions = Arc4u.Standard.OAuth2.Extensions.AuthenticationOptions;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Options;

namespace Arc4u.Standard.OAuth2
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddMsalAuthentication(this IServiceCollection services, AuthenticationOptions authenticationOptions)
        {
            if (null == authenticationOptions)
                throw new ArgumentNullException(nameof(authenticationOptions));

            if (null == authenticationOptions.OAuthSettings)
                throw new ArgumentNullException(nameof(authenticationOptions.OAuthSettings));

            if (null == authenticationOptions.OpenIdSettings)
                throw new ArgumentNullException(nameof(authenticationOptions.OpenIdSettings));

            services.AddAuthorization();
            services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            // OAuth.
            var (instance, tenantId) = ExtractFromAuthority(authenticationOptions.OAuthSettings);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi((bearerOptions) =>
                    {
                        bearerOptions.RequireHttpsMetadata = true;
                        bearerOptions.MetadataAddress = authenticationOptions.MetadataAddress;
                        bearerOptions.Authority = authenticationOptions.OAuthSettings.Values[TokenKeys.AuthorityKey];
                        bearerOptions.TokenValidationParameters.SaveSigninToken = true;
                        bearerOptions.TokenValidationParameters.AuthenticationType = Constants.BearerAuthenticationType;
                        bearerOptions.TokenValidationParameters.ValidateAudience = true;
                        bearerOptions.TokenValidationParameters.ValidAudiences = new[] { authenticationOptions.OAuthSettings.Values[TokenKeys.ServiceApplicationIdKey] };
                    }, (identityOptions) =>
                    {
                        identityOptions.MetadataAddress = authenticationOptions.MetadataAddress;
                        identityOptions.RequireHttpsMetadata = true;
                        identityOptions.Authority = authenticationOptions.OAuthSettings.Values[TokenKeys.AuthorityKey];
                        identityOptions.Instance = instance;
                        identityOptions.TenantId = tenantId;
                        identityOptions.ClientId = authenticationOptions.OAuthSettings.Values[TokenKeys.ClientIdKey];
                        identityOptions.ClientSecret = authenticationOptions.OAuthSettings.Values[TokenKeys.ApplicationKey];
                        identityOptions.TokenValidationParameters.SaveSigninToken = true;
                        identityOptions.TokenValidationParameters.AuthenticationType = Constants.BearerAuthenticationType;
                        identityOptions.TokenValidationParameters.ValidateAudience = true;
                        identityOptions.TokenValidationParameters.ValidAudiences = new[] { authenticationOptions.OAuthSettings.Values[TokenKeys.ServiceApplicationIdKey] };
                    })
                    .EnableTokenAcquisitionToCallDownstreamApi((options) =>
                    {
                        options.ClientSecret = authenticationOptions.OAuthSettings.Values[TokenKeys.ApplicationKey];
                        options.ClientId = authenticationOptions.OAuthSettings.Values[TokenKeys.ClientIdKey];
                        options.Instance = instance;
                        options.TenantId = tenantId;
                    }).AddInMemoryTokenCaches();


            // OpenId Connect
            (instance, tenantId) = ExtractFromAuthority(authenticationOptions.OpenIdSettings);

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
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
                .AddMicrosoftIdentityWebApp(options =>
                {
                    options.MetadataAddress = authenticationOptions.MetadataAddress;
                    options.RequireHttpsMetadata = true;
                    options.Instance = instance;
                    options.ClientId = authenticationOptions.OpenIdSettings.Values[TokenKeys.ClientIdKey];
                    options.TenantId = tenantId;
                    options.ClientSecret = authenticationOptions.OpenIdSettings.Values[TokenKeys.ApplicationKey];
                    options.TokenValidationParameters.SaveSigninToken = true;
                    options.TokenValidationParameters.AuthenticationType = Constants.CookiesAuthenticationType;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudiences = new[] { authenticationOptions.OpenIdSettings.Values[TokenKeys.ServiceApplicationIdKey] };
                }).EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            return services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = Constants.ChallengePolicyScheme;
                auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
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
