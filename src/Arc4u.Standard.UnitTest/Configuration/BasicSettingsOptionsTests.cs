using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Arc4u.Standard.OAuth2;
using FluentAssertions;
using Arc4u.OAuth2.Token;
using Arc4u.Configuration;
using Arc4u.OAuth2.Extensions;
using Arc4u.Standard.OAuth2.Middleware;
using Arc4u.Standard.OAuth2.Options;
using System.Linq;

namespace Arc4u.Standard.UnitTest;

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
    public void BasicStandardShould()
    {
        var options = _fixture.Create<BasicSettingsOptions>();
        var _default = new BasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication.Basic:Settings:ClientId"] = options.ClientId,
                             ["Authentication.Basic:Settings:Audience"] = options.Audience,
                             ["Authentication.Basic:Settings:Authority"] = options.Authority,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptions<BasicAuthenticationSettingsOptions>>()!.Value;

        sut.Should().NotBeNull();
        sut.BasicSettings.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.BasicSettings.Values[TokenKeys.Audience].Should().Be(options.Audience);
        sut.BasicSettings.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.BasicSettings.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.BasicSettings.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.BasicSettings.Values[TokenKeys.Scope].Should().Be(_default.Scope);
    }

    [Fact]
    public void CustomStandardShould()
    {
        var options = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication.Basic:Settings:ClientId"] = options.ClientId,
                             ["Authentication.Basic:Settings:Audience"] = options.Audience,
                             ["Authentication.Basic:Settings:Authority"] = options.Authority,
                             ["Authentication.Basic:Settings:ProviderId"] = options.ProviderId,
                             ["Authentication.Basic:Settings:AuthenticationType"] = options.AuthenticationType,
                             ["Authentication.Basic:Settings:Scope"] = options.Scope,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptions<BasicAuthenticationSettingsOptions>>()!.Value;

        sut.Should().NotBeNull();
        sut.BasicSettings.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.BasicSettings.Values[TokenKeys.Audience].Should().Be(options.Audience);
        sut.BasicSettings.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.BasicSettings.Values[TokenKeys.ProviderIdKey].Should().Be(options.ProviderId);
        sut.BasicSettings.Values[TokenKeys.AuthenticationTypeKey].Should().Be(options.AuthenticationType);
        sut.BasicSettings.Values[TokenKeys.Scope].Should().Be(options.Scope);
    }

    [Fact]
    public void BasicStandardIncompleteShouldThrowAnException()
    {
        var options = _fixture.Create<BasicSettingsOptions>();
        var _default = new BasicSettingsOptions();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication.Basic:Settings:Audience"] = options.Audience,
                             ["Authentication.Basic:Settings:Authority"] = options.Authority,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddBasicAuthenticationSettings(configuration));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void BasicSettingsDefaultUpnShould()
    {
        var options = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication.Basic:Settings:ClientId"] = options.ClientId,
                             ["Authentication.Basic:Settings:Audience"] = options.Audience,
                             ["Authentication.Basic:Settings:Authority"] = options.Authority,
                             ["Authentication.Basic:Settings:ProviderId"] = options.ProviderId,
                             ["Authentication.Basic:Settings:AuthenticationType"] = options.AuthenticationType,
                             ["Authentication.Basic:Settings:Scope"] = options.Scope,
                             ["Authentication.Basic:DefaultUpn"] = "@arc4u.net",
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptions<BasicAuthenticationSettingsOptions>>()!.Value;

        sut.Should().NotBeNull();
        sut.DefaultUpn.Should().Be("@arc4u.net");
    }

    /*
     *                     ["Authentication:DataProtection:EncryptionCertificate:File:Cert"] = @".\Configs\cert.pem",
                    ["Authentication:DataProtection:EncryptionCertificate:File:Key"] = @".\Configs\key.pem",

     */

    [Fact]
    public void BasicCertificateShould()
    {
        var options = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Authentication.Basic:Settings:ClientId"] = options.ClientId,
                             ["Authentication.Basic:Settings:Audience"] = options.Audience,
                             ["Authentication.Basic:Settings:Authority"] = options.Authority,
                             ["Authentication.Basic:Settings:ProviderId"] = options.ProviderId,
                             ["Authentication.Basic:Settings:AuthenticationType"] = options.AuthenticationType,
                             ["Authentication.Basic:Settings:Scope"] = options.Scope,
                             ["Authentication.Basic:Certificates:Cert1:File:Cert"] = @".\Configs\cert.pem",
                             ["Authentication.Basic:Certificates:Cert1:File:Key"] = @".\Configs\key.pem",
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
