using System.Diagnostics;

namespace Arc4u.Diagnostics.Telemetry;

public static class CorrelationIdActityExtensions
{
    /// <summary>
    /// Adds a new correlationId to the activity.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <returns></returns>
    public static Activity WithCorrelationId(this Activity activity)
        => activity.WithCorrelationId(Guid.NewGuid().ToString());

    /// <summary>
    /// Adds a new correlationId to the activity.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Activity WithCorrelationId(this Activity activity, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(correlationId);

        return activity.SetTag(CorrelationIdHttpHeaders.CorrelationId, correlationId);
    }

    /// <summary>
    /// Gets the correlation identifier associated with <see cref="Activity"/>
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <returns></returns>
    public static string? GetCorrelationId(this Activity activity)
    {
        var currentActivity = activity;

        while (currentActivity is not null)
        {
            var correlationId = (string?)currentActivity.GetTagItem(CorrelationIdHttpHeaders.CorrelationId);

            if (correlationId != null)
            {
                return correlationId;
            }

            currentActivity = currentActivity.Parent;
        }

        return null;
    }

}
