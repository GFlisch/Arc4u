using System.Collections.Generic;

namespace Arc4u.Configuration;

public class Caching
{
    public Caching()
    {
        Caches = new List<CachingCache>();
    }

    public string Default { get; set; }

    public List<CachingCache> Caches { get; set; }
}
