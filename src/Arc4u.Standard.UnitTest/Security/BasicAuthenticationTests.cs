using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Extensions;
using System;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Arc4u.Standard.OAuth2.Options;
using Arc4u.Standard.OAuth2;
using Arc4u.Standard.OAuth2.Middleware;
using Arc4u.OAuth2.Token;

namespace Arc4u.Standard.UnitTest.Security;

[Trait("Category", "CI")]
public class BasicAuthenticationTests
{
    public BasicAuthenticationTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void Default_Authority_Should()
    {
        var defaultAuthority = BuildAuthority();

        var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(
                               new Dictionary<string, string?>
                               {
                                   ["Authentication:DefaultAuthority:Url"] = defaultAuthority.Url,
                                   ["Authentication:DefaultAuthority:TokenEndpoint"] = defaultAuthority.TokenEndpoint,

                               }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);

        var app = services.BuildServiceProvider();

        var sut = app.GetRequiredService<IOptionsMonitor<AuthorityOptions>>().Get("Default");

        sut.Url.Should().Be(defaultAuthority.Url);
        sut.TokenEndpoint.Should().Be(defaultAuthority.TokenEndpoint);
    }

    [Fact]
    public void Basic_With_Default_Authority_Should()
    {
        var basicSettings = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(
                               new Dictionary<string, string?>
                               {
                                   ["Authentication:Basic:Settings:ClientId"] = basicSettings.ClientId,
                                   ["Authentication:Basic:Settings:Scope"] = basicSettings.Scope,

                               }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var app = services.BuildServiceProvider();

        var sut = app.GetRequiredService<IOptionsMonitor<BasicAuthenticationSettingsOptions>>().CurrentValue;

        sut.BasicSettings.Values[TokenKeys.ClientIdKey].Should().Be(basicSettings.ClientId);
        sut.BasicSettings.Values[TokenKeys.Scope].Should().Be(basicSettings.Scope);

        var sutAuthority = app.GetRequiredService<IOptionsMonitor<AuthorityOptions>>().Get("Basic");

        sutAuthority.Url.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public void Basic_With_Dedicated_Authority_Should()
    {
        var authority = BuildAuthority();
        var basicSettings = _fixture.Create<BasicSettingsOptions>();

        var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(
                               new Dictionary<string, string?>
                               {
                                   ["Authentication:Basic:Settings:ClientId"] = basicSettings.ClientId,
                                   ["Authentication:Basic:Settings:Scope"] = basicSettings.Scope,
                                   ["Authentication:Basic:Settings:Authority:url"] = authority.Url,
                                   ["Authentication:Basic:Settings:Authority:TokenEndpoint"] = authority.TokenEndpoint,

                               }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddBasicAuthenticationSettings(configuration);

        var app = services.BuildServiceProvider();

        var sut = app.GetRequiredService<IOptionsMonitor<BasicAuthenticationSettingsOptions>>().CurrentValue;

        sut.BasicSettings.Values[TokenKeys.ClientIdKey].Should().Be(basicSettings.ClientId);
        sut.BasicSettings.Values[TokenKeys.Scope].Should().Be(basicSettings.Scope);

        var sutAuthority = app.GetRequiredService<IOptionsMonitor<AuthorityOptions>>().Get("Basic");

        sutAuthority.Url.Should().Be(authority.Url);
        sutAuthority.TokenEndpoint.Should().Be(authority.TokenEndpoint);
    }

    private AuthorityOptions BuildAuthority() => _fixture.Build<AuthorityOptions>().With(p => p.TokenEndpoint, _fixture.Create<Uri>().AbsoluteUri).Create();
}
