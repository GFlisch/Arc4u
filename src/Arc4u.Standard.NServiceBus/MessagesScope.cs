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

    public MessagesScope(IContainerResolve container, ILogger logger, string iocResolveName)
    {
        Logger = logger;
        // Create the unit of work list of messages.
        MessagesToPublish.Create();

        // Search for the instance used to send commands and publish events on start of the
        // scope. So if we have a resolving issue, we know it immediately (and not after the work is done.
        if (!container.TryResolve<IEndpointConfiguration>(iocResolveName, out var endpointConfig))
        {
            logger.Technical().From<MessagesScope>().Warning($"Unable to resolve the IEndpointConfiguration with the name '{iocResolveName}'").Log();
            return;
        }

        if (null == endpointConfig!.Instance)
        {
            logger.Technical().From<MessagesScope>().Warning($"Instance is null for the IEndpointConfiguration with the name '{iocResolveName}'").Log();
            return;
        }

        _instance = endpointConfig.Instance;

    }

    public void Complete()
    {
        if (null == _instance)
        {
            Logger.Technical().From<MessagesScope>().Warning($"Unable to send any events or commands to the IEndpointConfiguration with the name.").Log();
            return;
        }

        // Publish events.
        foreach (object _event in MessagesToPublish.Events)
        {
            try
            {
                Logger.Technical().From<MessagesScope>().System($"Publish event: {_event.GetType().FullName}.").Log();
                _instance.Publish(_event).Wait();
            }
            catch (Exception ex)
            {
                Logger.Technical().From<MessagesScope>().Exception(ex).Log();
            }
        }

        // Send commands.
        foreach (Object command in MessagesToPublish.Commands)
        {
            try
            {
                Logger.Technical().From<MessagesScope>().System($"Send command: {command.GetType().FullName}.").Log();
                _instance.Send(command).Wait();
            }
            catch (Exception ex)
            {
                Logger.Technical().From<MessagesScope>().Exception(ex).Log();
            }
        }

        MessagesToPublish.Clear();
    }

    private readonly IEndpointInstance? _instance;
    public void Dispose()
    {

    }
}
