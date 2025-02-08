using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Arc4u.NServiceBus;

/// <summary>
/// Create a unit of work instance where Messages are at the end of the scope
/// sent to the instance of a NServiceBus sender.
/// </summary>
public class MessagesScope : IDisposable
{
    private readonly ILogger Logger;

    public MessagesScope(IServiceProvider container, ILogger logger, string iocResolveName)
    {
        Logger = logger;
        // Create the unit of work list of messages.
        MessagesToPublish.Create();

        // Search for the instance used to send commands and publish events on start of the
        // scope. So if we have a resolving issue, we know it immediately (and not after the work is done.
        if (!container.TryGetService<IEndpointConfiguration>(iocResolveName, out var endpointConfig))
        {
            logger.Technical<MessagesScope>().LogWarning($"Unable to resolve the IEndpointConfiguration with the name '{iocResolveName}'");
            return;
        }

        if (null == endpointConfig!.Instance)
        {
            logger.Technical<MessagesScope>().LogWarning($"Instance is null for the IEndpointConfiguration with the name '{iocResolveName}'");
            return;
        }

        _instance = endpointConfig.Instance;

    }

    public void Complete()
    {
        if (null == _instance)
        {
            Logger.Technical<MessagesScope>().LogWarning($"Unable to send any events or commands to the IEndpointConfiguration with the name.");
            return;
        }

        // Publish events.
        foreach (var _event in MessagesToPublish.Events)
        {
            try
            {
                Logger.Technical<MessagesScope>().LogTrace($"Publish event: {_event.GetType().FullName}.");
                _instance.Publish(_event).Wait();
            }
            catch (Exception ex)
            {
                Logger.Technical<MessagesScope>().LogException(ex);
            }
        }

        // Send commands.
        foreach (var command in MessagesToPublish.Commands)
        {
            try
            {
                Logger.Technical<MessagesScope>().LogTrace($"Send command: {command.GetType().FullName}.");
                _instance.Send(command).Wait();
            }
            catch (Exception ex)
            {
                Logger.Technical<MessagesScope>().LogException(ex);
            }
        }

        MessagesToPublish.Clear();
    }

    private readonly IEndpointInstance? _instance;
    public void Dispose()
    {

    }
}
