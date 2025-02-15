using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.NServiceBus.Core;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Arc4u.NServiceBus;

[Export(typeof(IMessageScope)), Scoped]
public class MessageScope : IMessageScope
{
    public MessageScope(ILogger<MessageScope> logger, IMessageSession messageSession, MessagesToPublish messages)
    {
        _logger = logger;
        _messageSession = messageSession;
        _messages = messages;
    }

    private readonly ILogger<MessageScope> _logger;
    private readonly IMessageSession _messageSession;
    private readonly MessagesToPublish _messages;
    public void Complete()
    {

        //await _messageSession.Send(command, sendOptions);
        foreach (var _event in _messages.Events)
        {
            try
            {
                _logger.Technical().LogPublishEvent(_event.GetType()?.FullName ?? "No event type");
                _messageSession.Publish(_event).Wait();
            }
            catch (Exception ex)
            {
                _logger.Technical().LogException(ex);
            }
        }

        // Send commands.
        foreach (var command in _messages.Commands)
        {
            try
            {
                _logger.Technical().LogSendCommand(command.GetType().FullName ?? "No command name");
                _messageSession.Send(command).Wait();
            }
            catch (Exception ex)
            {
                _logger.Technical().LogException(ex);
            }
        }

        _messages.Clear();
    }
}
