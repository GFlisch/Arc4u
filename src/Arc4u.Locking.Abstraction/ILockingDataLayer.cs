namespace Arc4u.Locking.Abstraction;

public interface ILockingDataLayer
{
    /// <summary>
    /// Creating a lock on the database being used
    /// </summary>
    /// <param name="label">
    /// Label for the lock
    /// <remarks>This label will be used as id to ensure the lock is only entered once</remarks>
    /// </param>
    /// <param name="maxAge">
    /// <see cref="TimeSpan"/> after which the lock will be deleted in case of inactivity
    /// <remarks>
    /// the lock will be kept alive while the process is running. The timeout strokes, when there is no more activity, due to an exception for example.
    /// </remarks>
    /// </param>
    /// <returns>
    /// <seealso cref="Lock"/> which will take of releasing the lock on the database
    /// <remarks>
    /// must be disposed after usage! (Best use in using!)
    /// </remarks>
    /// </returns>
    Task<Lock?> TryCreateLockAsync(string label, TimeSpan maxAge);
}