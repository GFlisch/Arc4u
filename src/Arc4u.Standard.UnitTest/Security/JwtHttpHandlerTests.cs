using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Arc4u.Caching;
using Arc4u.Configuration;
using Arc4u.Dependency.ComponentModel;
using Arc4u.Diagnostics;
using Arc4u.OAuth2;
using Arc4u.OAuth2.AspNetCore;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using Arc4u.OAuth2.TokenProviders;
using Arc4u.Security.Principal;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Arc4u.UnitTest.Security;

public class JwtHandlerToTest : JwtHttpHandler
{
    public JwtHandlerToTest(IScopedServiceProviderAccessor scopedServiceProviderAccessor, ILogger<JwtHandlerToTest> logger, IOptionsMonitor<SimpleKeyValueSettings> keyValuesSettingsOption, string resolvingName) : base(scopedServiceProviderAccessor, logger, keyValuesSettingsOption.Get(resolvingName))
    {
    }
}

/// <summary>
/// This test will control the different scenario defined for the usage of the JWtHttpHandler.
/// The handler can be used in the following case:
/// 1) Retrieve the bearer token when used with a user principal authenticated via an OpenId Connect scenario): AuthenticationType is OpenId.
/// 2) Retrieve the bearer token when used with a user principal authenticated in an Api call: AuthenticationType is OAuth2Bearer.
/// 3) By using a CLientSecret definition (username:password) and injecting the bearer token retrieve from a call to the authority provider: Authentication type is Inject.
/// 4) By injecting an encrypted username:password in the header of the http request : RemoteSecret token provider and AuthenticationType is Inject.
/// 5) By injecting a Basic authorization header during the http request: RemoteSecret, header key is Basic and AuthenticationType is Inject.
/// 6) Have an on behalf of scenario based on the scenario 1 or 2.
/// 7) Chain 2 Handlers: OAuth2Bearer + Cookies and the access token returned is the OAuth2 one
/// 8) Chain 2 Handlers: OAuth2Bearer + Cookies and the access token returned is the Cookies one
/// 9) Chain 3 Handlers: Oauth2Bearer + Cookies + Inject but inject is not occuring because a OAuth2Bearer is already available.
/// </summary>
public class JwtHttpHandlerTests
{
    public JwtHttpHandlerTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    // Scenario 1
    public async Task Jwt_With_OAuth2_And_Principal_With_OIDC_Token_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:OpenId.Settings:ClientId"] = "aa17786b-e33c-41ec-81cc-6063610aedeb",
                             ["Authentication:OpenId.Settings:ClientSecret"] = "This is a secret",
                             ["Authentication:OpenId.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OpenId.Settings:Scopes:0"] = "user.read",
                             ["Authentication:OpenId.Settings:Scopes:1"] = "user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddDefaultAuthority(configuration);
        services.ConfigureOpenIdSettings(configuration, "Authentication:OpenId.Settings");
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddScoped<TokenRefreshInfo>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        var mockTokenRefresh = _fixture.Freeze<Mock<ITokenRefreshProvider>>();
        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, OidcTokenProvider>(OidcTokenProvider.ProviderName);
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);
        container.RegisterInstance<ITokenRefreshProvider>(mockTokenRefresh.Object);
        container.CreateContainer();

        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();

        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        var tokenRefresh = scopedContainer.Resolve<TokenRefreshInfo>();
        tokenRefresh.RefreshToken = new TokenInfo("refresh_token", Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
        tokenRefresh.AccessToken = new TokenInfo("access_token", accessToken);

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.CookiesAuthenticationType), "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, Constants.OpenIdOptionsName)
        {
            InnerHandler = innerHandler.Object
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(accessToken);
    }

    [Fact]
    // Scenario 2
    public async Task Jwt_With_OAuth2_And_Principal_With_Bearer_Token_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:OAuth2.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OAuth2.Settings:Scopes"] = "user.read user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddDefaultAuthority(configuration);
        services.ConfigureOAuth2Settings(configuration, "Authentication:OAuth2.Settings");
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, BootstrapContextTokenProvider>("Bootstrap");
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);
        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType) { BootstrapContext = accessToken }, "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, "OAuth2")
        {
            InnerHandler = innerHandler.Object
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(accessToken);
    }

    [Fact]
    // Scenario 3
    // When we inject. There is no need to have a principal!
    public async Task Jwt_With_ClientSecet_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var configDic = new Dictionary<string, string?>
        {
            ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
            ["Authentication:ClientSecrets:Client1:User"] = options.User,
            ["Authentication:ClientSecrets:Client1:Credential"] = $"{options.User}:password",
            ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
        };
        foreach (var scope in options.Scopes)
        {
            configDic.Add($"Authentication:ClientSecrets:Client1:Scopes:{options.Scopes.IndexOf(scope)}", scope);
        }
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(configDic).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddSecretAuthentication(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddDefaultAuthority(configuration);

        // Mock the CredentialDiect (Calling the authorize endpoint based on a user and password!)
        var mockSecretTokenProvider = _fixture.Freeze<Mock<ICredentialTokenProvider>>();
        mockSecretTokenProvider.Setup(m => m.GetTokenAsync(It.IsAny<IKeyValueSettings>(), It.IsAny<CredentialsResult>()))
                               .ReturnsAsync(new TokenInfo("Bearer", accessToken));

        // Mock the cache used by the Credential token provider.
        var mockTokenCache = _fixture.Freeze<Mock<ITokenCache>>();
        mockTokenCache.Setup(m => m.Get<TokenInfo>(It.IsAny<string>())).Returns((TokenInfo)null);
        mockTokenCache.Setup(m => m.Put<TokenInfo>(It.IsAny<string>(), It.IsAny<TokenInfo>()));

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.RegisterInstance<ICredentialTokenProvider>(mockSecretTokenProvider.Object, "CredentialDirect");
        container.Register<ITokenProvider, CredentialSecretTokenProvider>("ClientSecret");
        container.Register<ICredentialTokenProvider, CredentialTokenCacheTokenProvider>("Credential");
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);
        container.RegisterInstance<ITokenCache>(mockTokenCache.Object);

        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, "Client1")
        {
            InnerHandler = innerHandler.Object
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(accessToken);
    }

    [Fact]
    // Scenario 4
    // When we inject. There is no need to have a principal!
    public async Task Jwt_With_RemoteSecreInjected_With_Basic_Authorization_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var options = _fixture.Create<RemoteSecretSettingsOptions>();
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:RemoteSecrets:Remote1:ClientSecret"] = options.ClientSecret,
                             ["Authentication:RemoteSecrets:Remote1:HeaderKey"] = "Basic",
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddRemoteSecretsAuthentication(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, RemoteClientSecretTokenProvider>(RemoteClientSecretTokenProvider.ProviderName);
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);

        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, "Remote1")
        {
            InnerHandler = innerHandler.Object
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Basic");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(options.ClientSecret);
    }

    [Fact]
    // Scenario 5
    // When we inject. There is no need to have a principal!
    public async Task Jwt_With_RemoteSecreInjected_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var options = _fixture.Create<RemoteSecretSettingsOptions>();
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:RemoteSecrets:Remote1:ClientSecret"] = options.ClientSecret,
                             ["Authentication:RemoteSecrets:Remote1:HeaderKey"] = options.HeaderKey,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddRemoteSecretsAuthentication(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, RemoteClientSecretTokenProvider>(RemoteClientSecretTokenProvider.ProviderName);
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);

        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, "Remote1")
        {
            InnerHandler = innerHandler.Object
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().BeNull();
        httpRequestMessage.Headers.Contains(options.HeaderKey).Should().BeTrue();
        httpRequestMessage.Headers.GetValues(options.HeaderKey).FirstOrDefault().Should().Be(options.ClientSecret);
    }

    [Fact]
    // Scenario 6
    public async Task Jwt_With_OAuth2_And_Principal_With_Bearer_Token_And_On_Behalf_of_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var options = _fixture.Create<SecretBasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:OnBehalfOf:Obo:ClientId"] = "aa17786b-e33c-41ec-81cc-6063610aedeb",
                             ["Authentication:OnBehalfOf:Obo:ClientSecret"] = "This is a secret",
                             ["Authentication:OnBehalfOf:Obo:Scopes:0"] = "user.read",
                             ["Authentication:OnBehalfOf:Obo:Scopes:1"] = "user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddDefaultAuthority(configuration);
        services.AddOnBehalfOf(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddScoped<TokenRefreshInfo>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        var mockActivitySourceFactory = new Mock<IActivitySourceFactory>();
        mockActivitySourceFactory.Setup(m => m.Get("Arc4u", null)).Returns((ActivitySource)null);
        services.AddSingleton<IActivitySourceFactory>(mockActivitySourceFactory.Object);

        // Uses the cache to return the access token in the Obo provider => avoid any call to the Authority!
        var mockCache = _fixture.Freeze<Mock<ICache>>();
        mockCache.Setup(m => m.GetAsync<TokenInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new TokenInfo("Bearer", accessToken));

        var mockCacheHelper = _fixture.Freeze<Mock<ICacheHelper>>();
        mockCacheHelper.Setup(m => m.GetCache()).Returns(mockCache.Object);

        services.AddSingleton<ICacheHelper>(mockCacheHelper.Object);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, AzureADOboTokenProvider>(AzureADOboTokenProvider.ProviderName);
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);
        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType) { BootstrapContext = accessToken }, "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, "Obo")
        {
            InnerHandler = innerHandler.Object
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(accessToken);
    }

    [Fact]
    // Scenario 7
    public async Task Jwt_With_OAuth2_And_OIDC_With_OAuth2_As_The_Winner_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:OAuth2.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OAuth2.Settings:Scopes"] = "user.read user.write",
                             ["Authentication:OpenId.Settings:ClientId"] = "aa17786b-e33c-41ec-81cc-6063610aedeb",
                             ["Authentication:OpenId.Settings:ClientSecret"] = "This is a secret",
                             ["Authentication:OpenId.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OpenId.Settings:Scopes:0"] = "user.read",
                             ["Authentication:OpenId.Settings:Scopes:1"] = "user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwtOAuth2 = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessTokenOAuth2 = new JwtSecurityTokenHandler().WriteToken(jwtOAuth2);

        var jwtCookies = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessTokenCookies = new JwtSecurityTokenHandler().WriteToken(jwtCookies);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddDefaultAuthority(configuration);
        services.ConfigureOAuth2Settings(configuration, "Authentication:OAuth2.Settings");
        services.ConfigureOpenIdSettings(configuration, "Authentication:OpenId.Settings");
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<TokenRefreshInfo>();

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, OidcTokenProvider>(OidcTokenProvider.ProviderName);
        container.Register<ITokenProvider, BootstrapContextTokenProvider>(BootstrapContextTokenProvider.ProviderName);
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);
        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        var tokenRefresh = scopedContainer.Resolve<TokenRefreshInfo>();
        tokenRefresh.RefreshToken = new TokenInfo("refresh_token", Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
        tokenRefresh.AccessToken = new TokenInfo("access_token", accessTokenCookies);

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType) { BootstrapContext = accessTokenOAuth2 }, "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, Constants.OAuth2OptionsName)
        {
            InnerHandler = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, Constants.OpenIdOptionsName)
            {
                InnerHandler = innerHandler.Object
            }
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(accessTokenOAuth2);
    }

    [Fact]
    // Scenario 8
    public async Task Jwt_With_OAuth2_And_OIDC_With_OpenId_As_The_Winner_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:OAuth2.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OAuth2.Settings:Scopes"] = "user.read user.write",
                             ["Authentication:OpenId.Settings:ClientId"] = "aa17786b-e33c-41ec-81cc-6063610aedeb",
                             ["Authentication:OpenId.Settings:ClientSecret"] = "This is a secret",
                             ["Authentication:OpenId.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OpenId.Settings:Scopes:0"] = "user.read",
                             ["Authentication:OpenId.Settings:Scopes:1"] = "user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwtOAuth2 = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessTokenOAuth2 = new JwtSecurityTokenHandler().WriteToken(jwtOAuth2);

        var jwtCookies = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessTokenCookies = new JwtSecurityTokenHandler().WriteToken(jwtCookies);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddDefaultAuthority(configuration);
        services.ConfigureOAuth2Settings(configuration, "Authentication:OAuth2.Settings");
        services.ConfigureOpenIdSettings(configuration, "Authentication:OpenId.Settings");
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<TokenRefreshInfo>();

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        var mockTokenRefresh = _fixture.Freeze<Mock<ITokenRefreshProvider>>();

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, OidcTokenProvider>(OidcTokenProvider.ProviderName);
        container.Register<ITokenProvider, BootstrapContextTokenProvider>(BootstrapContextTokenProvider.ProviderName);
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);
        container.RegisterInstance<ITokenRefreshProvider>(mockTokenRefresh.Object);
        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        var tokenRefresh = scopedContainer.Resolve<TokenRefreshInfo>();
        tokenRefresh.RefreshToken = new TokenInfo("refresh_token", Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
        tokenRefresh.AccessToken = new TokenInfo("access_token", accessTokenCookies);

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.CookiesAuthenticationType), "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, Constants.OAuth2OptionsName)
        {
            InnerHandler = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, Constants.OpenIdOptionsName)
            {
                InnerHandler = innerHandler.Object
            }
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(accessTokenCookies);
    }

    [Fact]
    // Scenario 9
    public async Task Jwt_With_OAuth2_And_OIDC_And_Inject_With_OAuth2_As_The_Winner_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var options = _fixture.Create<RemoteSecretSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:RemoteSecrets:Remote1:ClientSecret"] = options.ClientSecret,
                             ["Authentication:RemoteSecrets:Remote1:HeaderKey"] = "Basic",
                             ["Authentication:OAuth2.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OAuth2.Settings:Scopes"] = "user.read user.write",
                             ["Authentication:OpenId.Settings:ClientId"] = "aa17786b-e33c-41ec-81cc-6063610aedeb",
                             ["Authentication:OpenId.Settings:ClientSecret"] = "This is a secret",
                             ["Authentication:OpenId.Settings:Audiences:0"] = "urn://audience.com",
                             ["Authentication:OpenId.Settings:Scopes:0"] = "user.read",
                             ["Authentication:OpenId.Settings:Scopes:1"] = "user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwtOAuth2 = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessTokenOAuth2 = new JwtSecurityTokenHandler().WriteToken(jwtOAuth2);

        var jwtCookies = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessTokenCookies = new JwtSecurityTokenHandler().WriteToken(jwtCookies);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IScopedServiceProviderAccessor, ScopedServiceProviderAccessor>();
        services.AddDefaultAuthority(configuration);
        services.AddRemoteSecretsAuthentication(configuration);
        services.ConfigureOAuth2Settings(configuration, "Authentication:OAuth2.Settings");
        services.ConfigureOpenIdSettings(configuration, "Authentication:OpenId.Settings");
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<TokenRefreshInfo>();

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, OidcTokenProvider>(OidcTokenProvider.ProviderName);
        container.Register<ITokenProvider, BootstrapContextTokenProvider>(BootstrapContextTokenProvider.ProviderName);
        container.RegisterInstance<IHttpContextAccessor>(mockHttpContextAccessor.Object);
        container.Register<ITokenProvider, RemoteClientSecretTokenProvider>(RemoteClientSecretTokenProvider.ProviderName);

        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();
        var scopedServiceAccessor = container.Resolve<IScopedServiceProviderAccessor>();
        scopedServiceAccessor.ServiceProvider = scopedContainer.ServiceProvider;

        var tokenRefresh = scopedContainer.Resolve<TokenRefreshInfo>();
        tokenRefresh.RefreshToken = new TokenInfo("refresh_token", Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
        tokenRefresh.AccessToken = new TokenInfo("access_token", accessTokenCookies);

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType) { BootstrapContext = accessTokenOAuth2 }, "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        // Define the end handler that will simulate the call to the endpoint.
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        var sut = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, Constants.OAuth2OptionsName)
        {
            InnerHandler = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, Constants.OpenIdOptionsName)
            {
                InnerHandler = new JwtHandlerToTest(scopedServiceAccessor, scopedContainer.Resolve<ILogger<JwtHandlerToTest>>(), setingsOptions, "Remote1")
                {
                    InnerHandler = innerHandler.Object
                }
            }
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        // The request must have a Bearer token injected in the authohirzation header.
        httpRequestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization!.Parameter.Should().Be(accessTokenOAuth2);
    }
}
