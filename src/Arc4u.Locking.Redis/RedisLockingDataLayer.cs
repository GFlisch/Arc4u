using System.Runtime.CompilerServices;
using System.Transactions;
using Arc4u.Locking.Abstraction;
using StackExchange.Redis;

[assembly:InternalsVisibleTo("Arc4u.Locking.UnitTests")]

namespace Arc4u.Locking.Redis;
internal class RedisLockingDataLayer : ILockingDataLayer
{
//    private readonly Lazy<IDatabase> _lazyDatabase;
//    private readonly Lazy<IDatabase> _lazyDatabase;
    private readonly ConnectionMultiplexer _multiplexer;

    public RedisLockingDataLayer(Func<ConnectionMultiplexer> multiplexerFactory)
    {
        _multiplexer = multiplexerFactory();
      /*  _lazyDatabase = new Lazy<IDatabase>(() =>
            {
                var ret= _multiplexer.GetDatabase();
                ret.Multiplexer.ErrorMessage += (sender, args) =>
                {
                    Console.WriteLine(args);
                };
                ret.Multiplexer.InternalError  += (sender, args) =>
                {
                    Console.WriteLine(args);
                };
                ret.Multiplexer.ConnectionFailed+= (sender, args) =>
                {
                    Console.WriteLine(args);
                };
                ret.Multiplexer.ConnectionRestored+= (sender, args) =>
                {
                    Console.WriteLine(args);
                };
                
                return ret;
            },
            LazyThreadSafetyMode.ExecutionAndPublication);*/
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
        // var redisKey = new RedisKey(label.ToLowerInvariant());
        // var transactionScope = _multiplexer.GetDatabase().CreateTransaction();
        // transactionScope.AddCondition(Condition.KeyNotExists(redisKey));
        // var incrementTask = transactionScope.HashIncrementAsync(redisKey, new RedisValue(label), 1);
        // var expiryTask = transactionScope.KeyExpireAsync(redisKey, ttl);
        // var result = await transactionScope.ExecuteAsync();


     var result =   _multiplexer.GetDatabase().StringSet(new RedisKey(label),new RedisValue(label), ttl);
        
        
        //actually we do not need to wait on that, because it should be part of the result, but since this is actually
        //an internal of the Redis implementation, it is better to understand this way and safer for the future
//        Task.WaitAll(incrementTask, expiryTask);
        return result;
    }

    private async Task ReleaseLock(string label)
    {
    // var keys =   _multiplexer.GetServer("localhost:6379").Keys().ToList();
    // var listKeys = new List<string>();
     //listKeys.AddRange(keys.GetAsyncEnumerator() .Select(key => (string)key).ToList());

     var ret = _multiplexer.GetDatabase().KeyDelete(label);
       Console.WriteLine(ret);
    }
    
    private async Task KeepAlive(string label, TimeSpan ttl)
    {
        var lazyDatabaseValue =_multiplexer.GetDatabase();
        var ret = await lazyDatabaseValue.KeyExpireAsync(label, ttl);
       if (ret)
         Console.WriteLine(ret);
       else 
           Console.WriteLine(ret);
       
    }
}