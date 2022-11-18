using System.Runtime.CompilerServices;
using Arc4u.Locking.Abstraction;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

[assembly: InternalsVisibleTo("Arc4u.Locking.UnitTests")]

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

    /// <inheritdoc />
    public async Task<Lock?> TryCreateLockAsync(string label, TimeSpan maxAge, CancellationToken cancellationToken)
    {
        var result = await TryCreateRedisEntryAsync(label, maxAge);
        if (result)
        {
            async void ReleaseFunction()
            {
                await ReleaseLockAsync(label);
            }

            Task KeepAliveFunction()
            {
                return KeepAlive(label, maxAge, cancellationToken);
            }

            return new Lock(KeepAliveFunction, ReleaseFunction, cancellationToken);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<Lock?> TryCreateLockAsync(string label, TimeSpan maxAge, Func<Task> cleanUpCallBack,
        CancellationToken cancellationToken)
    {
        var result = await TryCreateRedisEntryAsync(label, maxAge);
        Console.WriteLine($"Got lock for {label} at {DateTime.Now.TimeOfDay}");
        if (result)
        {
            async void ReleaseFunction()
            {
                await ReleaseLockAsync(label);
                await cleanUpCallBack();
            }

            Task KeepAliveFunction()
            {
                return KeepAlive(label, maxAge, cancellationToken);
            }

            return new Lock(KeepAliveFunction, ReleaseFunction, cancellationToken);
        }

        return null;
    }

    /// <summary>
    ///     Creates an entry on redis, if it is not present yet
    /// </summary>
    /// <param name="label">Label to be used for the entry</param>
    /// <param name="ttl">
    ///     Timespan that the entry will be kept, when no <seealso cref="KeepAlive" /> is called for that
    ///     <paramref name="label" />
    /// </param>
    /// <returns>true, if the entry has been created</returns>
    /// <remarks>
    ///     A transaction is being used to ensure, that no entry is already present for the <paramref name="label" />
    ///     The entries value will just be the label again, since we do not care for the value, but the expiry.
    /// </remarks>
    private async Task<bool> TryCreateRedisEntryAsync(string label, TimeSpan ttl)
    {
        var redisKey = GenerateKey(label);

        var transactionScope = _multiplexer.GetDatabase().CreateTransaction();
        transactionScope.AddCondition(Condition.KeyNotExists(redisKey));
        var _ = transactionScope.StringSetAsync(redisKey, new RedisValue(label), ttl);

        return await transactionScope.ExecuteAsync();
    }

    /// <summary>
    ///     creating a key for redis, by lower casing the provided string
    /// </summary>
    /// <param name="label">Label to create a key for</param>
    /// <returns>RedisKey with lowercased <paramref name="label" /></returns>
    private static RedisKey GenerateKey(string label)
    {
        return new RedisKey(label.ToLowerInvariant());
    }

    /// <summary>
    /// Releasing a lock on the Redis 
    /// </summary>
    /// <param name="label">Label of the lock, that should be released</param>
    /// <returns>Task to await the releasing</returns>
    private Task ReleaseLockAsync(string label)
    {
        var ret = _multiplexer.GetDatabase().KeyDelete(GenerateKey(label));
        if (!ret) _logger.LogError($"Can not release lock for label {label}");

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Keeping the Redis Value alive by setting the expiry
    /// </summary>
    /// <param name="label">Label that should be kept alive</param>
    /// <param name="ttl">Time to live, that should be set. After that period, Redis will delete the entry</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    ///     if the <paramref name="cancellationToken" /> was already canceled, nothing will be done
    /// </remarks>
    private async Task KeepAlive(string label, TimeSpan ttl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        var lazyDatabaseValue = _multiplexer.GetDatabase();
        var ret = await lazyDatabaseValue.KeyExpireAsync(GenerateKey(label), ttl);
        if (!ret) _logger.LogCritical($"Can not extend the lock for label {label}");
    }
}