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
        sut.ReleaseClient(disConnected);

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

    private ConnectionPool<PoolableItem> BuildConnectionPool()
    {
        return new ConnectionPool<PoolableItem>(Mock.Of<ILogger<ConnectionPool<PoolableItem>>>(),
            _fixture.Create<IClientFactory<TestPoolableItem>>());
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class TestPoolableItem : PoolableItem
    {
        public TestPoolableItem()
        {
            IsActive = true;
        }

        public TestPoolableItem(bool isActive)
        {
            IsActive = isActive;
        }

        public override bool IsActive { get; }
    }
}