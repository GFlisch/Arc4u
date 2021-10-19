using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    public class Command : QueueMessage
    {
        public Command(object command, Dictionary<string, string> tags = null, uint? sendAfterSeconds = null, uint? expireAfterSeconds = null) : base(command, tags, sendAfterSeconds, expireAfterSeconds)
        {
        }
    }
}
