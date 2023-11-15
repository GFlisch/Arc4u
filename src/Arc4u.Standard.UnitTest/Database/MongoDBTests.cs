using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Arc4u.Security.Principal;
using System.Collections.Generic;
using System.Linq;
using Arc4u.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Arc4u.MongoDB;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class MongoDBTests
{
    public MongoDBTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    private class DatabaseDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextBuilder context)
        {
        }
    }

    [Fact]
    public void Test_MongoDB_Single_Server_Should()
    {
        var config = new ConfigurationBuilder()
                .AddInMemoryCollection(
                       new Dictionary<string, string?>
                       {
                           ["ConnectionStrings:mongo"] = "mongodb://localhost:27017/DB1",

                       }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddMongoDatabase<DatabaseDbContext>(configuration, "mongo");

        var app = services.BuildServiceProvider();

        var factory = app.GetService<IMongoClientFactory<DatabaseDbContext>>();
        var client = factory.CreateClient();

        client.Settings.Server.Host.Should().Be("localhost");
        client.Settings.Server.Port.Should().Be(27017);
        client.Settings.Servers.Should().HaveCount(1);
    }

    [Fact]
    public void Test_MongoDB_Cluster_Server_Should()
    {
        var config = new ConfigurationBuilder()
                .AddInMemoryCollection(
                       new Dictionary<string, string?>
                       {
                           ["ConnectionStrings:mongo"] = "mongodb://localhost:27017,localhost:26000/DB2",

                       }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddMongoDatabase<DatabaseDbContext>(configuration, "mongo");

        var app = services.BuildServiceProvider();

        var factory = app.GetService<IMongoClientFactory<DatabaseDbContext>>();
        var client = factory.CreateClient();

        client.Settings.Servers.Should().HaveCount(2);
        client.Settings.Servers.First().Host.Should().Be("localhost");
        client.Settings.Servers.First().Port.Should().Be(27017);

        client.Settings.Servers.Last().Host.Should().Be("localhost");
        client.Settings.Servers.Last().Port.Should().Be(26000);
    }

}
