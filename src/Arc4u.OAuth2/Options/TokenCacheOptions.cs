namespace Arc4u.OAuth2.Options;
public class TokenCacheOptions
{
    public TimeSpan MaxTime { get; set; } = TimeSpan.FromMinutes(20);

    public string CacheName { get; set; } = default!;
}
