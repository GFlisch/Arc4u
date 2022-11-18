using System.IO;

namespace Arc4u.Network.Pooling
{
    public interface IMftClient : IListDirectory
    {
        void ChangeDirectory(string path);

        void UploadFile(Stream input, string path);

        void DownloadFile(string path, Stream output);

        bool Exists(string path);

        void RenameFile(string oldPath, string newPath);

        void DeleteFile(string path);
    }

}