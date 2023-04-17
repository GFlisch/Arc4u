using System.Collections.Generic;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Arc4u.OAuth2.Extensions;
using FluentAssertions;

namespace Arc4u.Standard.UnitTest;

[Trait("Category", "CI")]
public class UserIdentifierTests
{
    public UserIdentifierTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void CustomIdentifierShould()
    {
        var i1 = _fixture.Create<string>();
        var i2 = _fixture.Create<string>();

        var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(
          new Dictionary<string, string?>
          {
              ["Authentication:UserIdentifier:Type"] = "RequiredDisplayableId",
          }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddUserIdentifier(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<UserIdentifierOption>>();

        sut.Should().NotBeNull();
        sut.Value.Type.Should().Be("RequiredDisplayableId");
    }

    [Fact]
    public void StandardIdentifierShould()
    {

        var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(
          new Dictionary<string, string?>
          {
          }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddUserIdentifier(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<UserIdentifierOption>>();

        sut.Should().NotBeNull();
        sut.Value.Type.Should().Be("UniqueId");
    }
}
