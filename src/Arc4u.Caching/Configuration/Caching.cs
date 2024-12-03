
namespace Arc4u.Configuration;

public class Caching
{
    public Caching()
    {
        Caches = [];
        Default = string.Empty;
    }

    public string Default { get; set; }

    public List<CachingCache> Caches { get; set; }
}
