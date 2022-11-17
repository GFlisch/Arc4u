namespace Arc4u.Locking.Abstraction;

public class LockingConfiguration
{
    public TimeSpan RefreshRate { get; set; } = TimeSpan.FromSeconds(5);
}