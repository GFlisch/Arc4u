using Arc4u.Configuration;
using Arc4u.OAuth2;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest;

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
    public void Secret_User_Password_Basic_Should()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();
        var authUrl = _fixture.Create<Uri>().ToString();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:Authority:Url"] = authUrl,
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
        sut.Values[TokenKeys.AuthorityKey].Should().Be("Client1");
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.Values[TokenKeys.Scope].Should().Be("openid");
        sut.Values["User"].Should().Be(options.User);
        sut.Values["Password"].Should().Be(options.Password);
        sut.Values.ContainsKey("Credential").Should().BeFalse();

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get("Client1");
        sutAuthority.Should().NotBeNull();
        sutAuthority.Url.Should().Be(authUrl);
    }

    [Fact]
    public void Secret_Credential_Basic_Should()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();
        var authUrl = _fixture.Create<Uri>().ToString();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:Authority:Url"] = authUrl,
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
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.Values[TokenKeys.Scope].Should().Be("openid");
        sut.Values[TokenKeys.AuthorityKey].Should().Be("Client1");
        sut.Values["User"].Should().Be(options.User);
        sut.Values["Credential"].Should().Be(options.Credential);
        sut.Values.ContainsKey("Password").Should().BeFalse();

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get("Client1");
        sutAuthority.Should().NotBeNull();
        sutAuthority.Url.Should().Be(authUrl);
    }

    [Fact]
    public void Secret_Credential_Basic_With_No_Authority_Should()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();
        var authUrl = _fixture.Create<Uri>().ToString();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                             ["Authentication:ClientSecrets:Client1:User"] = options.User,
                             ["Authentication:ClientSecrets:Client1:Credential"] = options.Credential,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddSecretAuthentication(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()?.Get("Client1");

        sut.Should().NotBeNull();

        sut.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.Values[TokenKeys.Scope].Should().Be("openid");
        sut.Values.ContainsKey(TokenKeys.AuthorityKey).Should().BeFalse();
        sut.Values["User"].Should().Be(options.User);
        sut.Values["Credential"].Should().Be(options.Credential);
        sut.Values.ContainsKey("Password").Should().BeFalse();

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get("Client1");
        sutAuthority.Should().NotBeNull();
        sutAuthority.Url.Should().BeNull();
    }

    [Fact]
    public void No_Secret_Should()
    {
        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddSecretAuthentication(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>();

        sut.Should().BeNull();
    }

    [Fact]
    public void Secret_Not_Filled_Exception_Should()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddSecretAuthentication(configuration));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void Secret_Password_And_Credential_Exception_Should()
    {
        var options = _fixture.Create<SecretBasicSettingsOptions>();
        var _default = new SecretBasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:ClientSecrets:Client1:ClientId"] = options.ClientId,
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
