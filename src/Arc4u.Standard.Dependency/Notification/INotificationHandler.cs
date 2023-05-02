using System.Threading.Tasks;
using System.Threading;

namespace Arc4u.Dependency.Notification;

/// <summary>
/// Will be used to implement a notification.
/// </summary>
/// <typeparam name="T">The type of the object notified.</typeparam>
public interface INotificationHandler<T>
{
    Task HandleAsync(T entity, CancellationToken cancellationToken);
}
