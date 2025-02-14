namespace Arc4u.Configuration.Memory;
public class MemoryCacheOption
{
    public double CompactionPercentage { get; set; } = 0.2;
    public long SizeLimitInMB { get; set; } = 100;
    public string? SerializerName { get; set; }
}
