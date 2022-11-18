namespace Arc4u.Locking.Abstraction;

/// <summary>
/// Configuration related to distributed locking
/// </summary>
public class LockingConfiguration
{
    /// <summary>
    /// TimeSpan, on how often a Lock will be refreshed, when the locked action is still being executed
    /// </summary>
    public TimeSpan RefreshRate { get; set; } = TimeSpan.FromSeconds(5);
}