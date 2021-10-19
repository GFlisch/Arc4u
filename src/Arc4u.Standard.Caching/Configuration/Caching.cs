using System;
using System.Collections.Generic;

namespace Arc4u.Configuration
{
    public class Caching
    {
        public Caching()
        {
            Caches = new List<CachingCache>();
            Principal = new CachingPrincipal();
        }
        public CachingPrincipal Principal { get; set; }

        public String Default { get; set; }

        public List<CachingCache> Caches { get; set; }
    }
}
