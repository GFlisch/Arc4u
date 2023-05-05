using System.Collections.Generic;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using Arc4u.Standard.OAuth2;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Xunit;
using System.Threading.Tasks;
using FluentAssertions;
using System;

namespace Arc4u.Standard.UnitTest.Security;

[Trait("Category", "CI")]

public class CredentialSecretTokenProviderTests
{
    public CredentialSecretTokenProviderTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public async Task SecretBasicTokenProviderShouldAsync()
    {
        // arrange.
        var settings = GetUserPasswordSettings();
        var credential = new CredentialsResult(true, settings.Values["User"], settings.Values["Password"]);
        var tokenTest = new TokenInfo("TokenType", "AccessToken", DateTime.UtcNow.AddMinutes(60));

        var mockCredentialTokenProvider = _fixture.Freeze<Mock<ICredentialTokenProvider>>();
        mockCredentialTokenProvider.Setup(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>())).ReturnsAsync(tokenTest).Verifiable();
        ICredentialTokenProvider credentialProvider = mockCredentialTokenProvider.Object;

        var mockContainer = _fixture.Freeze<Mock<IContainerResolve>>();
        mockContainer.Setup(m => m.TryResolve<ICredentialTokenProvider>(CredentialTokenCacheTokenProvider.ProviderName, out credentialProvider)).Returns(true).Verifiable();

        // act.
        var sut = _fixture.Create<CredentialSecretTokenProvider>();

        var token = await sut.GetTokenAsync(settings, null).ConfigureAwait(false);

        // assert.
        token.Should().NotBeNull();
        token.Token.Should().Be(tokenTest.Token);
        token.TokenType.Should().Be(tokenTest.TokenType);
        token.ExpiresOnUtc.Should().Be(tokenTest.ExpiresOnUtc);

        mockCredentialTokenProvider.Verify(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()), Times.Once);

    }

    [Fact]
    public async Task SecretBasicTokenProviderWithPasswordAndCredentialShouldAsync()
    {
        // arrange.
        var settings = GetUserPasswordAndCredentialSettings();
        var tokenTest = new TokenInfo("TokenType", "AccessToken", DateTime.UtcNow.AddMinutes(60));

        var mockCredentialTokenProvider = _fixture.Freeze<Mock<ICredentialTokenProvider>>();
        mockCredentialTokenProvider.Setup(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>())).ReturnsAsync(tokenTest).Verifiable();
        ICredentialTokenProvider credentialProvider = mockCredentialTokenProvider.Object;

        var mockContainer = _fixture.Freeze<Mock<IContainerResolve>>();
        mockContainer.Setup(m => m.TryResolve<ICredentialTokenProvider>(CredentialTokenCacheTokenProvider.ProviderName, out credentialProvider)).Returns(true).Verifiable();

        // act.
        var sut = _fixture.Create<CredentialSecretTokenProvider>();


        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(settings, null).ConfigureAwait(false)).ConfigureAwait(false);

        // assert.
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
        mockCredentialTokenProvider.Verify(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()), Times.Never);

    }

    [Fact]
    public async Task SecretBasicTokenProviderWithNoSettingsShouldAsync()
    {
        // arrange.
        SimpleKeyValueSettings settings = null;
        var credential = new CredentialsResult(false);
        var tokenTest = new TokenInfo("TokenType", "AccessToken", DateTime.UtcNow.AddMinutes(60));

        var mockCredentialTokenProvider = _fixture.Freeze<Mock<ICredentialTokenProvider>>();
        mockCredentialTokenProvider.Setup(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>())).ReturnsAsync(tokenTest).Verifiable();
        ICredentialTokenProvider credentialProvider = mockCredentialTokenProvider.Object;

        var mockContainer = _fixture.Freeze<Mock<IContainerResolve>>();
        mockContainer.Setup(m => m.TryResolve<ICredentialTokenProvider>(CredentialTokenCacheTokenProvider.ProviderName, out credentialProvider)).Returns(true).Verifiable();

        // act.
        var sut = _fixture.Create<CredentialSecretTokenProvider>();

        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(settings, null).ConfigureAwait(false)).ConfigureAwait(false);

        // assert.
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentNullException>();
        mockCredentialTokenProvider.Verify(m => m.GetTokenAsync(It.IsAny<SimpleKeyValueSettings>(), It.IsAny<CredentialsResult>()), Times.Never);

    }

    private SimpleKeyValueSettings GetUserPasswordSettings()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();

        return new SimpleKeyValueSettings(new Dictionary<string, string>
        {
            { TokenKeys.ClientIdKey, CredentialSecretTokenProvider.ProviderName },
            { TokenKeys.Audience, options.Audience },
            { TokenKeys.AuthorityKey, options.Authority },
            { TokenKeys.ProviderIdKey, options.ProviderId },
            { TokenKeys.Scope, options.Scope },
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
            { TokenKeys.Audience, options.Audience },
            { TokenKeys.AuthorityKey, options.Authority },
            { TokenKeys.ProviderIdKey, options.ProviderId },
            { TokenKeys.Scope, options.Scope },
            { TokenKeys.AuthenticationTypeKey, options.AuthenticationType },
            { "Password", options.Password },
            { "Credential", $"{options.User}:{options.Credential}" },
            { "BasicProviderId", CredentialTokenCacheTokenProvider.ProviderName }
        });
    }

}
