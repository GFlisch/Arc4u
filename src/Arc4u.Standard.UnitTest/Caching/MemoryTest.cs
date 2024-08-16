using AutoFixture.AutoMoq;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Xunit;
using Arc4u.Caching.Memory;
using System.Globalization;
using FluentAssertions;
using Moq;
using Arc4u.Serializer;
using Arc4u.Dependency;
using System;
using Arc4u.Configuration.Memory;
using Arc4u.Caching;

namespace Arc4u.UnitTest.Caching;

[Trait("Category", "CI")]
public class MemoryTest
{
    public MemoryTest()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void AddOptionByCodeToServiceCollectionShould()
    {
        // arrange
        var option1 = _fixture.Create<MemoryCacheOption>();

        IServiceCollection services = new ServiceCollection();

        services.AddMemoryCache("option1", options =>
        {
            options.SizeLimitInMB = option1.SizeLimitInMB;
            options.SerializerName = option1.SerializerName;
            options.CompactionPercentage = option1.CompactionPercentage;
        });

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<MemoryCacheOption>>()!.Get("option1");

        // assert
        sut.CompactionPercentage.Should().Be(option1.CompactionPercentage);
        sut.SizeLimitInMB.Should().Be(option1.SizeLimitInMB * 1024 * 1024);
        sut.SerializerName.Should().Be(option1.SerializerName);
    }

    [Fact]
    public void AddOptionByConfigToServiceCollectionShould()
    {
        // arrange
        var option1 = _fixture.Build<MemoryCacheOption>().With(m => m.CompactionPercentage, 0.8).Create(); ;

        var config = new ConfigurationBuilder()
                     .AddInMemoryCollection(
                         new Dictionary<string, string?>
                         {
                             ["Option1:SizeLimitInMB"] = option1.SizeLimitInMB.ToString(CultureInfo.InvariantCulture),
                             ["Option1:CompactionPercentage"] = option1.CompactionPercentage.ToString(CultureInfo.InvariantCulture),
                             ["Option1:SerializerName"] = option1.SerializerName,
                         }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddMemoryCache("option1", configuration, "Option1");

        var serviceProvider = services.BuildServiceProvider();

        // act
        var sut = serviceProvider.GetService<IOptionsMonitor<MemoryCacheOption>>()!.Get("option1");

        // assert
        sut.CompactionPercentage.Should().Be(option1.CompactionPercentage);
        sut.SizeLimitInMB.Should().Be(option1.SizeLimitInMB * 1024 * 1024);
        sut.SerializerName.Should().Be(option1.SerializerName);
    }

    [Fact]
    public void MemoryCacheShould()
    {
        // arrange

        var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(
                                 new Dictionary<string, string?>
                                 {
                                     ["Store:SizeLimitInMB"] = "10"
                                 }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddMemoryCache("Store", configuration, "Store");
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<IObjectSerialization, JsonSerialization>();

        var serviceProvider = services.BuildServiceProvider();

        var mockIContainer = _fixture.Freeze<Mock<IContainerResolve>>();
        IObjectSerialization serializer = serviceProvider.GetRequiredService<IObjectSerialization>();
        mockIContainer.Setup(m => m.TryResolve<IObjectSerialization>(out serializer)).Returns(true);

        var mockIOptions = _fixture.Freeze<Mock<IOptionsMonitor<MemoryCacheOption>>>();
        mockIOptions.Setup(m => m.Get("Store")).Returns(serviceProvider.GetService<IOptionsMonitor<MemoryCacheOption>>()!.Get("Store"));

        // act
        var cache = _fixture.Create<MemoryCache>();

        cache.Initialize("Store");

        cache.Put("test", "test");

        var value = cache.Get<string>("test");

        // assert
        value.Should().Be("test");
    }

    [Fact]
    public void MemoryCacheNotInitializedShould()
    {
        // arrange

        var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(
                                 new Dictionary<string, string?>
                                 {
                                     ["Store:SizeLimitInMB"] = "10"
                                 }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddMemoryCache("Store", configuration, "Store");
        services.AddSingleton<IConfiguration>(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var mockIContainer = _fixture.Freeze<Mock<IContainerResolve>>();
        IObjectSerialization? serializer = null;
        mockIContainer.Setup(m => m.TryResolve<IObjectSerialization>(out serializer)).Returns(false);

        var mockIOptions = _fixture.Freeze<Mock<IOptionsMonitor<MemoryCacheOption>>>();
        mockIOptions.Setup(m => m.Get("Store")).Returns(serviceProvider.GetService<IOptionsMonitor<MemoryCacheOption>>()!.Get("Store"));

        // act
        var cache = _fixture.Create<MemoryCache>();

        cache.Initialize("Store");

        var exception = Record.Exception(() => cache.Put("test", "test"));

        exception.Should().BeOfType<CacheNotInitializedException>();
        exception.Message.Should().Be("Memory Cache Store is not initialized. An IObjectSerialization instance cannot be resolved via the Ioc.");
    }

    [Fact]
    public void MemoryCacheShouldNotBeInitialized()
    {
        // arrange

        var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(
                                 new Dictionary<string, string?>
                                 {
                                     ["Store:SizeLimitInMB"] = "-1"
                                 }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddMemoryCache("Store", configuration, "Store");
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<IObjectSerialization, JsonSerialization>();

        var serviceProvider = services.BuildServiceProvider();

        var mockIContainer = _fixture.Freeze<Mock<IContainerResolve>>();
        IObjectSerialization serializer = serviceProvider.GetRequiredService<IObjectSerialization>();
        mockIContainer.Setup(m => m.TryResolve<IObjectSerialization>(out serializer)).Returns(true);
        
        var mockIOptions = _fixture.Freeze<Mock<IOptionsMonitor<MemoryCacheOption>>>();
        mockIOptions.Setup(m => m.Get("Store")).Returns(serviceProvider.GetService<IOptionsMonitor<MemoryCacheOption>>()!.Get("Store"));

        // act
        var cache = _fixture.Create<MemoryCache>();

        var exception = Record.Exception(() => cache.Initialize("Store"));

        exception.Should().BeOfType<ArgumentOutOfRangeException>();
        exception.Message.Should().StartWith("value must be non-negative.");

        exception = Record.Exception(() => cache.Put("test", "test"));

        
        exception.Message.Should().StartWith("Memory Cache Store is not initialized. With exception: value must be non-negative.");

    }
}
