using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Extensions;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest;

[Trait("Category", "CI")]
public class ClaimsIdentifierTests
{
    public ClaimsIdentifierTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void CustomClaimsShould()
    {
        var i1 = _fixture.Create<string>();
        var i2 = _fixture.Create<string>();

        var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(
          new Dictionary<string, string?>
          {
              ["Authentication:ClaimsIdentifier:0"] = i1,
              ["Authentication:ClaimsIdentifier:1"] = i2,
          }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddClaimsIdentifier(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<ClaimsIdentifierOption>>();

        sut.Should().NotBeNull();
        sut.Value.Should().HaveCount(2);
        sut.Value.Should().Equal(i1, i2);
    }

    [Fact]
    public void StandardClaimsShould()
    {

        var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(
          new Dictionary<string, string?>
          {
          }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddClaimsIdentifier(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<ClaimsIdentifierOption>>();

        sut.Should().NotBeNull();
        sut.Value.Should().HaveCount(2);
        sut.Value.Should().Equal("http://schemas.microsoft.com/identity/claims/objectidentifier", "oid");
    }
}
