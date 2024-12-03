using Arc4u.MongoDB;
using Arc4u.MongoDB.Configuration;
using Arc4u.MongoDB.Exceptions;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arc4u.UnitTest.Database;

[Trait("Category", "CI")]
public class MongoDBTests
{
    public MongoDBTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    private sealed class Contract
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    };

    private sealed class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    };

    private sealed class NotMapped
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    };

    private sealed class DatabaseDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextBuilder context)
        {
            context.MapCollection("Contracts").With<Contract>();
            context.MapCollection("Contracts").With<Company>();
            context.MapCollection("Companies").With<Contract>();
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
        var client = factory!.CreateClient();

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
        var client = factory!.CreateClient();

        client.Settings.Servers.Should().HaveCount(2);
        client.Settings.Servers.First().Host.Should().Be("localhost");
        client.Settings.Servers.First().Port.Should().Be(27017);

        client.Settings.Servers.Last().Host.Should().Be("localhost");
        client.Settings.Servers.Last().Port.Should().Be(26000);
    }

    [Fact]
    public void Test_MongoDB_ContextBuilder_For_A_Specific_Collection_Should()
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
        var client = factory!.GetCollection<Contract>("Contracts");
        client.Should().NotBeNull();
    }

    [Fact]
    public void Test_MongoDB_ContextBuilder_For_A_Non_Specific_Collection_Should_Fail()
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

        var exception = Record.Exception(() => factory!.GetCollection<Contract>());

        exception.Should().BeOfType<TypeMappedToMoreThanOneCollectionException<Contract>>();
    }

    [Fact]
    public void Test_MongoDB_ContextBuilder_For_A_Non_Mapped_Type_Should_Fail()
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

        var exception = Record.Exception(() => factory!.GetCollection<NotMapped>());

        exception.Should().BeOfType<TypeNotMappedToCollectionException>();
    }
}
