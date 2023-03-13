namespace Arc4u.Caching.Redis;
public class RedisCacheOption
{
    public string? ConnectionString { get; set; }
    public string InstanceName { get; set; } = "Default";
    public string? SerializerName { get; set; }
}
