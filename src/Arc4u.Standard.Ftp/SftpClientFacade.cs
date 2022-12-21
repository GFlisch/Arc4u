using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arc4u.Network.Pooling;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Arc4u.Standard.Ftp
{
    public class SftpClientFacade : PoolableItem, IMftClient
    {
        private readonly SftpClient _client;

        public SftpClientFacade(SftpClient client, Func<SftpClientFacade, Task> releaseFunc) : base(item => releaseFunc((SftpClientFacade)item))
        {
             _client = client;
        }

        public override bool IsActive => _client.IsConnected;

        // SFTP Client
        public void ChangeDirectory(string path)
        {
            _client.ChangeDirectory(path);
        }

        public void UploadFile(Stream input, string path)
        {
            _client.UploadFile(input, path);
        }

        public void DownloadFile(string path, Stream output)
        {
            _client.DownloadFile(path, output);
        }

        public bool Exists(string path)
        {
            return _client.Exists(path);
        }

        public void RenameFile(string oldPath, string newPath)
        {
            _client.RenameFile(oldPath,newPath);
        }

        public void DeleteFile(string path)
        {
            _client.DeleteFile(path);
        }

        //IListDirectory
        public ICollection<string> ListFiles(string path)
        {
            return _client.ListDirectory(path).Where(x => x.IsRegularFile).Select(x => x.FullName).ToList();
        }

        public ICollection<string> ListDirectories(string path)
        {
            return _client.ListDirectory(path).Where(x => x.IsDirectory).Select(x => x.FullName).ToList();
        }
    }

    public abstract class SftpClientLoggingDecoratorFacade<T> : PoolableItem, IMftClient
    where T:PoolableItem, IMftClient
    {
        private readonly T _decoree;
        private readonly ILogger<SftpClientLoggingDecoratorFacade<T>> _logger;
        
        protected abstract bool ThrowOnError { get; }

        public SftpClientLoggingDecoratorFacade(T decoree, ILogger<SftpClientLoggingDecoratorFacade<T>> logger) : base(decoree.ReleaseClient)
        {
            _decoree = decoree;
            _logger = logger;
        }

        public override bool IsActive => _decoree.IsActive;
        public ICollection<string> ListFiles(string path)
        {
            try
            {
                return _decoree.ListFiles(path);
            }
            catch (SftpPathNotFoundException e)
            {
                _logger.LogError($"");
                if (ThrowOnError)
                {
                    throw;
                }

                return Array.Empty<string>();
            }
        }

        public ICollection<string> ListDirectories(string path)
        {
            try
            {
                return _decoree.ListDirectories(path);
            }
            catch (SftpPathNotFoundException e)
            {
                _logger.LogError($"");
                if (ThrowOnError)
                {
                    throw;
                }

                return Array.Empty<string>();
            }
        }

        public void ChangeDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void UploadFile(Stream input, string path)
        {
            throw new NotImplementedException();
        }

        public void DownloadFile(string path, Stream output)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string path)
        {
            throw new NotImplementedException();
        }

        public void RenameFile(string oldPath, string newPath)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            
        }
    }
}
