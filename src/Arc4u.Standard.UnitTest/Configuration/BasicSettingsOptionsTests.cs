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
                             ["Basic.Settings:ClientId"] = options.ClientId,
                             ["Basic.Settings:Audience"] = options.Audience,
                             ["Basic.Settings:Authority"] = options.Authority,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Basic");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.Values[TokenKeys.Audience].Should().Be(options.Audience);
        sut.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(_default.ProviderId);
        sut.Values[TokenKeys.AuthenticationTypeKey].Should().Be(_default.AuthenticationType);
        sut.Values[TokenKeys.Scope].Should().Be(_default.Scope);
    }

    [Fact]
    public void CustomStandardShould()
    {
        var options = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Basic.Settings:ClientId"] = options.ClientId,
                             ["Basic.Settings:Audience"] = options.Audience,
                             ["Basic.Settings:Authority"] = options.Authority,
                             ["Basic.Settings:ProviderId"] = options.ProviderId,
                             ["Basic.Settings:AuthenticationType"] = options.AuthenticationType,
                             ["Basic.Settings:Scope"] = options.Scope,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicSettings(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Basic");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.ClientIdKey].Should().Be(options.ClientId);
        sut.Values[TokenKeys.Audience].Should().Be(options.Audience);
        sut.Values[TokenKeys.AuthorityKey].Should().Be(options.Authority);
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(options.ProviderId);
        sut.Values[TokenKeys.AuthenticationTypeKey].Should().Be(options.AuthenticationType);
        sut.Values[TokenKeys.Scope].Should().Be(options.Scope);
    }
}
