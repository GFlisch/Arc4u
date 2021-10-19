using System;
using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    /// <summary>
    /// The class will be registered as scoped!
    /// Will store the messages and will be used by the sender to publish on a PubSub or send on a queue.
    /// </summary>
    public class MessagesToPublish
    {
        private readonly List<Object> _messages;

        public MessagesToPublish()
        {
            _messages = new List<Object>();
        }

        public void Add(MessageObject message)
        {
            if (null == message)
                throw new ArgumentNullException(nameof(message));

            _messages.Add(message);
        }

        public void Clear()
        {
            _messages.Clear();
        }

        public IReadOnlyList<Object> Get => _messages.AsReadOnly();
    }
}
