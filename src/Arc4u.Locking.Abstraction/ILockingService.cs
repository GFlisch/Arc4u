namespace Arc4u.Locking.Abstraction;

public interface ILockingService
{
    /// <summary>
    ///     Runs Delegate within a critical section, syncrhonized over all
    /// </summary>
    /// <param name="label">
    ///     Label of the lock.
    ///     <remarks>
    ///         This is being used to synchronize the locks
    ///     </remarks>
    /// </param>
    /// <param name="ttl">
    ///     Time to life for the lock, if the application crashes or the connection between the services is lost.
    ///     <remarks>
    ///         This value should be rather small, even if the delegate being run takes longer. During the run, the lock
    ///         be be kept alive automatically
    ///     </remarks>
    /// </param>
    /// <param name="toBeRun">
    ///     Delegate, that should be run within the critical section
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation and delete the distributed lock
    /// </param>
    /// <returns>
    ///     Task, that finishes when the provided <paramref name="toBeRun" /> has finished and the lock was cleared
    /// </returns>
    Task RunWithinLockAsync(string label, TimeSpan ttl, Func<Task> toBeRun, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a lock, that can be used as distributed lock
    /// </summary>
    /// <param name="label">
    ///     Label of the lock.
    ///     <remarks>
    ///         This is being used to synchronize the locks
    ///     </remarks>
    /// </param>
    /// <param name="ttl">
    ///     Time to life for the lock, if the application crashes or the connection between the services is lost.
    ///     <remarks>
    ///         This value should be rather small, even if the delegate being run takes longer. During the run, the lock
    ///         be be kept alive automatically
    ///     </remarks>
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation and delete the distributed lock
    /// </param>
    /// <returns>
    ///     Lock, that is being refresh on the datalayer as long as it is not disposed
    /// </returns>
    /// <remarks>Most be disposed!</remarks>
    /// <exception cref="Exception">Thrown, when no lock could be obtained</exception>
    Task<Lock> CreateLock(string label, TimeSpan ttl, CancellationToken cancellationToken);

    /// <summary>
    ///     Tries to create a lock, that can be used as distributed lock
    /// </summary>
    /// <param name="label">
    ///     Label of the lock.
    ///     <remarks>
    ///         This is being used to synchronize the locks
    ///     </remarks>
    /// </param>
    /// <param name="ttl">
    ///     Time to life for the lock, if the application crashes or the connection between the services is lost.
    ///     <remarks>
    ///         This value should be rather small, even if the delegate being run takes longer. During the run, the lock
    ///         be be kept alive automatically
    ///     </remarks>
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation and delete the distributed lock
    /// </param>
    /// <returns>
    ///     Lock, that is being refresh on the datalayer as long as it is not disposed. If no lock could be obtained, null will
    ///     be returned.
    /// </returns>
    /// <remarks>Most be disposed!</remarks>
    Task<Lock?> TryCreateLock(string label, TimeSpan ttl, CancellationToken cancellationToken);
}