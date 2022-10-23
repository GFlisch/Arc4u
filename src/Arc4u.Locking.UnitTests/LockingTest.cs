using System;
using System.Threading;
using System.Threading.Tasks;
using Arc4u.Locking.Abstraction;
using Arc4u.Locking.Redis;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace Arc4u.Locking.UnitTests;

public class LockingTest
{
    private readonly Fixture _fixture = new();

    [Fact]
    [Trait("Dependency", "Redis")]
    public async Task RunWithinLock_UsingRedis_SecondCallIsBlocked()
    {
        var lockingDl = new RedisLockingDataLayer(() =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = {"localhost: 6379"}
            }), new NullLogger<RedisLockingDataLayer>());

        await RunWithinLock_SecondCallIsBlocked(lockingDl);
    }

    [Fact]
    public async Task RunWithinLock_UsingInMemory_SecondCallIsBlocked()
    {
        var lockingDl = new MemoryLockingDataLayer();
        await RunWithinLock_SecondCallIsBlocked(lockingDl);
    }

    private async Task RunWithinLock_SecondCallIsBlocked(ILockingDataLayer lockingDl)
    {
        var service = new LockingService(lockingDl, new LockingConfiguration(), new NullLogger<LockingService>());

        var label = _fixture.Create<string>();

        var firstTask = service.RunWithinLockAsync(label, TimeSpan.FromMinutes(2),
            async () => { await Task.Delay(TimeSpan.FromSeconds(3)); }, CancellationToken.None);

        bool run = false;
        var secondTask = service.RunWithinLockAsync(label, TimeSpan.FromMinutes(2), async () =>
            {
                run = true;
                await Task.Delay(TimeSpan.FromSeconds(1));
            },
            CancellationToken.None);

        await secondTask;

        run.Should().BeFalse();
        await firstTask;
    }


    [Fact]
    public async Task RunWithinLock_UsingRedis_CancelFirstLockRunsSecond()
    {
        var lockingDl = new RedisLockingDataLayer(() =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = {"localhost: 6379"}
            }), new NullLogger<RedisLockingDataLayer>());

        var service = new LockingService(lockingDl, new LockingConfiguration(), new NullLogger<LockingService>());

        var label = _fixture.Create<string>();

        var cancellationSource = new CancellationTokenSource();
        var firstTask = service.RunWithinLockAsync(label, TimeSpan.FromMinutes(2),
            async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                cancellationSource.Cancel();
                await Task.Delay(TimeSpan.FromSeconds(3));
            }, cancellationSource.Token);

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        bool run = false;
        var secondTask = service.RunWithinLockAsync(label, TimeSpan.FromMinutes(2), async () =>
            {
                run = true;
                await Task.Delay(TimeSpan.FromSeconds(1));
            },
            CancellationToken.None);


        await secondTask;
        await firstTask;

        run.Should().BeTrue();
        await firstTask;
    }

    [Fact]
    public async Task CreateLock_UsingRedis_SecondCallIsBlocked()
    {
        var lockingDl = new RedisLockingDataLayer(() =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = {"localhost: 6379"}
            }), new NullLogger<RedisLockingDataLayer>());

        var service = new LockingService(lockingDl, new LockingConfiguration(), new NullLogger<LockingService>());

        var label = _fixture.Create<string>();


        var cancellationSource = new CancellationTokenSource();
        Task firstTask = Task.Run(async () =>
        {
            using var @lock = await service.CreateLock(label, TimeSpan.FromMinutes(2), CancellationToken.None);
            await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationSource.Token);
        });

        Func<Task> sut = async () =>
        {
            using var @lock = await service.CreateLock(label, TimeSpan.FromMinutes(2), CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationSource.Token);
        };
        await sut.Should().ThrowAsync<Exception>().WithMessage($"Could not obtain a lock for label {label}");
        await firstTask;
    }

    [Fact]
    public async Task TryCreateLock_UsingRedis_SecondCallIsBlocked()
    {
        var lockingDl = new RedisLockingDataLayer(() =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = {"localhost: 6379"}
            }), new NullLogger<RedisLockingDataLayer>());

        var service = new LockingService(lockingDl, new LockingConfiguration(), new NullLogger<LockingService>());

        var label = _fixture.Create<string>();

        var cancellationSource = new CancellationTokenSource();
        var firstLock = await service.TryCreateLock(label, TimeSpan.FromMinutes(2), CancellationToken.None);
        Task firstTask = Task.Run(async () =>
        {
            using (firstLock)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationSource.Token);
            }
        });

        using var secondLock = await service.TryCreateLock(label, TimeSpan.FromMinutes(2), CancellationToken.None);
        secondLock.Should().BeNull();
        await firstTask;
    }
}