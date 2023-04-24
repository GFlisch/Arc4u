using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dependency.Notification;
public class NotificationHandlers<T> : INotificationHandlers<T>
{
    public NotificationHandlers(IServiceProvider serviceProvider)
    {
        Handlers = serviceProvider.GetServices<INotificationHandler<T>>();
    }

    public IEnumerable<INotificationHandler<T>> Handlers { get; }
}
