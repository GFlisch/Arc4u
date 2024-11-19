using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

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
    public async Task AuthorityOptionsShould()
    {
        var option = _fixture.Build<AuthorityOptions>().With(p => p.TokenEndpoint, _fixture.Create<Uri>()).Create();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
       new Dictionary<string, string?>
       {
           ["Authentication:DefaultAuthority:Url"] = option.Url.ToString(),
           ["Authentication:DefaultAuthority:TokenEndpoint"] = option.TokenEndpoint!.ToString(),
       }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);

        var app = services.BuildServiceProvider();

        var options = app.GetRequiredService<IOptionsMonitor<AuthorityOptions>>().Get("Default");

        options.Should().NotBeNull();
        options.Url.Should().Be(option.Url);
        options.TokenEndpoint.Should().Be(option.TokenEndpoint);
        (await options.GetEndpointAsync(CancellationToken.None).ConfigureAwait(false)).Should().Be(option.TokenEndpoint);
    }

    [Fact]
    public async Task Authority_Options_With_Construction_Of_Metadata_Should()
    {
        var config = new ConfigurationBuilder()
             .AddInMemoryCollection(
        new Dictionary<string, string?>
        {
            ["Authentication:DefaultAuthority:Url"] = "https://login.microsoftonline.com/e564e8c4-2da9-4f0b-8e3d-c1a065b60501/v2.0",
        }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDefaultAuthority(configuration);

        var app = services.BuildServiceProvider();

        var options = app.GetRequiredService<IOptionsMonitor<AuthorityOptions>>().Get("Default");

        options.Should().NotBeNull();
        options.Url.Should().Be("https://login.microsoftonline.com/e564e8c4-2da9-4f0b-8e3d-c1a065b60501/v2.0");
        options.TokenEndpoint.Should().BeNull();
        (await options.GetEndpointAsync(CancellationToken.None).ConfigureAwait(false)).Should().Be("https://login.microsoftonline.com/e564e8c4-2da9-4f0b-8e3d-c1a065b60501/oauth2/v2.0/token");
        options.TokenEndpoint.Should().Be("https://login.microsoftonline.com/e564e8c4-2da9-4f0b-8e3d-c1a065b60501/oauth2/v2.0/token");
        options.GetMetaDataAddress().Should().Be("https://login.microsoftonline.com/e564e8c4-2da9-4f0b-8e3d-c1a065b60501/v2.0/.well-known/openid-configuration");
    }

    [Fact]
    public void Authority_With_Only_Url_OptionsShould()
    {
        var option = _fixture.Build<AuthorityOptions>().With(p => p.TokenEndpoint, _fixture.Create<Uri>()).Create();
        var _default = new AuthorityOptions();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
       new Dictionary<string, string?>
       {
           ["Authentication:DefaultAuthority:Url"] = option.Url.ToString(),
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
