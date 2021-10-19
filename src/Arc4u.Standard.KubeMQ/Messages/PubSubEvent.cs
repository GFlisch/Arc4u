using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    public class PubSubEvent : MessageObject
    {
        public PubSubEvent(object message, Dictionary<string, string> tags = null) : base(message, tags)
        {

        }
    }
}
