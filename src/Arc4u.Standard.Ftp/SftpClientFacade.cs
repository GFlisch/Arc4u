using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arc4u.Network.Pooling;
using Renci.SshNet;

namespace Arc4u.Standard.Ftp
{
    public class SftpClientFacade : PoolableItem, IMftClient
    {
        private readonly SftpClient _client;

        public SftpClientFacade(SftpClient client)
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
}
