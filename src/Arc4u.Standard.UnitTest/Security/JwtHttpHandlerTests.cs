using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Arc4u.Dependency;
using Arc4u.OAuth2.Token;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Arc4u.Configuration;
using Arc4u.OAuth2;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Arc4u.OAuth2.Extensions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Moq.Protected;
using FluentAssertions;
using Arc4u.Security.Principal;
using Arc4u.Dependency.ComponentModel;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Arc4u.UnitTest.Security;

public class JwtHandlerToTest : JwtHttpHandler
{
    public JwtHandlerToTest(IContainerResolve containerResolve, ILogger<JwtHandlerToTest> logger, IOptionsMonitor<SimpleKeyValueSettings> keyValuesSettingsOption, string resolvingName) : base(containerResolve, logger, keyValuesSettingsOption, resolvingName)
    {
            
    }
}

public class JwtHttpHandlerTests
{
    public JwtHttpHandlerTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public async Task Jwt_With_RemoteSecrets_Should()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:User"] = options.User,
                             ["Authentication:ClientSecrets:Client1:Credential"] = options.Credential,
                         }).Build();

        JwtSecurityToken jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddSecretAuthentication(configuration);
        services.AddScoped<IApplicationContext, ApplicationInstanceContext>();

        var mockSecretTokenProvider = _fixture.Freeze<Mock<ITokenProvider>>();
        mockSecretTokenProvider.Setup(m => m.GetTokenAsync(It.IsAny<IKeyValueSettings>(), It.IsAny<object>()))
                               .ReturnsAsync(new TokenInfo("Bearer", accessToken));

        var container = new ComponentModelContainer(services);
        container.RegisterInstance<ITokenProvider>(mockSecretTokenProvider.Object, "ClientSecret");
        container.CreateContainer();

        using var scopedContainer = container.CreateScope();

        var appContext = scopedContainer.Resolve<IApplicationContext>();
        appContext.SetPrincipal(new AppPrincipal(new Arc4u.Security.Principal.Authorization(), new ClaimsIdentity(Constants.BearerAuthenticationType), "S-1-0-0"));
        var setingsOptions = scopedContainer.Resolve<IOptionsMonitor<SimpleKeyValueSettings>>();

        var mockLogging = _fixture.Freeze<Mock<ILogger<JwtHandlerToTest>>>();

        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        var sut = new JwtHandlerToTest(scopedContainer, mockLogging.Object, setingsOptions, "Client1")
        {
            InnerHandler = innerHandler.Object
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var invoker = new HttpMessageInvoker(sut);
        var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken()).ConfigureAwait(false);
        
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        httpRequestMessage.Headers.Authorization.Parameter.Should().Be(accessToken);
    }
}
