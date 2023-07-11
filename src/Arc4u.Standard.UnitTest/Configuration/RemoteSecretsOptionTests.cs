using System.Collections.Generic;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Extensions;
using Arc4u.Configuration;
using FluentAssertions;
using Arc4u.OAuth2.Token;

namespace Arc4u.UnitTest;

[Trait("Category", "CI")]

public class RemoteSecretsOptionTests
{
    public RemoteSecretsOptionTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void RemoteSecretsShould()
    {
        var options = _fixture.Create<RemoteSecretSettingsOptions>();
        var _default = new RemoteSecretSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:RemoteSecrets:Remote1:ClientSecret"] = options.ClientSecret,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddRemoteSecretsAuthentication(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Remote1");

        sut.Should().NotBeNull();

        sut.Values[TokenKeys.ClientSecretHeader].Should().Be(_default.HeaderKey);
        sut.Values[TokenKeys.ClientSecret].Should().Be(options.ClientSecret);
    }

    [Fact]
    public void RemoteSecretsWithNoSecretShould()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();
        var _default = new RemoteSecretSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:RemoteSecrets:Remote1:HeaderKey"] = options.HeaderKey,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        // act
        var exception = Record.Exception(() => services.AddRemoteSecretsAuthentication(configuration));

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void RemoteSecretsWithNoHeaderKeyShould()
    {
        // arrange
        var options = _fixture.Create<RemoteSecretSettingsOptions>();
        var _default = new RemoteSecretSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:RemoteSecrets:Remote1:HeaderKey"] = null,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        // act
        var exception = Record.Exception(() => services.AddRemoteSecretsAuthentication(configuration));

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }
}
