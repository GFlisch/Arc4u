using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    public abstract class QueueMessage : MessageObject
    {
        public QueueMessage(object message, Dictionary<string, string> tags = null, uint? sendAfterSeconds = null, uint? expireAfterSeconds = null) : base(message, tags)
        {
            _expireAfterSeconds = expireAfterSeconds;
            _sendAfterSeconds = sendAfterSeconds;
        }

        private readonly uint? _sendAfterSeconds = null;
        private readonly uint? _expireAfterSeconds = null;

        public uint? SendAfterSeconds => _sendAfterSeconds;

        public uint? ExpireAfterSeconds => _expireAfterSeconds;

    }
}
