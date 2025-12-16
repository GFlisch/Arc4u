namespace Arc4u.Configuration.Redis;

public class RedisSentinelCacheOption
{
    /// <summary>
    /// Name of this cache instance (optional, used for logging or multi-instance scenarios)
    /// </summary>
    public string InstanceName { get; set; } = "";

    /// <summary>
    /// The name of the master in Sentinel configuration
    /// </summary>
    public string MasterName { get; set; } = "";

    /// <summary>
    /// List of Sentinel endpoints (host:port)
    /// </summary>
    public string[] SentinelEndpoints { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Optional serializer name for the cache
    /// </summary>
    public string? SerializerName { get; set; }
}
