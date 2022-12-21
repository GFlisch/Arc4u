using System.Collections.Generic;

namespace Arc4u.Network.Pooling
{
    /// <summary>
    ///     Provides methods for navigating in the directory structure of a remote file system (MFT).
    /// </summary>
    public interface IListDirectory
    {
        /// <summary>
        ///     Returns a collection of full-qualified path names which represents the files in the specified <paramref name="path" />.
        /// </summary>
        public ICollection<string> ListFiles(string path);

        /// <summary>
        ///     Returns a collection of full-qualified path names which represents the sub-directories in the specified <paramref name="path" />.
        /// </summary>
        public ICollection<string> ListDirectories(string path);
    }
}
