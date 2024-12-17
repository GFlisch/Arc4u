using Arc4u.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
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

    public HandleMessageBase(IServiceProvider container)
    {
        _container = container;
    }

    private readonly IServiceProvider _container;
    /// <summary>
    /// Method that will be used by NServiceBus to process a message.
    /// The real work is implemented in the Handle(T message) abstract method.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Handle(T message, IMessageHandlerContext context)
    {
        var messages = _container.GetService<MessagesToPublish>();
        var logger = _container.GetService<ILogger<HandleMessageBase<T>>>();
        try
        {

            // business work to implement.
            await Handle(message).ConfigureAwait(false);

            var messagesNotProcessed = new MessagesToPublish();

            // Publish events.
            foreach (var _event in messages?.Events ?? [])
            {
                try
                {
                    await context.Publish(_event).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger?.Technical().LogException(ex);
                    messagesNotProcessed.Add(_event);
                }
            }

            // Send commands.
            foreach (var command in messages?.Commands ?? [])
            {
                try
                {
                    await context.Send(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger?.Technical().LogException(ex);
                    messagesNotProcessed.Add(command);
                }
            }

            if (messagesNotProcessed.Events.Any() || messagesNotProcessed.Commands.Any())
            {
                await handleMessagesNotProcessedAsync(messagesNotProcessed).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger?.Technical().LogException(ex);
            throw;
        }
        finally
        {
            // Clear messages to avoid any sending of undesirable messages by
            // another message received on the bus.
            messages?.Clear();
        }
    }

    /// <summary>
    /// Method to implement to process the business.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public abstract Task Handle(T message);

    public virtual Task handleMessagesNotProcessedAsync(MessagesToPublish notProcessedMessages)
    {
        notProcessedMessages.Clear();
        return Task.CompletedTask;
    }
}
