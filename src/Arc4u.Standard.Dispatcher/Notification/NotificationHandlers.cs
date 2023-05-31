using System;
using System.Collections.Generic;
using Arc4u.Dispatcher.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dispatcher.Notification;

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

public class NotificationHandlers<T1, T2> : INotificationHandlers<T1, T2>
{
    public NotificationHandlers(IServiceProvider serviceProvider)
    {
        Handlers = serviceProvider.GetServices<INotificationHandler<T1, T2>>();
    }

    public IEnumerable<INotificationHandler<T1, T2>> Handlers { get; }
}

public class NotificationHandlers<T1, T2, T3> : INotificationHandlers<T1, T2, T3>
{
    public NotificationHandlers(IServiceProvider serviceProvider)
    {
        Handlers = serviceProvider.GetServices<INotificationHandler<T1, T2, T3>>();
    }

    public IEnumerable<INotificationHandler<T1, T2, T3>> Handlers { get; }
}

public class NotificationHandlers<T1, T2, T3, T4> : INotificationHandlers<T1, T2, T3, T4>
{
    public NotificationHandlers(IServiceProvider serviceProvider)
    {
        Handlers = serviceProvider.GetServices<INotificationHandler<T1, T2, T3, T4>>();
    }

    public IEnumerable<INotificationHandler<T1, T2, T3, T4>> Handlers { get; }
}

public class NotificationHandlers<T1, T2, T3, T4, T5> : INotificationHandlers<T1, T2, T3, T4, T5>
{
    public NotificationHandlers(IServiceProvider serviceProvider)
    {
        Handlers = serviceProvider.GetServices<INotificationHandler<T1, T2, T3, T4, T5>>();
    }

    public IEnumerable<INotificationHandler<T1, T2, T3, T4, T5>> Handlers { get; }
}
