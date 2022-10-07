using System.Runtime.CompilerServices;
using Arc4u.Locking.Abstraction;
using StackExchange.Redis;

[assembly:InternalsVisibleTo("Arc4u.Locking.UnitTests")]

namespace Arc4u.Locking.Redis;
internal class RedisLockingDataLayer : ILockingDataLayer
{
    private readonly Lazy<IDatabase> _lazyDatabase;
    
    public RedisLockingDataLayer(Func<ConnectionMultiplexer> multiplexerFactory)
    {
        _lazyDatabase = new Lazy<IDatabase>(() => multiplexerFactory().GetDatabase(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
    
    public async Task<Lock?> TryCreateLock(string label, TimeSpan maxAge)
    {
        var redisKey = new RedisKey(label.ToLowerInvariant());
        var transactionScope = _lazyDatabase.Value.CreateTransaction();
        transactionScope.AddCondition(Condition.KeyNotExists(redisKey));
       var incrementTask = transactionScope.HashIncrementAsync(redisKey, new RedisValue(label), 1);
        var expiryTask = transactionScope.KeyExpireAsync(redisKey, maxAge);
        var result = await transactionScope.ExecuteAsync();
        Task.WaitAll(incrementTask, expiryTask);
        if (result)
        {
            async void ReleaseFunction() => await ReleaseLock(label);

            return new Lock(ReleaseFunction);
        }

        return null;
    }

    public async Task ReleaseLock(string label)
    {
        await _lazyDatabase.Value.KeyDeleteAsync(label);
    }
}