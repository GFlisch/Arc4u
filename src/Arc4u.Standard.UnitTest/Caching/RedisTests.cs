using AutoFixture.AutoMoq;
using AutoFixture;
using Arc4u.Caching.Redis;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Xunit;

namespace Arc4u.Standard.UnitTest.Caching;
public class RedisTests
{
    public RedisTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
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
                                     ["Option1:DatabaseName"] = option1.DatabaseName,
                                     ["Option1:SerializerName"] = option1.SerializerName,
                                     ["Option2:ConnectionString"] = option2.ConnectionString,
                                     ["Option2:DatabaseName"] = option2.DatabaseName,
                                     ["Option2:SerializerName"] = option2.SerializerName,
                                 }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.Configure<RedisCacheOption>("option1", configuration.GetSection("Option1"));
        services.Configure<RedisCacheOption>("option2", configuration.GetSection("Option2"));

        var serviceProvider = services.BuildServiceProvider();

        // act
        var config1 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option1");
        var config2 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option2");

        // assert

        config1.ConnectionString.Should().Be(option1.ConnectionString);
        config1.DatabaseName.Should().Be(option1.DatabaseName);
        config1.SerializerName.Should().Be(option1.SerializerName);

        config2.ConnectionString.Should().Be(option2.ConnectionString);
        config2.DatabaseName.Should().Be(option2.DatabaseName);
        config2.SerializerName.Should().Be(option2.SerializerName);
    }

    [Fact]
    public void OptionNameConfigNoDeclaredShould()
    {
        // arrange
        var option1 = _fixture.Create<RedisCacheOption>();

        var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(
                                 new Dictionary<string, string?>
                                 {
                                     ["Option1:ConnectionString"] = option1.ConnectionString,
                                     ["Option1:DatabaseName"] = option1.DatabaseName,
                                     ["Option1:SerializerName"] = option1.SerializerName,
                                 }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.Configure<RedisCacheOption>("option1", configuration.GetSection("Option1"));

        var serviceProvider = services.BuildServiceProvider();

        // act
        var config1 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option1");
        var config2 = serviceProvider.GetService<IOptionsMonitor<RedisCacheOption>>()!.Get("option2");

        // assert

        config1.ConnectionString.Should().Be(option1.ConnectionString);
        config1.DatabaseName.Should().Be(option1.DatabaseName);
        config1.SerializerName.Should().Be(option1.SerializerName);

        config2.ConnectionString.Should().BeNull();
        config2.SerializerName.Should().BeNull();
        config2.DatabaseName.Should().Be("Default");

    }
}
