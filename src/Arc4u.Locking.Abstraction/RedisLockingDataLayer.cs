using System.Runtime.CompilerServices;
using StackExchange.Redis;

[assembly:InternalsVisibleTo("Arc4u.Locking.UnitTests")]

namespace Arc4u.Locking.Abstraction;
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
        transactionScope.StringSetAsync(new RedisKey(label.ToLowerInvariant()), new RedisValue(label), maxAge);
        var result = await transactionScope.ExecuteAsync();
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

    public async Task CreateLock(string label, TimeSpan maxAge)
    {
        await _lazyDatabase.Value.StringSetAsync(new RedisKey(label.ToLowerInvariant()), new RedisValue(), maxAge);
    }
}