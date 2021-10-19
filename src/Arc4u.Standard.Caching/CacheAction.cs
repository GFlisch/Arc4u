using System;

namespace Arc4u.Caching
{
    [Flags]
    public enum CacheAction
    {
        Added = 1,
        Removed = 2,
        Updated = 4
    }
}
