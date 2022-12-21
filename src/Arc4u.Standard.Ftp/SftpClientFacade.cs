using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arc4u.Network.Pooling;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Arc4u.Standard.Ftp;

public class SftpClientFacade : PoolableItem, IMftClient
{
    private readonly SftpClient _client;
    private readonly ILogger<SftpClientFacade> _logger;

    public SftpClientFacade(SftpClient client, Func<SftpClientFacade, Task> releaseFunc,
        ILogger<SftpClientFacade> logger) : base(item => releaseFunc((SftpClientFacade) item))
    {
        _client = client;
        _logger = logger;
    }

    public override bool IsActive => _client.IsConnected;

    // SFTP Client
    public void ChangeDirectory(string path)
    {
        _client.ChangeDirectory(path);
    }

    public void UploadFile(Stream input, string path)
    {
        try
        {
            _client.UploadFile(input, path);
        }
        catch (SftpPathNotFoundException e)
        {
            _logger.LogError(e, $"Error while uploading file >{path}<");
            throw;
        }
    }

    public void DownloadFile(string path, Stream output)
    {
        try
        {
            _client.DownloadFile(path, output);
        }
        catch (SftpPathNotFoundException e)
        {
            _logger.LogError(e, $"Error while downloading file >{path}<");
            throw;
        }
    }

    public bool Exists(string path)
    {
        try
        {
            return _client.Exists(path);
        }
        catch (SftpPathNotFoundException e)
        {
            _logger.LogError(e, $"Error while checking existance of >{path}<");
            throw;
        }
    }

    public void RenameFile(string oldPath, string newPath)
    {
        try
        {
            _client.RenameFile(oldPath, newPath);
        }
        catch (SftpPathNotFoundException e)
        {
            _logger.LogError(e, $"Error while renaming >{oldPath}< to >{newPath}<");
            throw;
        }
    }

    public void DeleteFile(string path)
    {
        try
        {
            _client.DeleteFile(path);
        }
        catch (SftpPathNotFoundException e)
        {
            _logger.LogError(e, $"Error while downloading file >{path}<");
            throw;
        }
    }

    //IListDirectory
    public ICollection<string> ListFiles(string path)
    {
        try
        {
            return _client.ListDirectory(path).Where(x => x.IsRegularFile).Select(x => x.FullName).ToList();
        }
        catch (SftpPathNotFoundException e)
        {
            _logger.LogError(e, $"Error listing files of >{path}<");
            throw;
        }
    }

    public ICollection<string> ListDirectories(string path)
    {
        try
        {
            return _client.ListDirectory(path).Where(x => x.IsDirectory).Select(x => x.FullName).ToList();
        }
        catch (SftpPathNotFoundException e)
        {
            _logger.LogError(e, $"Error listing directories of >{path}<");
            throw;
        }
    }
}