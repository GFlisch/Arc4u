namespace Arc4u.Locking.Abstraction;

public interface ILockingService
{
    Task RunWithinLockAsync(string label, TimeSpan maxAge, Func<Task> toBeRun, CancellationToken cancellationToken);
}