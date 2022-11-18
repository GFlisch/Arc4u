using Arc4u.Locking.Abstraction;
using Arc4u.Locking.Redis;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace Arc4u.Locking.UnitTests;

[Trait("Dependency", "Redis")]
public class RedisLockingTest : LockingTest
{
    protected override ILockingDataLayer BuildDataLayer()
    {
        var lockingDl = new RedisLockingDataLayer(() =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = {"localhost: 6379"}
            }), new NullLogger<RedisLockingDataLayer>());
        return lockingDl;
    }
}