namespace Arc4u.Caching
{
    /// <summary>
    /// Exception used to encapsulate the error from the cache engine.
    /// </summary>
    public class DataCacheException : Exception
    {
        public DataCacheException(String message) : base(message)
        {
        }
    }
}
