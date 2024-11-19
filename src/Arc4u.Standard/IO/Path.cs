namespace Arc4u.IO
{
    /// <summary>
    /// The class help you to retrieve and construct a relative path.
    /// </summary>
    public static class Path
    {
        /// <summary>
        /// Give a path and tell you if the path is relative or not.
        /// </summary>
        /// <param name="path">the relative path to test.</param>
        /// <returns>true if relative.</returns>
        public static bool IsRelative(string path)
        {
            return !System.IO.Path.IsPathRooted(path);

            throw new InvalidOperationException("Cannot test if the path relative.");
        }

        /// <summary>
        /// From two path (a root) and the full path, the method returns the relative path from the root.
        /// </summary>
        /// <param name="rootPath">relative to this.</param>
        /// <param name="fullFileName">the complete file name.</param>
        /// <returns>the relative path.</returns>
        public static string MakeRelative(string rootPath, string fullFileName)
        {
            var root = rootPath.Replace('/', '\\');
            var fullUri = new Uri(fullFileName);
            var rootUri = new Uri(root.Last().Equals('\\') ? root : root + @"\");

            var result = rootUri.MakeRelativeUri(fullUri).ToString().Replace('/', '\\');

            if (result.Equals(fullFileName))
            {
                throw new InvalidOperationException($"Impossible to find a match between {root} and {fullFileName}.");
            }

            return result;
        }

        /// <summary>
        /// Construct the full file name from the root and the relative part.
        /// </summary>
        /// <param name="rootPath">the root path.</param>
        /// <param name="relativePath">the relative path.</param>
        /// <returns>the complete file name.</returns>
        public static string MakeFullPath(string rootPath, string relativePath)
        {
            var full = System.IO.Path.Combine(rootPath, relativePath);
            return System.IO.Path.GetFullPath(full);
        }
    }
}
