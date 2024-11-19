namespace Arc4u.Caching;
public interface ICacheContext
{
    ICache this[string cacheName] { get; }

    ICache Default { get; }
    // CachingPrincipal Principal { get; set; }

    bool Exist(string cacheName);
}
