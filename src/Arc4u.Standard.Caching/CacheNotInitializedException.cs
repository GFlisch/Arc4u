using System;

namespace Arc4u.Caching
{
    public class CacheNotInitializedException : Exception
    {
        public CacheNotInitializedException() : base($"The cache used is not initialized!") { }
    }
}
