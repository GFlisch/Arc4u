using System;
using System.Reflection;
using System.Threading.Tasks;
using Arc4u.Network.Pooling;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arc4u.Standard.UnitTest.Ftp;

public class SftpConnectionPoolTests
{
    private readonly Fixture _fixture;

    public SftpConnectionPoolTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void SftpConnection_HappyPath_GetNewClient()
    {
        var sut = BuildConnectionPool();
        var c = sut.GetClient();

        c.IsActive.Should().BeTrue();
        sut.ConnectionsCount.Should().Be(0);
    }

    [Fact]
    public void SftpConnection_HappyPath_GetCachedClientAndRelease()
    {
        var disConnected = new TestPoolableItem(false);
        var sut = BuildConnectionPool();
        ReleaseClient(sut, disConnected);

        sut.ConnectionsCount.Should().Be(0);
        using var c = sut.GetClient();
        {
            c.IsActive.Should().BeTrue();
            sut.ConnectionsCount.Should().Be(0);
        }
        c.Dispose();
        sut.ConnectionsCount.Should().Be(1);
    }

    [Fact]
    public void SftpConnection_HappyPath_GetAndReleaseClient()
    {
        var sut = BuildConnectionPool();

        sut.ConnectionsCount.Should().Be(0);
        using var c = sut.GetClient();
        {
            c.IsActive.Should().BeTrue();
        }
        c.Dispose();
        sut.ConnectionsCount.Should().Be(1);
    }

    [Fact]
    public void SftpConnection_Client_Should_AddThePoolOnlyOnce()
    {
        var sut = BuildConnectionPool();

        sut.ConnectionsCount.Should().Be(0);
        using var c = sut.GetClient();
        {
            c.IsActive.Should().BeTrue();
        }
        c.Dispose();
        sut.ConnectionsCount.Should().Be(1);
        c.Dispose();
        c.Dispose();
        sut.ConnectionsCount.Should().Be(1);
    }

    private Task ReleaseClient<T>(ConnectionPool<T> pool, T item) where T : PoolableItem
    {
        var releaseMethod =
            typeof(ConnectionPool<PoolableItem>).GetMethod("ReleaseClient",
                BindingFlags.Instance | BindingFlags.NonPublic);
        releaseMethod.Should().NotBeNull();
        return (Task) releaseMethod!.Invoke(pool, new object[] {item});
    }

    private ConnectionPool<PoolableItem> BuildConnectionPool()
    {
        ConnectionPool<PoolableItem> ret = null!;

        var frozenYoghurt = _fixture.Freeze<Mock<IClientFactory<TestPoolableItem>>>();
        Func<TestPoolableItem, Task> foo = item => ReleaseClient(ret!, item);
        frozenYoghurt.Setup(factory => factory.CreateClient(It.IsAny<Func<TestPoolableItem, Task>>()))
           .Returns(new TestPoolableItem(foo));

        var clientFactory = frozenYoghurt.Object;
        ret = new ConnectionPool<PoolableItem>(Mock.Of<ILogger<ConnectionPool<PoolableItem>>>(),
            clientFactory);
        return ret;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class TestPoolableItem : PoolableItem
    {
        
        public TestPoolableItem(Func<TestPoolableItem, Task> releaseFunc) : base( item => releaseFunc((TestPoolableItem)item)){        IsActive = true;}
        
        public TestPoolableItem() : this( item => Task.CompletedTask)
        {
    
        }

        public TestPoolableItem(bool isActive) : this()
        {
            IsActive = isActive;
        }

        public override bool IsActive { get; }
    }
}