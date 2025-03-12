using Arc4u.OAuth2.TicketStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest.Security;

public class CacheTicketStoreOptionsTests
{
    [Fact]
    public void GetOptionsWithNothingInConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddCacheTicketStore(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CacheTicketStoreOptions>>().Value;

        Assert.Equal("Default", options.CacheName);
        Assert.Equal("AuthSessionStore-", options.KeyPrefix);
    }

    [Fact]
    public void GetOptionsWithSpecificCacheName()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AuthenticationCacheTicketStore:CacheName", "CustomCache" }
            })
            .Build();

        services.AddCacheTicketStore(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CacheTicketStoreOptions>>().Value;

        Assert.Equal("CustomCache", options.CacheName);
        Assert.Equal("AuthSessionStore-", options.KeyPrefix);
    }

    [Fact]
    public void GetOptionsWithSpecificKeyPrefix()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AuthenticationCacheTicketStore:KeyPrefix", "CustomPrefix-" }
            })
            .Build();

        services.AddCacheTicketStore(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CacheTicketStoreOptions>>().Value;

        Assert.Equal("Default", options.CacheName);
        Assert.Equal("CustomPrefix-", options.KeyPrefix);
    }

    [Fact]
    public void GetOptionsWithSpecificValues()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AuthenticationCacheTicketStore:CacheName", "CustomCache" },
                { "AuthenticationCacheTicketStore:KeyPrefix", "CustomPrefix-" }
            })
            .Build();

        services.AddCacheTicketStore(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CacheTicketStoreOptions>>().Value;

        Assert.Equal("CustomCache", options.CacheName);
        Assert.Equal("CustomPrefix-", options.KeyPrefix);
    }
}
