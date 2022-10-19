using System.Runtime.CompilerServices;
using System.Transactions;
using Arc4u.Locking.Abstraction;
using StackExchange.Redis;

[assembly:InternalsVisibleTo("Arc4u.Locking.UnitTests")]

namespace Arc4u.Locking.Redis;

internal class RedisLockingDataLayer : ILockingDataLayer
{
    private readonly ConnectionMultiplexer _multiplexer;

    public RedisLockingDataLayer(Func<ConnectionMultiplexer> multiplexerFactory)
    {
        _multiplexer = multiplexerFactory();
    }

    public async Task<Lock?> TryCreateLock(string label, TimeSpan maxAge)
    {
        var result = await InternalCreateLock(label, maxAge);
        if (result)
        {
            async void ReleaseFunction() => await ReleaseLock(label);
            Task KeepAliveFunction() => KeepAlive(label, maxAge);
            return new Lock(KeepAliveFunction, ReleaseFunction);
        }

        return null;
    }

    private async Task<bool> InternalCreateLock(string label, TimeSpan ttl)
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

    private async Task ReleaseLock(string label)
    {
        var ret = _multiplexer.GetDatabase().KeyDelete(  GenerateKey(label));
        if (!ret)
        {
            // log the error
        }
    }

    private async Task KeepAlive(string label, TimeSpan ttl)
    {
        var lazyDatabaseValue = _multiplexer.GetDatabase();
        var ret = await lazyDatabaseValue.KeyExpireAsync(  GenerateKey(label), ttl);
        if (!ret)
        {
            throw new SystemException("can not set expiry!");
            // log the error
        }
    }
}