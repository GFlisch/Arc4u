using Arc4u.Caching;
using Arc4u.Caching.Redis;
using Arc4u.Configuration.Redis;
using Arc4u.Dependency;
using Arc4u.Serializer;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Xunit;

namespace Arc4u.UnitTest.Caching;

public class RedisTests
{
    private readonly Fixture _fixture;

    public RedisTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    [Trait("Category", "CI")]
    public void OptionNameConfigShould()
    {
        // arrange
        var option1 = _fixture.Create<RedisCacheOption>();
        var option2 = _fixture.Create<RedisCacheOption>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Option1:ConnectionString"] = option1.ConnectionString,
                    ["Option1:InstanceName"] = option1.InstanceName,
                    ["Option1:SerializerName"] = option1.SerializerName,
                    ["Option2:ConnectionString"] = option2.ConnectionString,
                    ["Option2:InstanceName"] = option2.InstanceName,
                    ["Option2:SerializerName"] = option2.SerializerName
                }).Build();

        var configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.Configure<RedisCacheOption>("option1", configuration.GetSection("Option1"));
        services.Configure<RedisCacheOption>("option2", configuration.GetSection("Option2"));

        var serviceProvider = services.BuildServiceProvider();

        // act
        var config1 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option1");
        var config2 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option2");

        // assert

        config1.ConnectionString.Should().Be(option1.ConnectionString);
        config1.InstanceName.Should().Be(option1.InstanceName);
        config1.SerializerName.Should().Be(option1.SerializerName);

        config2.ConnectionString.Should().Be(option2.ConnectionString);
        config2.InstanceName.Should().Be(option2.InstanceName);
        config2.SerializerName.Should().Be(option2.SerializerName);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void OptionNameConfigNoDeclaredShould()
    {
        // arrange
        var option1 = _fixture.Create<RedisCacheOption>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Option1:ConnectionString"] = option1.ConnectionString,
                    ["Option1:InstanceName"] = option1.InstanceName,
                    ["Option1:SerializerName"] = option1.SerializerName
                }).Build();

        var configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.Configure<RedisCacheOption>("option1", configuration.GetSection("Option1"));

        var serviceProvider = services.BuildServiceProvider();

        // act
        var config1 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option1");
        var config2 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option2");

        // assert

        config1.ConnectionString.Should().Be(option1.ConnectionString);
        config1.InstanceName.Should().Be(option1.InstanceName);
        config1.SerializerName.Should().Be(option1.SerializerName);

        config2.ConnectionString.Should().BeNull();
        config2.SerializerName.Should().BeNull();
        config2.InstanceName.Should().Be("Default");
    }

    [Fact]
    [Trait("Category", "CI")]
    public void AddOptionByCodeToServiceCollectionShould()
    {
        // arrange
        var option1 = _fixture.Create<RedisCacheOption>();

        IServiceCollection services = new ServiceCollection();

        services.AddRedisCache("option1", options =>
        {
            options.InstanceName = option1.InstanceName;
            options.SerializerName = option1.SerializerName;
            options.ConnectionString = option1.ConnectionString;
        });

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option1");

        // assert
        sut.ConnectionString.Should().Be(option1.ConnectionString);
        sut.InstanceName.Should().Be(option1.InstanceName);
        sut.SerializerName.Should().Be(option1.SerializerName);
    }

    [Fact]
    [Trait("Category", "CI")]
    public void AddOptionByConfigToServiceCollectionShould()
    {
        // arrange
        var option1 = _fixture.Create<RedisCacheOption>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Option1:ConnectionString"] = option1.ConnectionString,
                    ["Option1:InstanceName"] = option1.InstanceName,
                    ["Option1:SerializerName"] = option1.SerializerName
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddRedisCache("option1", configuration, "Option1");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option1");

        // assert
        sut.ConnectionString.Should().Be(option1.ConnectionString);
        sut.InstanceName.Should().Be(option1.InstanceName);
        sut.SerializerName.Should().Be(option1.SerializerName);
    }

    [Fact]
    [Trait("Category", "All")]
    public void DatabaseConnectionShould()
    {
        // arrange

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Store:ConnectionString"] =
                        "localhost:6379,abortConnect=false,connectTimeout=30,connectRetry=5,ssl=false,DefaultDatabase=4",
                    ["Store:InstanceName"] = "db1"
                }).Build();

        var configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        var serilog = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(serilog, false));
        services.AddILogger();

        services.AddTransient<ICache, RedisCache>();
        services.AddRedisCache("Store", configuration, "Store");
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<IObjectSerialization, JsonSerialization>();

        var serviceProvider = services.BuildServiceProvider();

        // act
        var cache = serviceProvider.GetRequiredService<ICache>();

        cache.Initialize("Store");

        cache.Put("test", "test");

        var value = cache.Get<string>("test");

        // assert
        value.Should().Be("test");
    }

    [Fact]
    [Trait("Category", "CI")]
    public void RedisSentinelCacheOptionConfigShould()
    {
        // arrange
        var option1 = _fixture.Create<RedisSentinelCacheOption>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["SentinelOption1:InstanceName"] = option1.InstanceName,
                    ["SentinelOption1:MasterName"] = option1.MasterName,
                    ["SentinelOption1:SerializerName"] = option1.SerializerName,
                    ["SentinelOption1:SentinelEndpoints:0"] = option1.SentinelEndpoints[0],
                    ["SentinelOption1:SentinelEndpoints:1"] = option1.SentinelEndpoints[1],
                    ["SentinelOption1:SentinelEndpoints:2"] = option1.SentinelEndpoints[2]
                }).Build();

        var configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();
        services.AddRedisSentinelCache("sentineloption1", configuration, "SentinelOption1");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<RedisSentinelCacheOption>>()!.Get("sentineloption1");

        // assert
        sut.InstanceName.Should().Be(option1.InstanceName);
        sut.MasterName.Should().Be(option1.MasterName);
        sut.SerializerName.Should().Be(option1.SerializerName);
        sut.SentinelEndpoints.Should().HaveCount(3);
        sut.SentinelEndpoints[0].Should().Be(option1.SentinelEndpoints[0]);
        sut.SentinelEndpoints[1].Should().Be(option1.SentinelEndpoints[1]);
        sut.SentinelEndpoints[2].Should().Be(option1.SentinelEndpoints[2]);
    }

    [Fact]
    [Trait("Category", "All")]
    public void RedisSentinelCacheShould()
    {
        // arrange

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["SentinelOption1:InstanceName"] = "RedisTest-",
                    ["SentinelOption1:RedisPassword"] = "titi",
                    ["SentinelOption1:DefaultDatabase"] = "5",
                    ["SentinelOption1:SentinelEndpoints:0"] = "localhost:16379",
                }).Build();

        var configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        var serilog = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(serilog, false));
        services.AddILogger();

        services.AddTransient<ICache, RedisSentinelCache>();
        services.AddRedisSentinelCache("Store", configuration, "SentinelOption1");
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<IObjectSerialization, JsonSerialization>();

        var serviceProvider = services.BuildServiceProvider();

        // act
        var cache = serviceProvider.GetRequiredService<ICache>();

        cache.Initialize("Store");

        cache.Put("test", "test");

        var value = cache.Get<string>("test");

        // assert
        value.Should().Be("test");
    }
}
