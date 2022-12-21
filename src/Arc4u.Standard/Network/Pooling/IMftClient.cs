using System.Collections.Generic;
using System.IO;

namespace Arc4u.Network.Pooling
{
    public interface IMftClient : IListDirectory
    {
        
        public void ChangeDirectory(string path);

        public void UploadFile(Stream input, string path);

        public void DownloadFile(string path, Stream output);

        public bool Exists(string path);

        public void RenameFile(string oldPath, string newPath);

        public void DeleteFile(string path);
    }

}