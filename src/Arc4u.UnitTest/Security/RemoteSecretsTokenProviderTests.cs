using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class RemoteSecretsTokenProviderTests
{
    private readonly Fixture _fixture;

    public RemoteSecretsTokenProviderTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public async Task RemoteSecrets_TokenProvider_Should()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();

        var settings = new SimpleKeyValueSettings(new Dictionary<string, string>
        {
            { TokenKeys.ProviderIdKey, RemoteClientSecretTokenProvider.ProviderName },
            { TokenKeys.ClientSecret, options.ClientSecret },
            { TokenKeys.ClientSecretHeader, options.HeaderKey }
        });

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var tokenResult = await sut.GetTokenAsync(settings, null);

        // assert
        tokenResult.Should().NotBeNull();
        tokenResult.IsSuccess.Should().BeTrue();
        var token = tokenResult.Value;
        token.Should().NotBeNull();
        token!.Token.Should().Be(options.ClientSecret);
        token.TokenType.Should().Be(options.HeaderKey);
    }

    [Fact]
    public async Task RemoteSecrets_TokenProvider_With_No_Settings_Should()
    {
        // arrange

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var exception =
            await Record.ExceptionAsync(async () => await sut.GetTokenAsync(null, null).ConfigureAwait(false));

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task RemoteSecrets_TokenProvider_With_No_ClientSecret_Should()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();

        var settings = new SimpleKeyValueSettings(new Dictionary<string, string>
        {
            { TokenKeys.ProviderIdKey, RemoteClientSecretTokenProvider.ProviderName },
            { TokenKeys.ClientSecretHeader, options.HeaderKey }
        });

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var exception =
            await Record.ExceptionAsync(async () => await sut.GetTokenAsync(settings, null).ConfigureAwait(false));

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public async Task RemoteSecrets_TokenProvider_With_No_HeaderKey_Should()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();

        var settings = new SimpleKeyValueSettings(new Dictionary<string, string>
        {
            { TokenKeys.ProviderIdKey, RemoteClientSecretTokenProvider.ProviderName },
            { TokenKeys.ClientSecret, options.ClientSecret }
        });

        // act
        var sut = _fixture.Create<RemoteClientSecretTokenProvider>();
        var result = await sut.GetTokenAsync(settings, null);

        // assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Client secret Header is missing. Cannot process the request.");
    }
}
