using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Arc4u.Standard.OAuth2;
using Arc4u.Standard.OAuth2.Extensions;
using FluentAssertions;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;

namespace Arc4u.Standard.UnitTest;

[Trait("Category", "CI")]
public class SecretBasicSettingsOptionsTests
{
    public SecretBasicSettingsOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void SecretUserPasswordBasicShould()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:Audience"] = options.Audience,
                             ["Authentication:ClientSecrets:Client1:Authority"] = options.Authority,
                             ["Authentication:ClientSecrets:Client1:User"] = options.User,
                             ["Authentication:ClientSecrets:Client1:Password"] = options.Password,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddSecretAuthentication(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Client1");

        sut.Should().NotBeNull();
        
        sut.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.Values[TokenKeys.Audience].Should().Be(options.Audience);
        sut.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.Values[TokenKeys.Scope].Should().Be(_default.Scope);
        sut.Values["User"].Should().Be(options.User);
        sut.Values["Password"].Should().Be(options.Password);
        sut.Values["Credential"].Should().BeNull();
    }

    [Fact]
    public void SecretCredentialBasicShould()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:Audience"] = options.Audience,
                             ["Authentication:ClientSecrets:Client1:Authority"] = options.Authority,
                             ["Authentication:ClientSecrets:Client1:User"] = options.User,
                             ["Authentication:ClientSecrets:Client1:Credential"] = options.Credential,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddSecretAuthentication(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Client1");

        sut.Should().NotBeNull();

        sut.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.Values[TokenKeys.Audience].Should().Be(options.Audience);
        sut.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.Values[TokenKeys.Scope].Should().Be(_default.Scope);
        sut.Values["User"].Should().Be(options.User);
        sut.Values["Credential"].Should().Be(options.Credential);
        sut.Values["Password"].Should().BeNull();
    }

    [Fact]
    public void SecretNotFilledExceptionShould()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:Audience"] = options.Audience,
                             ["Authentication:ClientSecrets:Client1:Authority"] = options.Authority,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddSecretAuthentication(configuration));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void SecretPasswordAndCredentialExceptionShould()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:Audience"] = options.Audience,
                             ["Authentication:ClientSecrets:Client1:Authority"] = options.Authority,
                             ["Authentication:ClientSecrets:Client1:Password"] = options.Password,
                             ["Authentication:ClientSecrets:Client1:Credential"] = options.Credential,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddSecretAuthentication(configuration));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }
}
