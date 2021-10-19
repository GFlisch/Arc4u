using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    public class EventMessage : QueueMessage
    {
        public EventMessage(object @event, Dictionary<string, string> tags = null, uint? sendAfterSeconds = null, uint? expireAfterSeconds = null) : base(@event, tags, sendAfterSeconds, expireAfterSeconds)
        {
        }
    }
}
