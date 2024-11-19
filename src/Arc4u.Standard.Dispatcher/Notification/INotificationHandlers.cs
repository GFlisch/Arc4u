namespace Arc4u.Dispatcher.Notification;

/// <summary>
/// Interface to return the registered <see cref="INotificationHandler{T}"/> in the DI.
/// </summary>
/// <typeparam name="T">The type of the object.</typeparam>
public interface INotificationHandlers<T>
{
    IEnumerable<INotificationHandler<T>> Handlers { get; }
}

public interface INotificationHandlers<T1, T2>
{
    IEnumerable<INotificationHandler<T1, T2>> Handlers { get; }
}

public interface INotificationHandlers<T1, T2, T3>
{
    IEnumerable<INotificationHandler<T1, T2, T3>> Handlers { get; }
}

public interface INotificationHandlers<T1, T2, T3, T4>
{
    IEnumerable<INotificationHandler<T1, T2, T3, T4>> Handlers { get; }
}

public interface INotificationHandlers<T1, T2, T3, T4, T5>
{
    IEnumerable<INotificationHandler<T1, T2, T3, T4, T5>> Handlers { get; }
}
