using System;
using System.IO;
using System.Threading.Tasks;
using Arc4u.Network.Pooling;
using Arc4u.Standard.Ftp;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Renci.SshNet;
using Renci.SshNet.Common;
using Xunit;

namespace Arc4u.Standard.UnitTest.Ftp;

public class SftpClientFacadeTest
{
    private readonly Fixture _fixture;

    public SftpClientFacadeTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Exists_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.Exists(It.IsAny<string>())).Returns(true);
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var fileName = _fixture.Create<string>();
        sut.Exists(fileName).Should().BeTrue();

        mock.Verify(client => client.Exists(fileName));
    }

    [Fact]
    public void Exists_ThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();

        mock.Setup(client => client.Exists(It.IsAny<string>())).Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var fileName = _fixture.Create<string>();
        ((Action) (() => sut.Exists(fileName))).Should().Throw<SftpPathNotFoundException>();

        mock.Verify(client => client.Exists(fileName));
    }


    [Fact]
    public void ChangeDirectory_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        sut.ChangeDirectory(path);
        mock.Verify(client => client.ChangeDirectory(path));
    }

    [Fact]
    public void ChangeDirectory_ThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.ChangeDirectory(It.IsAny<string>())).Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        ((Action) (() => sut.ChangeDirectory(path))).Should().Throw<SftpPathNotFoundException>();
        mock.Verify(client => client.ChangeDirectory(path));
    }


    [Fact]
    public void DeleteFile_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        sut.DeleteFile(path);
        mock.Verify(client => client.DeleteFile(path));
    }


    [Fact]
    public void DeleteFile_ThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.DeleteFile(It.IsAny<string>())).Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        ((Action) (() => sut.DeleteFile(path))).Should().Throw<SftpPathNotFoundException>();
        mock.Verify(client => client.DeleteFile(path));
    }

    [Fact]
    public void DownloadFile_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        var stream = new MemoryStream();
        sut.DownloadFile(path, stream);
        mock.Verify(client => client.DownloadFile(path, stream, It.IsAny<Action<ulong>>()));
    }


    [Fact]
    public void DownloadFile_ThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.DownloadFile(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<Action<ulong>>()))
            .Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        var stream = new MemoryStream();
        ((Action) (() => sut.DownloadFile(path, stream))).Should().Throw<SftpPathNotFoundException>();
        mock.Verify(client => client.DownloadFile(path, stream, It.IsAny<Action<ulong>>()));
    }

    [Fact]
    public void ListDirectories_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        sut.ListDirectories(path);
        mock.Verify(client => client.ListDirectory(path, It.IsAny<Action<int>>()));
    }

    [Fact]
    public void ListDirectories_ThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.ListDirectory(It.IsAny<string>(), It.IsAny<Action<int>>()))
            .Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        ((Action) (() => sut.ListDirectories(path))).Should().Throw<SftpPathNotFoundException>();
        mock.Verify(client => client.ListDirectory(path, It.IsAny<Action<int>>()));
    }

    [Fact]
    public void ListFiles_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        sut.ListFiles(path);
        mock.Verify(client => client.ListDirectory(path, It.IsAny<Action<int>>()));
    }


    [Fact]
    public void ListFilesThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.ListDirectory(It.IsAny<string>(), It.IsAny<Action<int>>()))
            .Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var path = _fixture.Create<string>();
        ((Action) (() => sut.ListFiles(path))).Should().Throw<SftpPathNotFoundException>();
        mock.Verify(client => client.ListDirectory(path, It.IsAny<Action<int>>()));
    }


    [Fact]
    public void RenameFile_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var oldFileName = _fixture.Create<string>();
        var newFileName = _fixture.Create<string>();
        sut.RenameFile(oldFileName, newFileName);
        mock.Verify(client => client.RenameFile(oldFileName, newFileName));
    }

    [Fact]
    public void RenameFile_ThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.RenameFile(It.IsAny<string>(), It.IsAny<string>()))
            .Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var oldFileName = _fixture.Create<string>();
        var newFileName = _fixture.Create<string>();
        ((Action) (() => sut.RenameFile(oldFileName, newFileName))).Should().Throw<SftpPathNotFoundException>();
        mock.Verify(client => client.RenameFile(oldFileName, newFileName));
    }

    [Fact]
    public void UploadFile_Called()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var fileName = _fixture.Create<string>();
        var stream = new MemoryStream();
        sut.UploadFile(stream, fileName);
        mock.Verify(client => client.UploadFile(stream, fileName, It.IsAny<Action<ulong>>()));
    }

    [Fact]
    public void UploadFile_ThrowsOnException()
    {
        var mock = new Mock<IActiveState>().As<ISftpClient>();
        mock.Setup(client => client.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Action<ulong>>()))
            .Throws<SftpPathNotFoundException>();
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());
        var fileName = _fixture.Create<string>();
        var stream = new MemoryStream();
        ((Action) (() => sut.UploadFile(stream, fileName))).Should().Throw<SftpPathNotFoundException>();
        mock.Verify(client => client.UploadFile(stream, fileName, It.IsAny<Action<ulong>>()));
    }

    [Fact]
    public void IsActive_Called()
    {
        var activeMock = new Mock<IActiveState>();
        activeMock.Setup(state => state.IsConnected).Returns(true);
        var mock = activeMock.As<ISftpClient>();

        mock.Setup(client => client.Exists(It.IsAny<string>())).Returns(true);
        var sut = new SftpClientFacade(mock.Object, facade => Task.CompletedTask, new NullLogger<SftpClientFacade>());

        sut.IsActive.Should().BeTrue();
        activeMock.Verify(client => client.IsConnected);
    }
    
    [Fact]
    public void Release_Called()
    {
        var activeMock = new Mock<IActiveState>();
        var mock = activeMock.As<ISftpClient>();
        SftpClientFacade? releasedItem = null;
        var sut = new SftpClientFacade(mock.Object, facade =>
        {
            releasedItem = facade;
            return Task.CompletedTask;
        }, new NullLogger<SftpClientFacade>());

        sut.ReleaseClient?.Invoke(sut);

        releasedItem.Should().BeSameAs(sut);
    }
}