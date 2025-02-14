namespace Arc4u.Configuration.Redis;
public class RedisCacheOption
{
    public string? ConnectionString { get; set; }
    public string InstanceName { get; set; } = "Default";
    public string? SerializerName { get; set; }
}
