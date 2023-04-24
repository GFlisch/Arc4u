using System.Collections.Generic;

namespace Arc4u.Dependency.Notification;

/// <summary>
/// Interface to return the registered <see cref="INotificationHandler{T}"/> in the DI.
/// </summary>
/// <typeparam name="T">An object, most of the time a domain object.</typeparam>
public interface INotificationHandlers<T>
{
    IEnumerable<INotificationHandler<T>> Handlers { get; }
}
