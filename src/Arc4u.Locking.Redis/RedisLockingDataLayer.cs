using System.Runtime.CompilerServices;
using Arc4u.Locking.Abstraction;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

[assembly:InternalsVisibleTo("Arc4u.Locking.UnitTests")]

namespace Arc4u.Locking.Redis;

internal class RedisLockingDataLayer : ILockingDataLayer
{
    private readonly ILogger<RedisLockingDataLayer> _logger;
    private readonly ConnectionMultiplexer _multiplexer;

    public RedisLockingDataLayer(Func<ConnectionMultiplexer> multiplexerFactory, ILogger<RedisLockingDataLayer> logger)
    {
        _logger = logger;
        _multiplexer = multiplexerFactory();
    }

    public async Task<Lock?> TryCreateLockAsync(string label, TimeSpan maxAge)
    {
        var result = await InternalCreateLockAsync(label, maxAge);
        if (result)
        {
            async void ReleaseFunction() => await ReleaseLockAsync(label);
            Task KeepAliveFunction() => KeepAlive(label, maxAge);
            return new Lock(KeepAliveFunction, ReleaseFunction);
        }

        return null;
    }

    private async Task<bool> InternalCreateLockAsync(string label, TimeSpan ttl)
    {
        var redisKey = GenerateKey(label);
        
        var transactionScope = _multiplexer.GetDatabase().CreateTransaction();
        transactionScope.AddCondition(Condition.KeyNotExists(redisKey));
        var _ =transactionScope.StringSetAsync(redisKey, new RedisValue(label), ttl);

        return await transactionScope.ExecuteAsync();
    }

    private static RedisKey GenerateKey(string label)
    {
        return new RedisKey(label.ToLowerInvariant());
    }

    private Task ReleaseLockAsync(string label)
    {
        var ret = _multiplexer.GetDatabase().KeyDelete(  GenerateKey(label));
        if (!ret)
        {
            _logger.LogError($"Can not release lock for label {label}");
        }

        return Task.CompletedTask;
    }

    private async Task KeepAlive(string label, TimeSpan ttl)
    {
        var lazyDatabaseValue = _multiplexer.GetDatabase();
        var ret = await lazyDatabaseValue.KeyExpireAsync(  GenerateKey(label), ttl);
        if (!ret)
        {
            _logger.LogCritical($"Can not extend the lock for label {label}");
        }
    }
}