using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Environment = Arc4u.Configuration.Environment;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class CredentialSecretTokenProviderTests
{
    private readonly Fixture _fixture;

    public CredentialSecretTokenProviderTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public async Task SecretBasicTokenProviderShouldAsync()
    {
        // arrange.
        var settings = GetUserPasswordSettings();
        var credential = new CredentialsResult(true, settings.Values["User"], settings.Values["Password"]);
        var tokenTest = new TokenInfo("TokenType", "AccessToken", DateTime.UtcNow.AddMinutes(60));

        var mockCredentialTokenProvider = _fixture.Freeze<Mock<ICredentialTokenProvider>>();
        mockCredentialTokenProvider
            .Setup(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()))
            .ReturnsAsync(tokenTest).Verifiable();
        var credentialProvider = mockCredentialTokenProvider.Object;

        var mockContainer = _fixture.Freeze<Mock<IKeyedServiceProvider>>();
        mockContainer.Setup(m => m.GetKeyedService(typeof(ICredentialTokenProvider), It.IsAny<string>()))
            .Returns(credentialProvider).Verifiable();

        _fixture.Inject<IServiceProvider>(mockContainer.Object);

        // act.
        var sut = _fixture.Create<CredentialSecretTokenProvider>();

        var tokenResult = await sut.GetTokenAsync(settings, null);

        // assert.
        tokenResult.Should().NotBeNull();
        tokenResult.IsSuccess.Should().BeTrue();
        var token = tokenResult.Value;
        token.Should().NotBeNull();
        token!.Token.Should().Be(tokenTest.Token);
        token.TokenType.Should().Be(tokenTest.TokenType);
        token.ExpiresOnUtc.Should().Be(tokenTest.ExpiresOnUtc);

        mockCredentialTokenProvider.Verify(
            m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()), Times.Once);
    }

    [Fact]
    public async Task SecretBasicTokenProviderWithPasswordAndCredentialShouldAsync()
    {
        // arrange.
        var settings = GetUserPasswordAndCredentialSettings();
        var tokenTest = new TokenInfo("TokenType", "AccessToken", DateTime.UtcNow.AddMinutes(60));

        var mockCredentialTokenProvider = _fixture.Freeze<Mock<ICredentialTokenProvider>>();
        mockCredentialTokenProvider
            .Setup(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()))
            .ReturnsAsync(tokenTest).Verifiable();

        var services = new ServiceCollection();
        services.AddKeyedSingleton<ICredentialTokenProvider>(CredentialTokenCacheTokenProvider.ProviderName,
            mockCredentialTokenProvider.Object);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddILogger();
        services.AddApplicationConfig(config =>
        {
            config.ApplicationName = _fixture.Create<string>();
            config.Environment = _fixture.Create<Environment>();
        });
        services.AddKeyedTransient<ITokenProvider, CredentialSecretTokenProvider>(CredentialSecretTokenProvider
            .ProviderName);

        var container = services.BuildServiceProvider();
        // act.
        var sut = container.GetRequiredKeyedService<ITokenProvider>(CredentialSecretTokenProvider.ProviderName);

        var result = await sut.GetTokenAsync(settings, null);

        // assert.
        result.IsFailed.Should().BeTrue();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].Message.Should().Be("User/Password or Credential must be filled in.");
        mockCredentialTokenProvider.Verify(
            m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()), Times.Never);
    }

    [Fact]
    public async Task SecretBasicTokenProviderWithNoSettingsShouldAsync()
    {
        // arrange.
        SimpleKeyValueSettings? settings = null;
        var credential = new CredentialsResult(false);
        var tokenTest = new TokenInfo("TokenType", "AccessToken", DateTime.UtcNow.AddMinutes(60));

        var mockCredentialTokenProvider = _fixture.Freeze<Mock<ICredentialTokenProvider>>();
        mockCredentialTokenProvider
            .Setup(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()))
            .ReturnsAsync(tokenTest).Verifiable();
        var credentialProvider = mockCredentialTokenProvider.Object;

        var mockContainer = _fixture.Freeze<Mock<IKeyedServiceProvider>>();
        mockContainer.Setup(m => m.GetKeyedService(typeof(ICredentialTokenProvider), It.IsAny<string>()))
            .Returns(credentialProvider).Verifiable();

        // act.
        var sut = _fixture.Create<CredentialSecretTokenProvider>();

        var exception =
            await Record.ExceptionAsync(async () => await sut.GetTokenAsync(settings, null).ConfigureAwait(false));

        // assert.
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentNullException>();
        mockCredentialTokenProvider.Verify(
            m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()), Times.Never);
    }

    [Fact]
    public void Read_Secret_From_Config_With_Credential_Should()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientSecrets:Service:ClientId"] = "ClientId",
                    ["Authentication:ClientSecrets:Service:Scopes:0"] = "A scope",
                    ["Authentication:ClientSecrets:Service:Credential"] = "user:passw0rd"
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddSecretAuthentication(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Service");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Scope].Should().Be("A scope");
        sut.Values.ContainsKey("User").Should().BeFalse();
        sut.Values.ContainsKey("Password").Should().BeFalse();
        sut.Values.ContainsKey("Credential").Should().BeTrue();
    }

    private SimpleKeyValueSettings GetUserPasswordSettings()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();

        return new SimpleKeyValueSettings(new Dictionary<string, string>
        {
            { TokenKeys.ClientIdKey, CredentialSecretTokenProvider.ProviderName },
            { TokenKeys.AuthorityKey, "Basic" },
            { TokenKeys.ProviderIdKey, options.ProviderId },
            { TokenKeys.Scope, string.Join(' ', options.Scopes) },
            { TokenKeys.AuthenticationTypeKey, options.AuthenticationType },
            { "Password", options.Password },
            { "User", options.User },
            { "BasicProviderId", CredentialTokenCacheTokenProvider.ProviderName }
        });
    }

    private SimpleKeyValueSettings GetUserPasswordAndCredentialSettings()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();

        return new SimpleKeyValueSettings(new Dictionary<string, string>
        {
            { TokenKeys.ClientIdKey, CredentialSecretTokenProvider.ProviderName },
            { TokenKeys.AuthorityKey, "Basic" },
            { TokenKeys.ProviderIdKey, options.ProviderId },
            { TokenKeys.Scope, string.Join(' ', options.Scopes) },
            { TokenKeys.AuthenticationTypeKey, options.AuthenticationType },
            { "Password", options.Password },
            { "Credential", $"{options.User}:{options.Credential}" },
            { "BasicProviderId", CredentialTokenCacheTokenProvider.ProviderName }
        });
    }
}
