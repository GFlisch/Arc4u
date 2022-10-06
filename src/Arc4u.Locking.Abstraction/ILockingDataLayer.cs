namespace Arc4u.Locking.Abstraction;

public interface ILockingDataLayer
{
    Task CreateLock(string label, TimeSpan maxAge);
    Task<Lock?> TryCreateLock(string label, TimeSpan maxAge);
    Task ReleaseLock(string label);
}