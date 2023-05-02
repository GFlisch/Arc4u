using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Arc4u.Dependency.Notification;

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

    public static async Task PublishForEachAsync<T>(this INotificationHandlers<T> notifier, T param, CancellationToken cancellationToken)
    {
        foreach (var handler in notifier.Handlers)
        {
            await handler.HandleAsync(param, cancellationToken).ConfigureAwait(false);
        }
    }
}
