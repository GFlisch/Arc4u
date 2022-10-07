using Arc4u.Locking.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Arc4u.Locking.Redis;

public static class StartupExtension
{
    public static IServiceCollection UseRedisLocking(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILockingDataLayer, RedisLockingDataLayer>();
        serviceCollection.AddSingleton(CreateRedisConfiguration);
        serviceCollection.AddSingleton(CreateMultiplexerFactory);
        return serviceCollection;
    }

    private static ConfigurationOptions CreateRedisConfiguration(IServiceProvider serviceProvider)
    {
        var configuration =  serviceProvider.GetService<IConfiguration>();
        if (configuration is null)
        {
            throw new SystemException($"Could not resolve {nameof(IConfiguration)}");
        }
      
        var redisConfiguration = new RedisConfiguration();
        configuration.GetSection(nameof(RedisConfiguration)).Bind(redisConfiguration);
        return new ConfigurationOptions() {EndPoints = {redisConfiguration.Host},};
    }

    private static Func<ConnectionMultiplexer> CreateMultiplexerFactory(IServiceProvider serviceProvider)
    {
        ConnectionMultiplexer Factory()
        {
            var configuration = serviceProvider.GetService<ConfigurationOptions>();

            return ConnectionMultiplexer.Connect(configuration);
        }

        return Factory;
    }
}