using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Arc4u.NServiceBus;

/// <summary>
/// The base class will wrap the handle of a NServiceBus message by
/// adding the unit of work behavior to add events and commands during the
/// process of a message. See <see cref="MessagesToPublish"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class HandleMessageBase<T> : IHandleMessages<T>
{
    public HandleMessageBase(ILogger<HandleMessageBase<T>>? logger = null)
    {
        _logger = logger;
    }

    private readonly ILogger<HandleMessageBase<T>>? _logger;
    /// <summary>
    /// Method that will be used by NServiceBus to process a message.
    /// The real work is implemented in the Handle(T message) abstract method.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Handle(T message, IMessageHandlerContext context)
    {
        try
        {
            // Create the unit of work list of messages.
            MessagesToPublish.Create();

            // business work to implement.
            await Handle(message).ConfigureAwait(false);

            // Publish events.
            foreach (object _event in MessagesToPublish.Events)
            {
                try
                {
                    await context.Publish(_event).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.Technical().LogException(ex);
                }
            }

            // Send commands.
            foreach (Object command in MessagesToPublish.Commands)
            {
                try
                {
                    await context.Send(command).ConfigureAwait(false);
                }
                catch (System.Exception ex)
                {
                    _logger?.Technical().LogException(ex);
                }
            }

        }
        catch (Exception ex)
        {
            _logger?.Technical().LogException(ex);
        }
        finally
        {
            // Clear messages to avoid any sending of undesirable messages by
            // another message received on the bus.
            MessagesToPublish.Clear();
        }
    }

    /// <summary>
    /// Method to implement to process the business.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public abstract Task Handle(T message);
}
