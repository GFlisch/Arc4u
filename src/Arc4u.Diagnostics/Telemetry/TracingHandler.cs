using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Arc4u.Diagnostics.Telemetry;
public sealed class TracingHandler(DefaultActivitySourceFactory defaultActivitySourceFactory, ILogger<TracingHandler> logger) : DelegatingHandler
{
    private readonly ActivitySource _activitySource = defaultActivitySourceFactory.GetArc4u();
    private readonly ILogger<TracingHandler> _logger = logger;
    private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var activity = Activity.Current;
        bool shouldStopActivity = false;

        try
        {
            if (activity is null)
            {
                _logger.Technical().Warning("No activity found! Creating default one with {name} as name. Tracing a correlationId will not be possible.", request.RequestUri!.ToString());
                activity = _activitySource.StartActivity(request.RequestUri!.ToString());
                shouldStopActivity = true;
            }

            var correlationId = activity?.GetCorrelationId();
            if (correlationId is null)
            {
                activity = activity?.WithCorrelationId();
                correlationId = activity?.GetCorrelationId();
            }

            if (activity is not null)
            {
                Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current),
                    request.Headers, (headers, name, value) => headers.Add(name, value));
            }

            if (request.Headers.Contains(CorrelationIdActityExtensions.CorrelationIdKey))
            {
                request.Headers.Remove(CorrelationIdActityExtensions.CorrelationIdKey);
            }

            request.Headers.Add(CorrelationIdActityExtensions.CorrelationIdKey, correlationId);


            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if(activity is not null)
            {
                await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                var ctx =  Propagator.Extract(default, response, (response, name) =>
                    response.Headers.Contains(name)? response.Headers.GetValues(name) :
                    response.TrailingHeaders.Contains(name) ? response.TrailingHeaders.GetValues(name): []);
                Baggage.SetBaggage(ctx.Baggage.GetBaggage());
            }

            return response;
        }
        finally
        {
            if (shouldStopActivity)
            {
                activity?.Stop();
            }
        }
    }
}

public static class CorrelationIdActityExtensions
{
    public const string CorrelationIdKey = "CorrelationId";

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

        return activity.SetTag(CorrelationIdKey, correlationId);
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
            var correlationId = (string?)currentActivity.GetTagItem(CorrelationIdKey);

            if (correlationId != null)
            {
                return correlationId;
            }

            currentActivity = currentActivity.Parent;
        }

        return null;
    }

}
