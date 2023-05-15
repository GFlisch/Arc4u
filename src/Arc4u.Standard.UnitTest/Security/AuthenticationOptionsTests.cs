using System.Collections.Generic;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.DependencyInjection;
using Arc4u.OAuth2.Extensions;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;
using System;
using Arc4u.Standard.OAuth2;

namespace Arc4u.Standard.UnitTest.Security;

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
    public void Oauth2_Key_Values_With_Authority_Should()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();
        var authority = _fixture.Build<AuthorityOptions>().With(p => p.Url, _fixture.Create<Uri>().ToString()).Create();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["OAuth2.Settings:Audiences"] = options.Audiences,
                             ["OAuth2.Settings:Authority:Url"] = authority.Url,
                             ["OAuth2.Settings:Scopes"] = options.Scopes,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get(Constants.OAuth2OptionsName);

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(options.Audiences);
        sut.Values[TokenKeys.AuthorityKey].Should().Be(Constants.OAuth2OptionsName);
        sut.Values[TokenKeys.Scopes].Should().Be(options.Scopes);

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get(Constants.OAuth2OptionsName);

        sutAuthority.Url.Should().NotBeNullOrWhiteSpace();
        sutAuthority.Url.Should().Be(authority.Url);

    }

    [Fact]
    public void Oauth2_Key_Values_Without_Authority_Should()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["OAuth2.Settings:Audiences"] = options.Audiences,
                             ["OAuth2.Settings:Scopes"] = options.Scopes,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get(Constants.OAuth2OptionsName);

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(options.Audiences);
        sut.Values.ContainsKey(TokenKeys.AuthorityKey).Should().BeFalse();
        sut.Values[TokenKeys.Scopes].Should().Be(options.Scopes);

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get(Constants.OAuth2OptionsName);

        sutAuthority.Url.Should().BeNullOrWhiteSpace();


    }

    [Fact]
    public void Test_Oauth2_With_No_Scopes_Key_Values_Should()
    {
        var options = _fixture.Create<OAuth2SettingsOption>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["OAuth2.Settings:Audiences"] = options.Audiences,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.ConfigureOAuth2Settings(configuration, "OAuth2.Settings");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("OAuth2");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.Audiences].Should().Be(options.Audiences);
        sut.Values.Should().NotContainKey(TokenKeys.Scopes);
    }
}
