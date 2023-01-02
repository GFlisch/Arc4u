using System.IO;

namespace Arc4u.Network.Pooling;

public interface IRemoteFileSystem : IListDirectory
{
    /// <summary>Changes remote directory to path.</summary>
    /// <param name="path">New directory path.</param>
    public void ChangeDirectory(string path);

    /// <summary>Uploads stream into remote file.</summary>
    /// <param name="input">Data input stream.</param>
    /// <param name="path">Remote file path.</param>
    /// <remarks>
    ///     Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions
    ///     thrown by the stream.
    /// </remarks>
    public void UploadFile(Stream input, string path);

    /// <summary>
    ///     Downloads remote file specified by the path into the stream.
    /// </summary>
    /// <param name="path">File to download.</param>
    /// <param name="output">Stream to write the file into.</param>
    /// <remarks>
    ///     Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions
    ///     thrown by the stream.
    /// </remarks>
    public void DownloadFile(string path, Stream output);


    /// <summary>Checks whether file or directory exists</summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///     <c>true</c> if directory or file exists; otherwise <c>false</c>.
    /// </returns>
    public bool Exists(string path);

    /// <summary>Renames remote file from old path to new path.</summary>
    /// <param name="oldPath">Path to the old file location.</param>
    /// <param name="newPath">Path to the new file location.</param>
    public void RenameFile(string oldPath, string newPath);

    /// <summary>Deletes remote file specified by path.</summary>
    /// <param name="path">File to be deleted path.</param>
    public void DeleteFile(string path);
}