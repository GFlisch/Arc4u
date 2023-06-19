using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using Arc4u.Caching;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.ComponentModel;
using Arc4u.Diagnostics;
using Arc4u.gRPC.Interceptors;
using Arc4u.OAuth2;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using Arc4u.OAuth2.TokenProviders;
using Arc4u.Security.Principal;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Grpc.Core.Interceptors.Interceptor;
using Grpc.Core;
using System.Threading.Tasks;
using Arc4u.OAuth2.Options;

namespace Arc4u.UnitTest.Security;

/// <summary>
/// This test will control the different scenario defined for the usage of the GRpcinterceptor.
/// The handler can be used in the following case:
/// 1) Retrieve the bearer token when used with a user principal authenticated via an OpenId Connect scenario): AuthenticationType is OpenId.
/// 2) Retrieve the bearer token when used with a user principal authenticated in an Api call: AuthenticationType is OAuth2Bearer.
/// 3) By using a CLientSecret definition (username:password) and injecting the bearer token retrieve from a call to the authority provider: Authentication type is Inject.
/// 4) By injecting an encrypted username:password in the header of the http request : RemoteSecret token provider and AuthenticationType is Inject.
/// 5) By injecting a Basic authorization header during the http request: RemoteSecret, header key is Basic and AuthenticationType is Inject.
/// 6) Have an on behalf of scenario based on the scenario 1 or 2.
/// </summary>
///

public class InterceptorTest : OAuth2Interceptor
{
    public InterceptorTest(IContainerResolve containerResolve, ILogger<OAuth2Interceptor> logger, IOptionsMonitor<SimpleKeyValueSettings> keyValuesSettingsOption, string settingsName) : base(containerResolve, logger, keyValuesSettingsOption, settingsName)
    {
    }
}

public class RequestMessage
{
    public Guid Id { get; set; }
}

public class ResponseMessage
{
    public string Content { get; set; }
}

public class GRpcInterceptorTests
{
    public GRpcInterceptorTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    // Scenario 1
    public void Jwt_With_Principal_With_OIDC_Token_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var options = _fixture.Create<SecretBasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:OpenId.Settings:ClientId"] = "aa17786b-e33c-41ec-81cc-6063610aedeb",
                             ["Authentication:OpenId.Settings:ClientSecret"] = "This is a secret",
                             ["Authentication:OpenId.Settings:Audiences"] = "urn://audience.com",
                             ["Authentication:OpenId.Settings:Scopes"] = "user.read user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);
        services.ConfigureOpenIdSettings(configuration, "Authentication:OpenId.Settings");
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddScoped<TokenRefreshInfo>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var mockTokenRefresh = _fixture.Freeze<Mock<ITokenRefreshProvider>>();
        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, OidcTokenProvider>(OidcTokenProvider.ProviderName);
        container.RegisterInstance<ITokenRefreshProvider>(mockTokenRefresh.Object);
        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();

        var tokenRefresh = scopedContainer.Resolve<TokenRefreshInfo>();
        tokenRefresh.RefreshToken = new TokenInfo("refresh_token", Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
        tokenRefresh.AccessToken = new TokenInfo("access_token", accessToken);

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.CookiesAuthenticationType), "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        var mockMethod = _fixture.Freeze<Mock<Method<string, string>>>();

        var mockClientInterceptorContext = new ClientInterceptorContext<string, string>(mockMethod.Object, "host", new CallOptions(new Metadata()));
        var mock = _fixture.Freeze<Mock<BlockingUnaryCallContinuation<string, string>>>();

        // Act
        var sut = new InterceptorTest(scopedContainer, scopedContainer.Resolve<ILogger<InterceptorTest>>(), setingsOptions, Constants.OpenIdOptionsName);

        sut.BlockingUnaryCall<string, string>("Test", mockClientInterceptorContext, mock.Object);

        // Assert
        mockClientInterceptorContext.Options.Headers.Should().NotBeNull();
        mockClientInterceptorContext.Options.Headers!.GetValue("authorization").Should().Be($"Bearer {accessToken}");
    }

    [Fact]
    // Scenario 2
    public void Jwt_With_Principal_With_OAuth2_Token_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:OAuth2.Settings:Audiences"] = "urn://audience.com",
                             ["Authentication:OAuth2.Settings:Scopes"] = "user.read user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);
        services.ConfigureOAuth2Settings(configuration, "Authentication:OAuth2.Settings");
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, BootstrapContextTokenProvider>("Bootstrap");
        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType) { BootstrapContext = accessToken }, "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        var mockMethod = _fixture.Freeze<Mock<Method<string, string>>>();

        var mockClientInterceptorContext = new ClientInterceptorContext<string, string>(mockMethod.Object, "host", new CallOptions(new Metadata()));
        var mock = _fixture.Freeze<Mock<BlockingUnaryCallContinuation<string, string>>>();

        // Act
        var sut = new InterceptorTest(scopedContainer, scopedContainer.Resolve<ILogger<InterceptorTest>>(), setingsOptions, "OAuth2");

        sut.BlockingUnaryCall<string, string>("Test", mockClientInterceptorContext, mock.Object);

        // Assert
        mockClientInterceptorContext.Options.Headers.Should().NotBeNull();
        mockClientInterceptorContext.Options.Headers!.GetValue("authorization").Should().Be($"Bearer {accessToken}");
    }

    [Fact]
    // Scenario 3
    // When we inject. There is no need to have a principal!
    public void Jwt_With_ClientSecet_Should()
    {
        // arrange
        // arrange the configuration to setup the Client secret.
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:Scope"] = options.Scope,
                             ["Authentication:ClientSecrets:Client1:User"] = options.User,
                             ["Authentication:ClientSecrets:Client1:Credential"] = $"{options.User}:password",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

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

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.RegisterInstance<ICredentialTokenProvider>(mockSecretTokenProvider.Object, "CredentialDirect");
        container.Register<ITokenProvider, CredentialSecretTokenProvider>("ClientSecret");
        container.Register<ICredentialTokenProvider, CredentialTokenCacheTokenProvider>("Credential");
        container.RegisterInstance<ITokenCache>(mockTokenCache.Object);

        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        var mockMethod = _fixture.Freeze<Mock<Method<string, string>>>();

        var mockClientInterceptorContext = new ClientInterceptorContext<string, string>(mockMethod.Object, "host", new CallOptions(new Metadata()));
        var mock = _fixture.Freeze<Mock<BlockingUnaryCallContinuation<string, string>>>();

        // Act
        var sut = new InterceptorTest(scopedContainer, scopedContainer.Resolve<ILogger<InterceptorTest>>(), setingsOptions, "Client1");

        sut.BlockingUnaryCall<string, string>("Test", mockClientInterceptorContext, mock.Object);

        // Assert
        mockClientInterceptorContext.Options.Headers.Should().NotBeNull();
        mockClientInterceptorContext.Options.Headers!.GetValue("authorization").Should().Be($"Bearer {accessToken}");
    }

    [Fact]
    // Scenario 4
    // When we inject. There is no need to have a principal!
    public void Jwt_With_RemoteSecreInjected_With_Basic_Authorization_Should()
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

        services.AddRemoteSecretsAuthentication(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, RemoteClientSecretTokenProvider>(RemoteClientSecretTokenProvider.ProviderName);

        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType), "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        var mockMethod = _fixture.Freeze<Mock<Method<string, string>>>();

        var mockClientInterceptorContext = new ClientInterceptorContext<string, string>(mockMethod.Object, "host", new CallOptions(new Metadata()));
        var mock = _fixture.Freeze<Mock<BlockingUnaryCallContinuation<string, string>>>();

        // Act
        var sut = new InterceptorTest(scopedContainer, scopedContainer.Resolve<ILogger<InterceptorTest>>(), setingsOptions, "Remote1");

        sut.BlockingUnaryCall<string, string>("Test", mockClientInterceptorContext, mock.Object);

        // Assert
        mockClientInterceptorContext.Options.Headers.Should().NotBeNull();
        mockClientInterceptorContext.Options.Headers!.GetValue("authorization").Should().Be($"Basic {options.ClientSecret}");
    }

    [Fact]
    // Scenario 5
    // When we inject. There is no need to have a principal!
    public void Jwt_With_RemoteSecreInjected_Should()
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

        services.AddRemoteSecretsAuthentication(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, RemoteClientSecretTokenProvider>(RemoteClientSecretTokenProvider.ProviderName);

        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType), "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        var mockMethod = _fixture.Freeze<Mock<Method<string, string>>>();

        var mockClientInterceptorContext = new ClientInterceptorContext<string, string>(mockMethod.Object, "host", new CallOptions(new Metadata()));
        var mock = _fixture.Freeze<Mock<BlockingUnaryCallContinuation<string, string>>>();

        // Act
        var sut = new InterceptorTest(scopedContainer, scopedContainer.Resolve<ILogger<InterceptorTest>>(), setingsOptions, "Remote1");

        sut.BlockingUnaryCall<string, string>("Test", mockClientInterceptorContext, mock.Object);

        // Assert
        mockClientInterceptorContext.Options.Headers.Should().NotBeNull();
        mockClientInterceptorContext.Options.Headers!.GetValue(options.HeaderKey).Should().Be(options.ClientSecret);
    }

    [Fact]
    // Scenario 6
    public void Jwt_With_OAuth2_And_Principal_With_Bearer_Token_And_On_Behalf_of_Should()
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
                             ["Authentication:OnBehalfOf:Obo:Scope"] = "user.read user.write",
                             ["Authentication:DefaultAuthority:Url"] = "https://login.microsoft.com"
                         }).Build();

        // Define an access token that will be used as the return of the call to the CredentialDirect token credential provider.
        var jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        // Register the different services.
        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);
        services.AddOnBehalfOf(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();
        services.AddScoped<TokenRefreshInfo>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));


        var mockActivitySourceFactory = new Mock<IActivitySourceFactory>();
        mockActivitySourceFactory.Setup(m => m.Get("Arc4u", null)).Returns((ActivitySource)null);
        services.AddSingleton<IActivitySourceFactory>(mockActivitySourceFactory.Object);

        // Used the cache to return the access token in the Obo provider => avoid any call to the Authority!
        var mockCache = _fixture.Freeze<Mock<ICache>>();
        mockCache.Setup(m => m.GetAsync<TokenInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new TokenInfo("Bearer", accessToken));

        var mockCacheHelper = _fixture.Freeze<Mock<ICacheHelper>>();
        mockCacheHelper.Setup(m => m.GetCache()).Returns(mockCache.Object);

        services.AddSingleton<ICacheHelper>(mockCacheHelper.Object);

        // Register the different TokenProvider and CredentialTokenProviders.
        var container = new ComponentModelContainer(services);
        container.Register<ITokenProvider, AzureADOboTokenProvider>(AzureADOboTokenProvider.ProviderName);
        container.CreateContainer();

        // Create a scope to be in the context majority of the time a business code is.
        using var scopedContainer = container.CreateScope();

        // Define a Principal with no OAuth2Bearer token here => we test the injection.
        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType) { BootstrapContext = accessToken }, "S-1-0-0"));

        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        var mockMethod = _fixture.Freeze<Mock<Method<string, string>>>();

        var mockClientInterceptorContext = new ClientInterceptorContext<string, string>(mockMethod.Object, "host", new CallOptions(new Metadata()));
        var mock = _fixture.Freeze<Mock<BlockingUnaryCallContinuation<string, string>>>();

        // Act
        var sut = new InterceptorTest(scopedContainer, scopedContainer.Resolve<ILogger<InterceptorTest>>(), setingsOptions, "Obo");

        sut.BlockingUnaryCall<string, string>("Test", mockClientInterceptorContext, mock.Object);

        // Assert
        mockClientInterceptorContext.Options.Headers.Should().NotBeNull();
        mockClientInterceptorContext.Options.Headers!.GetValue("authorization").Should().Be($"Bearer {accessToken}");
    }
}
