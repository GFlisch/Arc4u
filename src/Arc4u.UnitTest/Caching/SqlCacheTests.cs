using Arc4u.Caching.Sql;
using Arc4u.Configuration.Sql;
using Arc4u.Dependency;
using Arc4u.Serializer;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Arc4u.UnitTest.Caching;

/// <summary>
/// To run the test all the tests, you need a sql server instance.
/// To prepare the database install the followin dotnet tools.
///  dotnet tool install --global dotnet-sql-cache
/// Run this command:
///  dotnet sql-cache create "Data Source=localhost;Initial Catalog=CacheTestDB;User ID=sa;Password=P@ssw0rd!;Connect Timeout=30;Encrypt=False" dbo TestCache
/// </summary>
public class SqlCacheTests
{
    public SqlCacheTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    [Trait("Category", "CI")]
    public void AddOptionByCodeToServiceCollectionShould()
    {
        // arrange
        var option1 = _fixture.Create<SqlCacheOption>();

        IServiceCollection services = new ServiceCollection();

        services.AddSqlCache("option1", options =>
        {
            options.SchemaName = option1.SchemaName;
            options.SerializerName = option1.SerializerName;
            options.TableName = option1.TableName;
            options.ConnectionString = option1.ConnectionString;
        });

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SqlCacheOption>>()!.Get("option1");

        // assert
        sut.SchemaName.Should().Be(option1.SchemaName);
        sut.TableName.Should().Be(option1.TableName);
        sut.SerializerName.Should().Be(option1.SerializerName);
        sut.ConnectionString.Should().Be(option1.ConnectionString);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void AddOptionByConfigToServiceCollectionShould()
    {
        // arrange
        var option1 = _fixture.Create<SqlCacheOption>();

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Option1:SchemaName"] = option1.SchemaName,
                             ["Option1:TableName"] = option1.TableName,
                             ["Option1:ConnectionString"] = option1.ConnectionString,
                             ["Option1:SerializerName"] = option1.SerializerName,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddSqlCache("option1", configuration, "Option1");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<SqlCacheOption>>()!.Get("option1");

        // assert
        sut.SchemaName.Should().Be(option1.SchemaName);
        sut.TableName.Should().Be(option1.TableName);
        sut.SerializerName.Should().Be(option1.SerializerName);
        sut.ConnectionString.Should().Be(option1.ConnectionString);
    }

    [Fact]
    [Trait("Category", "All")]
    public void SqlCacheShould()
    {
        // arrange
        IServiceCollection services = new ServiceCollection();

        var builder = new SqlConnectionStringBuilder
        {
            ConnectTimeout = 30,
            InitialCatalog = "CacheTestDB",
            Encrypt = false,
            Password = "P@ssw0rd!",
            UserID = "sa",
            DataSource = "localhost"
        };

        services.AddSqlCache("storeName", options =>
        {
            options.ConnectionString = builder.ConnectionString;
            options.SchemaName = "dbo";
            options.TableName = "TestCache";
        });

        services.AddTransient<IObjectSerialization, JsonSerialization>();

        var serviceProvider = services.BuildServiceProvider();

        _fixture.Inject<IServiceProvider>(serviceProvider);

        var mockIOptions = _fixture.Freeze<Mock<IOptionsMonitor<SqlCacheOption>>>();
        mockIOptions.Setup(m => m.Get("storeName")).Returns(serviceProvider.GetService<IOptionsMonitor<SqlCacheOption>>()!.Get("storeName"));

        // act
        var cache = _fixture.Create<SqlCache>();

        cache.Initialize("storeName");

        cache.Put("test", "test");

        var value = cache.Get<string>("test");

        // assert
        value.Should().Be("test");
    }
}
