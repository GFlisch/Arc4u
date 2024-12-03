namespace Arc4u.Dispatcher.Notification;

public static class PublishersExtension
{
    /// <summary>
    /// Call each <see cref="INotificationHandler{T}"/> in parallel.
    /// </summary>
    /// <typeparam name="T">The type of the object notified</typeparam>
    /// <param name="notifier">The handler</param>
    /// <param name="param">The instance od the parameter.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task PublishWhenAllAsync<T>(this INotificationHandlers<T> notifier, T param, CancellationToken cancellationToken)
    {
        await Task.WhenAll(notifier.Handlers.Select(handler => handler.HandleAsync(param, cancellationToken))
                                   .ToArray()).ConfigureAwait(false);
    }

    public static async Task PublishWhenAllAsync<T1, T2>(this INotificationHandlers<T1, T2> notifier, T1 param1, T2 param2, CancellationToken cancellationToken)
    {
        await Task.WhenAll(notifier.Handlers.Select(handler => handler.HandleAsync(param1, param2, cancellationToken))
                                   .ToArray()).ConfigureAwait(false);
    }

    public static async Task PublishWhenAllAsync<T1, T2, T3>(this INotificationHandlers<T1, T2, T3> notifier, T1 param1, T2 param2, T3 param3, CancellationToken cancellationToken)
    {
        await Task.WhenAll(notifier.Handlers.Select(handler => handler.HandleAsync(param1, param2, param3, cancellationToken))
                                   .ToArray()).ConfigureAwait(false);
    }

    public static async Task PublishWhenAllAsync<T1, T2, T3, T4>(this INotificationHandlers<T1, T2, T3, T4> notifier, T1 param1, T2 param2, T3 param3, T4 param4, CancellationToken cancellationToken)
    {
        await Task.WhenAll(notifier.Handlers.Select(handler => handler.HandleAsync(param1, param2, param3, param4, cancellationToken))
                                   .ToArray()).ConfigureAwait(false);
    }

    public static async Task PublishWhenAllAsync<T1, T2, T3, T4, T5>(this INotificationHandlers<T1, T2, T3, T4, T5> notifier, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, CancellationToken cancellationToken)
    {
        await Task.WhenAll(notifier.Handlers.Select(handler => handler.HandleAsync(param1, param2, param3, param4, param5, cancellationToken))
                                   .ToArray()).ConfigureAwait(false);
    }

    public static async Task PublishForEachAsync<T>(this INotificationHandlers<T> notifier, T param, CancellationToken cancellationToken)
    {
        foreach (var handler in notifier.Handlers)
        {
            await handler.HandleAsync(param, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task PublishForEachAsync<T1, T2>(this INotificationHandlers<T1, T2> notifier, T1 param1, T2 param2, CancellationToken cancellationToken)
    {
        foreach (var handler in notifier.Handlers)
        {
            await handler.HandleAsync(param1, param2, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task PublishForEachAsync<T1, T2, T3>(this INotificationHandlers<T1, T2, T3> notifier, T1 param1, T2 param2, T3 param3, CancellationToken cancellationToken)
    {
        foreach (var handler in notifier.Handlers)
        {
            await handler.HandleAsync(param1, param2, param3, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task PublishForEachAsync<T1, T2, T3, T4>(this INotificationHandlers<T1, T2, T3, T4> notifier, T1 param1, T2 param2, T3 param3, T4 param4, CancellationToken cancellationToken)
    {
        foreach (var handler in notifier.Handlers)
        {
            await handler.HandleAsync(param1, param2, param3, param4, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async Task PublishForEachAsync<T1, T2, T3, T4, T5>(this INotificationHandlers<T1, T2, T3, T4, T5> notifier, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, CancellationToken cancellationToken)
    {
        foreach (var handler in notifier.Handlers)
        {
            await handler.HandleAsync(param1, param2, param3, param4, param5, cancellationToken).ConfigureAwait(false);
        }
    }
}
