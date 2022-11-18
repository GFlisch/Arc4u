using Arc4u.Network.Pooling;
using Arc4u.Standard.Ftp;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arc4u.Standard.UnitTest.Ftp
{
    public class SftpConnectionPoolTests
    {
        [Fact]
        public void SftpConnection_HappyPath_GetNewClient()
        {
            var connected = new Mock<SftpClientFacade>();
            connected.Setup(c => c.IsActive).Returns(true);

            var factory = new Mock<SftpClientFactoryMock>();
            factory.Setup(f => f.CreateClient()).Returns(connected.Object);

            var sut = new ConnectionPool<SftpClientFacade>(Mock.Of<ILogger<ConnectionPool<SftpClientFacade>>>(), factory.Object);
            var c = sut.GetClient();

            c.IsActive.Should().BeTrue();
            sut.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void SftpConnection_HappyPath_GetCachedClientAndRelease()
        {
            var connected = new Mock<SftpClientFacade>();
            connected.Setup(c => c.IsActive).Returns(true);

            var disConnected = new Mock<SftpClientFacade>();
            disConnected.Setup(c => c.IsActive).Returns(false);

            var factory = new Mock<SftpClientFactoryMock>();
            factory.Setup(f => f.CreateClient()).Returns(connected.Object);

            var sut = new ConnectionPool<SftpClientFacade>(Mock.Of<ILogger<ConnectionPool<SftpClientFacade>>>(), factory.Object);
            sut.ReleaseClient(disConnected.Object);

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
            var connected = new Mock<SftpClientFacade>();
            connected.Setup(c => c.IsActive).Returns(true);

            var factory = new Mock<SftpClientFactoryMock>();
            factory.Setup(f => f.CreateClient()).Returns(connected.Object);

            var sut = new ConnectionPool<SftpClientFacade>(Mock.Of<ILogger<ConnectionPool<SftpClientFacade>>>(), factory.Object);

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
            var connected = new Mock<SftpClientFacade>();
            connected.Setup(c => c.IsActive).Returns(true);

            var factory = new Mock<SftpClientFactoryMock>();
            factory.Setup(f => f.CreateClient()).Returns(connected.Object);

            var sut = new ConnectionPool<SftpClientFacade>(Mock.Of<ILogger<ConnectionPool<SftpClientFacade>>>(), factory.Object);

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
    }
}
