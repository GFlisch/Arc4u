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

public interface INotificationHandler<T1,T2>
{
    Task HandleAsync(T1 param1, T2 param2, CancellationToken cancellationToken);
}

public interface INotificationHandler<T1, T2, T3>
{
    Task HandleAsync(T1 param1, T2 param2, T3 param3, CancellationToken cancellationToken);
}

public interface INotificationHandler<T1, T2, T3, T4>
{
    Task HandleAsync(T1 param1, T2 param2, T3 param3, T4 param4, CancellationToken cancellationToken);
}

public interface INotificationHandler<T1, T2, T3, T4, T5>
{
    Task HandleAsync(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, CancellationToken cancellationToken);
}
