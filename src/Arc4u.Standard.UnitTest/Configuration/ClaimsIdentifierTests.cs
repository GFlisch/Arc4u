using System.Collections.Generic;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Arc4u.OAuth2.Configuration;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Arc4u.OAuth2.Extensions;

namespace Arc4u.Standard.UnitTest;

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
              ["Authentication:ClaimsIdentifer:0"] = i1,
              ["Authentication:ClaimsIdentifer:1"] = i2,
          }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddClaimsIdentifier(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<ClaimsIdentifierOption>>();

        sut.Should().NotBeNull();
        sut.Value.Should().HaveCount(2);
        sut.Value.Should().Equal(i1,i2);
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
