using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Extensions;
using FluentAssertions;
using System;

namespace Arc4u.UnitTest.Security;


[Trait("Category", "CI")]
public class AuthorityOptionsTests
{
    
    public AuthorityOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void AuthorityOptionsShould()
    {
        var option = _fixture.Build<AuthorityOptions>().With(p => p.TokenEndpoint, _fixture.Create<Uri>().AbsoluteUri).Create();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
       new Dictionary<string, string?>
       {
           ["Authentication:DefaultAuthority:Url"] = option.Url,
           ["Authentication:DefaultAuthority:TokenEndpoint"] = option.TokenEndpoint,
       }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);

        var app = services.BuildServiceProvider();

        var options = app.GetRequiredService<IOptionsMonitor<AuthorityOptions>>().Get("Default");

        options.Should().NotBeNull();
        options.Url.Should().Be(option.Url);
        options.TokenEndpoint.Should().Be(option.TokenEndpoint);
    }

    [Fact]
    public void Authority_With_Only_Url_OptionsShould()
    {
        var option = _fixture.Build<AuthorityOptions>().With(p => p.TokenEndpoint, _fixture.Create<Uri>().AbsoluteUri).Create();
        var _default = new AuthorityOptions();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
       new Dictionary<string, string?>
       {
           ["Authentication:DefaultAuthority:Url"] = option.Url,
       }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);

        var app = services.BuildServiceProvider();

        var options = app.GetRequiredService<IOptionsMonitor<AuthorityOptions>>().Get("Default");

        options.Should().NotBeNull();
        options.Url.Should().Be(option.Url);
        options.TokenEndpoint.Should().Be(_default.TokenEndpoint);
    }

}
