namespace Arc4u.Caching.Memory;
public class MemoryCacheOption
{
  public double CompactionPercentage { get; set; } = 0.2;
  public long SizeLimit { get; set; } = 1024 * 1024 * 1024;
  public string? SerializerName { get; set; }
}
