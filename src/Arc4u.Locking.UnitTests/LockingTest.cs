using System;
using System.Threading;
using System.Threading.Tasks;
using Arc4u.Locking.Abstraction;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Arc4u.Locking.UnitTests;

public abstract class LockingTest
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task CreateLock_UsingRedis_SecondCallIsBlocked()
    {
        var lockingDl = BuildDataLayer();
        await CreateLock_SecondCallIsBlocked(lockingDl);
    }

    [Fact]
    public async Task TryCreateLock_UsingRedis_SecondCallIsBlocked()
    {
        var lockingDl = BuildDataLayer();
        await TryCreateLock_SecondCallIsBlocked(lockingDl);
    }

    [Fact]
    public async Task RunWithinLock_UsingRedis_SecondCallIsBlocked()
    {
        var lockingDl = BuildDataLayer();

        await RunWithinLock_SecondCallIsBlocked(lockingDl);
    }

    [Fact]
    public async Task RunWithinLock_UsingRedis_CancelFirstLockRunsSecond()
    {
        var lockingDl = BuildDataLayer();
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

        var run = false;
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

    private async Task RunWithinLock_SecondCallIsBlocked(ILockingDataLayer lockingDl)
    {
        var service = new LockingService(lockingDl, new LockingConfiguration(), new NullLogger<LockingService>());

        var label = _fixture.Create<string>();

        var firstTask = service.RunWithinLockAsync(label, TimeSpan.FromMinutes(2),
            async () => { await Task.Delay(TimeSpan.FromSeconds(3)); }, CancellationToken.None);

        var run = false;
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


    private async Task CreateLock_SecondCallIsBlocked(ILockingDataLayer lockingDl)
    {
        var service = new LockingService(lockingDl, new LockingConfiguration(), new NullLogger<LockingService>());

        var label = _fixture.Create<string>();


        var cancellationSource = new CancellationTokenSource();
        var firstTask = Task.Run(async () =>
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

    private async Task TryCreateLock_SecondCallIsBlocked(ILockingDataLayer lockingDl)
    {
        var service = new LockingService(lockingDl, new LockingConfiguration(), new NullLogger<LockingService>());

        var label = _fixture.Create<string>();

        var cancellationSource = new CancellationTokenSource();
        var firstLock = await service.TryCreateLock(label, TimeSpan.FromMinutes(2), CancellationToken.None);
        var firstTask = Task.Run(async () =>
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

    protected abstract ILockingDataLayer BuildDataLayer();
}