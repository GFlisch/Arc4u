using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;

namespace Arc4u.NServiceBus
{
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
            foreach (Object _event in _messages.Events)
            {
                try
                {
                    _logger.Technical().Information($"Publish event: {_event.GetType().FullName}.").Log();
                    _messageSession.Publish(_event).Wait();
                }
                catch (Exception ex)
                {
                    _logger.Technical().Exception(ex).Log();
                }
            }

            // Send commands.
            foreach (Object command in _messages.Commands)
            {
                try
                {
                    _logger.Technical().Information($"Send command: {command.GetType().FullName}.").Log();
                    _messageSession.Send(command).Wait();
                }
                catch (Exception ex)
                {
                    _logger.Technical().Exception(ex).Log();
                }
            }

            _messages.Clear();
        }
    }
}
