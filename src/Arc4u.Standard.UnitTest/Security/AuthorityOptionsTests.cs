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

namespace Arc4u.Standard.UnitTest.Security;


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
        var option = _fixture.Create<AuthorityOptions>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
       new Dictionary<string, string?>
       {
           ["Authentication:DefaultAuthority:Url"] = option.Url,
           ["Authentication:DefaultAuthority:TokenEndpointV2"] = option.TokenEndpointV2,
           ["Authentication:DefaultAuthority:TokenEndpointV1"] = option.TokenEndpointV1,
       }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);

        var app = services.BuildServiceProvider();

        var options = app.GetRequiredService<IOptions<AuthorityOptions>>().Value;

        options.Should().NotBeNull();
        options.Url.Should().Be(option.Url);
        options.TokenEndpointV1.Should().Be(option.TokenEndpointV1);
        options.TokenEndpointV2.Should().Be(option.TokenEndpointV2);
    }

    [Fact]
    public void Authority_With_Only_Url_OptionsShould()
    {
        var option = _fixture.Create<AuthorityOptions>();
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

        var options = app.GetRequiredService<IOptions<AuthorityOptions>>().Value;

        options.Should().NotBeNull();
        options.Url.Should().Be(option.Url);
        options.TokenEndpointV1.Should().Be(_default.TokenEndpointV1);
        options.TokenEndpointV2.Should().Be(_default.TokenEndpointV2);
    }

}
