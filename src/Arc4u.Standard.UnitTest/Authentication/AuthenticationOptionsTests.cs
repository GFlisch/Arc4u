using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.DependencyInjection;
using Arc4u.OAuth2.Extensions;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;

namespace Arc4u.Standard.UnitTest.Authentication;

[Trait("Category", "CI")]
public class AuthenticationOptionsTests
{
    public AuthenticationOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void TestOauth2KeyValuesShould()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["OAuth2.Settings:ClientId"] = options.ClientId,
                             ["OAuth2.Settings:Audiences"] = options.Audiences,
                             ["OAuth2.Settings:Authority"] = options.Authority,
                             ["OAuth2.Settings:Scopes"] = options.Scopes,
                          }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("OAuth2");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.Values[TokenKeys.Audiences].Should().Be(options.Audiences);
        sut.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.Values[TokenKeys.Scopes].Should().Be(options.Scopes);
    }

    [Fact]
    public void TestOauth2WithNoScopesKeyValuesShould()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["OAuth2.Settings:ClientId"] = options.ClientId,
                             ["OAuth2.Settings:Audiences"] = options.Audiences,
                             ["OAuth2.Settings:Authority"] = options.Authority,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("OAuth2");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.Values[TokenKeys.Audiences].Should().Be(options.Audiences);
        sut.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.Values.Should().NotContainKey(TokenKeys.Scopes);
    }
}
