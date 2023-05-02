using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dependency.Notification;

/// <summary>
/// Implement <see cref="INotificationHandlers{T}"/> and fetch from the DI the <see cref="INotificationHandler{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NotificationHandlers<T> : INotificationHandlers<T>
{
    public NotificationHandlers(IServiceProvider serviceProvider)
    {
        Handlers = serviceProvider.GetServices<INotificationHandler<T>>();
    }

    public IEnumerable<INotificationHandler<T>> Handlers { get; }
}
