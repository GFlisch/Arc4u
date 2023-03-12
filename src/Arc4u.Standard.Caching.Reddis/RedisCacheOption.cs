namespace Arc4u.Caching.Redis;
public class RedisCacheOption
{
    public string? ConnectionString { get; set; }
    public string DatabaseName { get; set; } = "Default";
    public string? SerializerName { get; set; }
}
