namespace Arc4u.Caching;

public class CacheNotInitializedException : Exception
{
    const string defaultMessage = "The cache used is not initialized!";
    public CacheNotInitializedException() : base(defaultMessage) { }

    public CacheNotInitializedException(string message) : base(message ?? defaultMessage) { }
}
