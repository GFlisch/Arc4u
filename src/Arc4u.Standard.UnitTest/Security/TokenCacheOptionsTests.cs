using Arc4u.Configuration;
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
public class TokenCacheOptionsTests
{
    public TokenCacheOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void TockeCacheOption_From_Action_Should()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddTokenCache(options =>
        {
            options.CacheName = "test";
        });

        var servicePovider = services.BuildServiceProvider();

        var options = servicePovider.GetRequiredService<IOptions<TokenCacheOptions>>();

        var sut = options.Value;

        sut.CacheName.Should().Be("test");
    }

    [Fact]
    public void TockeCacheOption_From_Config_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var options = _fixture.Create<TokenCacheOptions>();

        var config = new ConfigurationBuilder()
              .AddInMemoryCollection(
                  new Dictionary<string, string?>
                  {
                      ["Authentication:TokenCache:CacheName"] = options.CacheName,
                  }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        services.AddTokenCache(configuration);

        var servicePovider = services.BuildServiceProvider();

        var sutOptions = servicePovider.GetRequiredService<IOptions<TokenCacheOptions>>();

        var sut = sutOptions.Value;

        sut.CacheName.Should().Be(options.CacheName);
    }

    [Fact]
    public void TockeCacheOption_From_Config_With_Default_MaxTime_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var options = _fixture.Build<TokenCacheOptions>().With(p => p.CacheName, _fixture.Create<string>()).Create();
        var defaultOptions = new TokenCacheOptions();

        var config = new ConfigurationBuilder()
              .AddInMemoryCollection(
                  new Dictionary<string, string?>
                  {
                      ["Authentication:TokenCache:CacheName"] = options.CacheName,
                  }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        services.AddTokenCache(configuration);

        var servicePovider = services.BuildServiceProvider();

        var sutOptions = servicePovider.GetRequiredService<IOptions<TokenCacheOptions>>();

        var sut = sutOptions.Value;

        sut.CacheName.Should().Be(options.CacheName);
    }

    [Fact]
    public void TockeCacheOption_From_Config_With_MaxTime_To_Zero_Exception_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var options = _fixture.Create<TokenCacheOptions>();

        var config = new ConfigurationBuilder()
              .AddInMemoryCollection(
                  new Dictionary<string, string?>
                  {
                      ["Authentication:TokenCache:CacheName"] = options.CacheName
                  }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        var exception = Record.Exception(() => services.AddTokenCache(configuration));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void TockeCacheOption_From_Config_With_No_CacheName_Exception_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var config = new ConfigurationBuilder()
              .AddInMemoryCollection(
                  new Dictionary<string, string?>
                  {
                      ["Authentication:TokenCache:CacheName"] = ""
                  }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        var exception = Record.Exception(() => services.AddTokenCache(configuration));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConfigurationException>();
    }
}
