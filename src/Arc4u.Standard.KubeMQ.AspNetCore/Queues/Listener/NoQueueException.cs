using System;

namespace Arc4u.KubeMQ.AspNetCore.Queues
{
    public class NoQueueException : Exception
    {
        public NoQueueException()
        {

        }

        public NoQueueException(string queueName) : base($"No queue {queueName} is found.")
        {

        }
    }
}
