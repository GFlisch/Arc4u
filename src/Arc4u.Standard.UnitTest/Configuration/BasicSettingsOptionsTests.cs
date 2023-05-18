using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Arc4u.OAuth2;
using FluentAssertions;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Middleware;
using Arc4u.OAuth2.Options;
using System.Linq;

namespace Arc4u.UnitTest;

[Trait("Category", "CI")]
public class BasicSettingsOptionsTests
{
    public BasicSettingsOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void Basic_Standard_Should()
    {
        var options = _fixture.Create<BasicSettingsOptions>();
        var _default = new BasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:Basic:Settings:ClientId"] = options.ClientId,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptions<BasicAuthenticationSettingsOptions>>()!.Value;

        sut.Should().NotBeNull();
        sut.BasicSettings.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.BasicSettings.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.BasicSettings.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.BasicSettings.Values[TokenKeys.Scope].Should().Be(_default.Scope);
        sut.BasicSettings.Values.ContainsKey(TokenKeys.ClientSecret).Should().BeFalse();
    }

    [Fact]
    public void Custom_Standard_Should()
    {
        var options = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:Basic:Settings:ClientId"] = options.ClientId,
                             ["Authentication:Basic:Settings:ProviderId"] = options.ProviderId,
                             ["Authentication:Basic:Settings:AuthenticationType"] = options.AuthenticationType,
                             ["Authentication:Basic:Settings:Scope"] = options.Scope,
                             ["Authentication:Basic:Settings:ClientSecret"] = options.ClientSecret,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptions<BasicAuthenticationSettingsOptions>>()!.Value;

        sut.Should().NotBeNull();
        sut.BasicSettings.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.BasicSettings.Values[TokenKeys.ProviderIdKey].Should().Be(options.ProviderId);
        sut.BasicSettings.Values[TokenKeys.AuthenticationTypeKey].Should().Be(options.AuthenticationType);
        sut.BasicSettings.Values[TokenKeys.Scope].Should().Be(options.Scope);
        sut.BasicSettings.Values[TokenKeys.ClientSecret].Should().Be(options.ClientSecret);
    }

    [Fact]
    public void Basic_Settings_Default_Upn_Should()
    {
        var options = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:Basic:Settings:ClientId"] = options.ClientId,
                             ["Authentication:Basic:Settings:ProviderId"] = options.ProviderId,
                             ["Authentication:Basic:Settings:AuthenticationType"] = options.AuthenticationType,
                             ["Authentication:Basic:Settings:Scope"] = options.Scope,
                             ["Authentication:Basic:DefaultUpn"] = "@arc4u.net",
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptions<BasicAuthenticationSettingsOptions>>()!.Value;

        sut.Should().NotBeNull();
        sut.DefaultUpn.Should().Be("@arc4u.net");
        sut.BasicSettings.Values.ContainsKey(TokenKeys.ClientSecret).Should().BeFalse();
    }

      [Fact]
    public void BasicCertificateShould()
    {
        var options = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication:Basic:Settings:ClientId"] = options.ClientId,
                             ["Authentication:Basic:Settings:ProviderId"] = options.ProviderId,
                             ["Authentication:Basic:Settings:AuthenticationType"] = options.AuthenticationType,
                             ["Authentication:Basic:Settings:Scope"] = options.Scope,
                             ["Authentication:Basic:Certificates:Cert1:File:Cert"] = @".\Configs\cert.pem",
                             ["Authentication:Basic:Certificates:Cert1:File:Key"] = @".\Configs\key.pem",
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptions<BasicAuthenticationSettingsOptions>>()!.Value;

        sut.Should().NotBeNull();
        sut.CertificateHeaderOptions.Should().NotBeNull();
        sut.CertificateHeaderOptions.Any().Should().BeTrue();
        sut.CertificateHeaderOptions.First().Key.Should().Be("Cert1");
    }
}
