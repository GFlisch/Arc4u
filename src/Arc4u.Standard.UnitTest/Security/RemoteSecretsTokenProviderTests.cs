using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]

public class RemoteSecretsTokenProviderTests
{
    public RemoteSecretsTokenProviderTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public async Task RemoteSecrets_TokenProvider_Should()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();

        var settings = new SimpleKeyValueSettings(new Dictionary<string, string>()
        {
            {TokenKeys.ProviderIdKey, RemoteClientSecretTokenProvider.ProviderName },
            {TokenKeys.ClientSecret,  options.ClientSecret},
            {TokenKeys.ClientSecretHeader, options.HeaderKey }
        });

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var token = await sut.GetTokenAsync(settings, null).ConfigureAwait(false);

        // assert
        token.Should().NotBeNull();
        token.Token.Should().Be(options.ClientSecret);
        token.TokenType.Should().Be(options.HeaderKey);
    }

    [Fact]
    public async Task RemoteSecrets_TokenProvider_With_No_Settings_Should()
    {
        // arrange

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(null, null).ConfigureAwait(false)).ConfigureAwait(false);

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task RemoteSecrets_TokenProvider_With_No_ClientSecret_Should()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();

        var settings = new SimpleKeyValueSettings(new Dictionary<string, string>()
        {
            {TokenKeys.ProviderIdKey, RemoteClientSecretTokenProvider.ProviderName },
            {TokenKeys.ClientSecretHeader, options.HeaderKey }
        });

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(settings, null).ConfigureAwait(false)).ConfigureAwait(false);

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public async Task RemoteSecrets_TokenProvider_With_No_HeaderKey_Should()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();

        var settings = new SimpleKeyValueSettings(new Dictionary<string, string>()
        {
            {TokenKeys.ProviderIdKey, RemoteClientSecretTokenProvider.ProviderName },
            {TokenKeys.ClientSecret,  options.ClientSecret},
        });

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(settings, null).ConfigureAwait(false)).ConfigureAwait(false);

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }
}
