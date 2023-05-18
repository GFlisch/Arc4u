using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Msal.Token;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Arc4u.OAuth2.Extensions;

public static partial class AuthenticationExtensions
{
    public static AuthenticationBuilder AddMsalB2CAuthentication(this IServiceCollection services, MsalAuthenticationOptions authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(authenticationOptions);

        if (authenticationOptions.OAuthSettings is null)
        {
            throw new ArgumentNullException(nameof(authenticationOptions), $"{nameof(authenticationOptions.OAuthSettings)} is not defined");
        }

        if (authenticationOptions.OpenIdSettings is null)
        {
            throw new ArgumentNullException(nameof(authenticationOptions), $"{nameof(authenticationOptions.OpenIdSettings)} is not defined");
        }

        if (authenticationOptions.ClaimsIdentifierOptions is null)
        {
            throw new ArgumentNullException(nameof(authenticationOptions), $"{nameof(authenticationOptions.ClaimsIdentifierOptions)} is not defined");
        }

        services.AddClaimsIdentifier(authenticationOptions.ClaimsIdentifierOptions);
        services.AddAuthorization();
        services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.
        services.TryAddSingleton<IMsalTokenCacheProvider, Cache>();

        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
            // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
            options.HandleSameSiteCookieCompatibility();
        });

        // OAuth.
        var (instance, policy, tenantId) = ExtractFromB2CAuthority(authenticationOptions.OAuthSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi((bearerOptions) =>
                {
                    bearerOptions.RequireHttpsMetadata = true;
                    bearerOptions.MetadataAddress = authenticationOptions.MetadataAddress;
                    bearerOptions.Authority = authenticationOptions.OAuthSettings.Values[TokenKeys.AuthorityKey];
                    bearerOptions.TokenValidationParameters.SaveSigninToken = true;
                    bearerOptions.TokenValidationParameters.AuthenticationType = authenticationOptions.OAuthSettings.Values[TokenKeys.AuthenticationTypeKey];
                    bearerOptions.TokenValidationParameters.ValidateAudience = true;
                    bearerOptions.TokenValidationParameters.ValidAudiences = new[] { authenticationOptions.OAuthSettings.Values[TokenKeys.ServiceApplicationIdKey] };
                    bearerOptions.TokenValidationParameters.NameClaimType = "name";
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
                    identityOptions.TokenValidationParameters.AuthenticationType = authenticationOptions.OAuthSettings.Values[TokenKeys.AuthenticationTypeKey];
                    identityOptions.TokenValidationParameters.ValidateAudience = true;
                    identityOptions.TokenValidationParameters.ValidAudiences = new[] { authenticationOptions.OAuthSettings.Values[TokenKeys.ServiceApplicationIdKey] };
                })
                .EnableTokenAcquisitionToCallDownstreamApi((options) =>
                {
                    options.ClientSecret = authenticationOptions.OAuthSettings.Values[TokenKeys.ApplicationKey];
                    options.ClientId = authenticationOptions.OAuthSettings.Values[TokenKeys.ClientIdKey];
                    options.Instance = instance;
                    options.TenantId = tenantId;
                });

        (instance, policy, tenantId) = ExtractFromB2CAuthority(authenticationOptions.OpenIdSettings);

        var scopes = authenticationOptions.OpenIdSettings.Values[TokenKeys.Scopes].Split(',', ';');

        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                options.ClientId = authenticationOptions.OpenIdSettings.Values[TokenKeys.ClientIdKey];
                options.ClientSecret = authenticationOptions.OpenIdSettings.Values[TokenKeys.ApplicationKey];
                options.Authority = authenticationOptions.OpenIdSettings.Values[TokenKeys.AuthorityKey];
                options.Instance = instance;
                options.Domain = tenantId;
                if (authenticationOptions.OpenIdSettings.Values.ContainsKey("SignedOutCallbackPath"))
                    options.SignedOutCallbackPath = authenticationOptions.OpenIdSettings.Values["SignedOutCallbackPath"];
                options.SignUpSignInPolicyId = authenticationOptions.OpenIdSettings.Values["SignUpSignInPolicyId"];
                options.ForwardDefaultSelector = ctx =>
                {
                    var authHeader = ctx?.Request?.Headers[HeaderNames.Authorization].FirstOrDefault();
                    if (authHeader?.StartsWith("Bearer ") == true)
                        return JwtBearerDefaults.AuthenticationScheme;

                    return null;
                };
                options.CallbackPath = new Uri(authenticationOptions.OpenIdSettings.Values[TokenKeys.RedirectUrl]).AbsolutePath;
                options.TokenValidationParameters.SaveSigninToken = false;
                options.TokenValidationParameters.AuthenticationType = authenticationOptions.OpenIdSettings.Values[TokenKeys.AuthenticationTypeKey];
                options.TokenValidationParameters.ValidateAudience = true;
            })
            .EnableTokenAcquisitionToCallDownstreamApi();

        return services.AddAuthentication();
    }

    // https://<host>/tfp/<tenant>/<policy>
    private static (string instance, string policy, string tenant) ExtractFromB2CAuthority(IKeyValueSettings settings)
    {
        var authority = settings.Values[TokenKeys.AuthorityKey];

        // validate the authority.
        var expression = new Regex(@"https://(\S+)/tfp/(\S+)/(\S+)(/\S+)?");

        var match = expression.Match(authority);

        if (!match.Success)
            throw new ApplicationException("B2C 'authority' Uri should have at least 3 segments in the path (i.e. https://<host>/tfp/<tenant>/<policy>/...). ");

        var instance = $"https://{match.Groups[1].Value}";
        var tenant = match.Groups[2].Value;
        var policy = match.Groups[3].Value;

        return (instance, policy, tenant);
    }
}
