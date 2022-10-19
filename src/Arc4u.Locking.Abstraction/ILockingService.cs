namespace Arc4u.Locking.Abstraction;

public interface ILockingService
{
    Task RunWithinLock(string label, TimeSpan maxAge, Func<Task> toBeRun);
}