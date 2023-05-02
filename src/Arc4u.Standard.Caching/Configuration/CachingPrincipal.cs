using System;

namespace Arc4u.Configuration;

public class CachingPrincipal
{
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(20);

    public string CacheName { get; set; }
}
