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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Environment = Arc4u.Configuration.Environment;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class CredentialTokenCacheTokenProviderTests
{
    private readonly Fixture _fixture;

    public CredentialTokenCacheTokenProviderTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public async Task GetToken_With_No_Settings_Should_Throw()
    {
        // arrange
        var sut = _fixture.Create<CredentialTokenCacheTokenProvider>();

        // act
        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(null, null).ConfigureAwait(false));

        // assert
        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task GetToken_Builds_Credential_From_Settings_UserPassword_Should()
    {
        // arrange
        var user = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var settings = BuildSettings(user, password);

        CredentialsResult? captured = null;
        var (container, token) = BuildContainer(c => captured = c);

        var sut = ActivatorUtilities.CreateInstance<CredentialTokenCacheTokenProvider>(container);

        // act
        var result = await sut.GetTokenAsync(settings, null);

        // assert: the credential is built from the User/Password keys in the settings.
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be(token.Token);
        captured.Should().NotBeNull();
        captured!.CredentialsEntered.Should().BeTrue();
        captured.Upn.Should().Be(user);
        captured.Password.Should().Be(password);
    }

    [Fact]
    public async Task GetToken_Uses_Credential_From_PlatformParameters_Should()
    {
        // arrange: settings carry no User/Password, the credential comes from the platform parameter
        // (the path used by the Basic authentication middleware).
        var settings = BuildSettings(user: null, password: null);
        var credential = new CredentialsResult(true, _fixture.Create<string>(), _fixture.Create<string>());

        CredentialsResult? captured = null;
        var (container, token) = BuildContainer(c => captured = c);

        var sut = ActivatorUtilities.CreateInstance<CredentialTokenCacheTokenProvider>(container);

        // act
        var result = await sut.GetTokenAsync(settings, credential);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be(token.Token);
        captured.Should().BeSameAs(credential);
    }

    [Fact]
    public async Task GetToken_Without_Credential_Should_Fail()
    {
        // arrange: no User/Password in settings and no platform parameter.
        var settings = BuildSettings(user: null, password: null);
        var (container, _) = BuildContainer(_ => { });

        var sut = ActivatorUtilities.CreateInstance<CredentialTokenCacheTokenProvider>(container);

        // act
        var result = await sut.GetTokenAsync(settings, null);

        // assert
        result.IsFailed.Should().BeTrue();
    }

    private SimpleKeyValueSettings BuildSettings(string? user, string? password)
    {
        var values = new Dictionary<string, string>
        {
            { TokenKeys.ClientIdKey, _fixture.Create<string>() },
            { TokenKeys.Scope, _fixture.Create<string>() },
            { TokenKeys.AuthenticationTypeKey, _fixture.Create<string>() }
        };
        if (user is not null)
        {
            values.Add("User", user);
        }
        if (password is not null)
        {
            values.Add("Password", password);
        }
        return new SimpleKeyValueSettings(values);
    }

    private (IServiceProvider Container, TokenInfo Token) BuildContainer(Action<CredentialsResult> onCredential)
    {
        var token = new TokenInfo("Bearer", _fixture.Create<string>(), DateTime.UtcNow.AddMinutes(60));

        // Mock the inner CredentialDirect provider that performs the actual STS call.
        var mockDirect = new Mock<ICredentialTokenProvider>();
        mockDirect
            .Setup(m => m.GetTokenAsync(It.IsAny<IKeyValueSettings>(), It.IsAny<CredentialsResult>()))
            .Callback<IKeyValueSettings, CredentialsResult>((_, c) => onCredential(c))
            .ReturnsAsync(token);

        // Token cache always misses so the inner provider is invoked.
        var mockCache = new Mock<ITokenCache>();
        mockCache.Setup(m => m.Get<TokenInfo>(It.IsAny<string>())).Returns((TokenInfo?)null);

        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddILogger();
        services.AddApplicationConfig(config =>
        {
            config.ApplicationName = _fixture.Create<string>();
            config.Environment = _fixture.Create<Environment>();
        });
        services.AddDefaultAuthority(options => options.SetData(new Uri("https://login.microsoft.com"), null, null, null));
        services.AddKeyedSingleton<ICredentialTokenProvider>(CredentialTokenProvider.ProviderName, mockDirect.Object);
        services.AddSingleton<ITokenCache>(mockCache.Object);

        return (services.BuildServiceProvider(), token);
    }
}
