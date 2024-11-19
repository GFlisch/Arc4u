namespace Arc4u.Caching
{
    /// <summary>
    /// Event argument to inform an action on the cache.
    /// </summary>
    public class CacheEventArgs : EventArgs
    {
        public CacheEventArgs(String key, CacheAction action)
        {
            _key = key;
            _cacheAction = action;
        }

        private readonly String _key;
        /// <summary>
        /// The <see cref="String"/> used to identify the object in the cache.
        /// </summary>
        public String Key { get { return _key; } }

        private readonly CacheAction _cacheAction;
        /// <summary>
        /// Action performed on the object. Can be an update, an insert or a delete.
        /// </summary>
        public CacheAction Action { get { return _cacheAction; } }
    }
}
