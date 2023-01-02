using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace Arc4u.Standard.OAuth2.Extensions;

public static partial class AuthenticationExtensions
{
    public static AuthenticationBuilder AddOidcAuthentication(this IServiceCollection services, Action<OidcAuthenticationOptions> authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(authenticationOptions);

        var oidcOptions = new OidcAuthenticationOptions();
        authenticationOptions(oidcOptions);

        ArgumentNullException.ThrowIfNull(oidcOptions.OAuth2Settings);
        ArgumentNullException.ThrowIfNull(oidcOptions.OpenIdSettings);

        // Will keep in memory the AccessToken and Refresh token for the time of the request...
        services.Configure(authenticationOptions);
        services.AddScoped<TokenRefreshInfo>();
        services.AddAuthorization();
        services.AddHttpContextAccessor(); // give access to the HttpContext if requested by an external packages.
        services.AddTransient(oidcOptions.CookieAuthenticationEventsType);
        services.AddTransient(oidcOptions.JwtBearerEventsType);
        services.AddTransient(oidcOptions.OpenIdConnectEventsType);

        // OAuth2.
        SecurityKey? securityKey = oidcOptions.CertSecurityKey is not null ? new X509SecurityKey(oidcOptions.CertSecurityKey) : null;

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
                    options.UsePkce= true; // Impact on th security. It is best to do this...
                    options.UseTokenLifetime = false;
                    options.SaveTokens = false;
                    options.Authority = oidcOptions.OpenIdSettings.Values[TokenKeys.AuthorityKey];
                    options.RequireHttpsMetadata = false; // do we force? Docker image for testing...
                    options.MetadataAddress = oidcOptions.MetadataAddress;
                    // for the other OIDC
                    //options.ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
                    // for AzureAD
                    options.ResponseType = OpenIdConnectResponseType.Code;


                    options.Scope.Clear();
                    options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                    options.Scope.Add(OpenIdConnectScope.OfflineAccess);
                    foreach (var scope in SplitString(oidcOptions.OpenIdSettings.Values[TokenKeys.Scopes]))
                        options.Scope.Add(scope);

                    options.ClientId = oidcOptions.OpenIdSettings.Values[TokenKeys.ClientIdKey];
                    options.ClientSecret = oidcOptions.OpenIdSettings.Values[TokenKeys.ApplicationKey];
                    // we don't call the user info endpoint => On AzureAd the user.read scope is needed.
                    options.GetClaimsFromUserInfoEndpoint = false;
                    
                    options.TokenValidationParameters.SaveSigninToken = false;
                    options.TokenValidationParameters.AuthenticationType = Constants.CookiesAuthenticationType;
                    options.TokenValidationParameters.ValidateAudience = true;
                    // we will use the same key to generate and validate so we can use this also in the different services...
                    if (securityKey is not null)
                    {
                        options.TokenValidationParameters.IssuerSigningKey = securityKey;
                    }
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.SaveTokens = true;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                    // AzureAD
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost;
                    
                    options.EventsType = oidcOptions.OpenIdConnectEventsType;
                })
                .AddJwtBearer(option =>
                {
                    option.RequireHttpsMetadata = false;
                    option.Authority = oidcOptions.OAuth2Settings.Values[TokenKeys.AuthorityKey];
                    option.MetadataAddress = oidcOptions.MetadataAddress;
                    option.SaveToken = true;
                    option.TokenValidationParameters.SaveSigninToken = false;
                    option.TokenValidationParameters.AuthenticationType = Constants.BearerAuthenticationType;
                    option.TokenValidationParameters.ValidateIssuer = false;
                    option.TokenValidationParameters.ValidateAudience = true;
                    option.TokenValidationParameters.ValidAudiences = SplitString(oidcOptions.OAuth2Settings.Values[Audiences]);
                    if (securityKey is not null)
                        option.TokenValidationParameters.IssuerSigningKey = securityKey;

                    option.EventsType = oidcOptions.JwtBearerEventsType;
                }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = oidcOptions.CookieName;
                    options.SessionStore = oidcOptions.TicketStore;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.EventsType = oidcOptions.CookieAuthenticationEventsType;
                });

        return authenticationBuilder;

    }

    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, Action<JwtAuthenticationOptions> authenticationOptions)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        ArgumentNullException.ThrowIfNull(authenticationOptions, nameof(authenticationOptions));

        var options = new JwtAuthenticationOptions();
        authenticationOptions(options);

        ArgumentNullException.ThrowIfNull(options.OAuth2Settings, nameof(options.OAuth2Settings));
        ArgumentNullException.ThrowIfNull(options.JwtBearerEventsType, nameof(options.JwtBearerEventsType));
        ArgumentNullException.ThrowIfNull(options.MetadataAddress, nameof(options.MetadataAddress));

        services.Configure(authenticationOptions);

        services.AddTransient(options.JwtBearerEventsType);
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        var authenticationBuilder = 
        services.AddAuthentication(auth => auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    SecurityKey? securityKey = options.CertSecurityKey is not null ? new X509SecurityKey(options.CertSecurityKey) : null;

                    option.RequireHttpsMetadata = false;
                    option.Authority = options.OAuth2Settings.Values[TokenKeys.AuthorityKey];
                    option.MetadataAddress = options.MetadataAddress;
                    option.SaveToken = true;
                    option.TokenValidationParameters.SaveSigninToken = false;
                    option.TokenValidationParameters.AuthenticationType = Constants.BearerAuthenticationType;
                    option.TokenValidationParameters.ValidateIssuer = false;
                    option.TokenValidationParameters.ValidateAudience = true;
                    option.TokenValidationParameters.ValidAudiences = SplitString(options.OAuth2Settings.Values[Audiences]);
                    if (securityKey is not null)
                        option.TokenValidationParameters.IssuerSigningKey = securityKey;
                    option.EventsType = options.JwtBearerEventsType;
                });

        return authenticationBuilder;
    }

    static string[] SplitString(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

    public const string Audiences = "Audiences";


}