using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Arc4u.Caching.Sql;
using FluentAssertions;
using Moq;
using Arc4u.Serializer;
using Arc4u.Dependency;
using Microsoft.Data.SqlClient;
using Arc4u.Configuration.Sql;

namespace Arc4u.Standard.UnitTest.Caching;

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
        var builder = new SqlConnectionStringBuilder();
        builder.ConnectTimeout = 30;
        builder.InitialCatalog = "CacheTestDB";
        builder.Encrypt = false;
        builder.Password = "P@ssword";
        builder.UserID = "sa";
        builder.DataSource = "localhost";

        services.AddSqlCache("storeName", options =>
        {
            options.ConnectionString = builder.ConnectionString;
        });

        services.AddTransient<IObjectSerialization, JsonSerialization>();

        var serviceProvider = services.BuildServiceProvider();

        var mockIContainer = _fixture.Freeze<Mock<IContainerResolve>>();
        mockIContainer.Setup(m => m.Resolve<IObjectSerialization>()).Returns(serviceProvider.GetService<IObjectSerialization>()!);

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
