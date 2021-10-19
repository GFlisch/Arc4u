using Arc4u.KubeMQ.AspNetCore.Configuration;
using Polly;
using Polly.Retry;
using System;

namespace Arc4u.KubeMQ.AspNetCore.Queues
{
    public class QueueDefinition : ChannelParameter
    {
        /// <summary>
        /// The type of the message (event or command) mapped to this queue
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        /// The serializer to use.
        /// </summary>
        public String Serializer { get; set; }

        /// <summary>
        /// Define if the message is stored or not in KubeMQ.
        /// </summary>
        public bool Persisted { get; set; }

        /// <summary>
        /// Define the Polly Retry policy to use!
        /// </summary>
        public Func<PolicyBuilder, RetryPolicy> RetryPolicy { get; set; }

        /// <summary>
        /// Define dead letter queue name
        /// </summary>
        public String DeadLetterQueueName { get; set; } = null;


        public int MaxRetryBeforeSendingToDeadLetterQueue { get; set; } = -1;

        public bool IsValid()
        {
            return !String.IsNullOrWhiteSpace(Namespace) && !String.IsNullOrWhiteSpace(Address) && null != MessageType;
        }

        public QueueDefinition Clone()
        {
            return new QueueDefinition
            {
                Namespace = this.Namespace,
                MessageType = this.MessageType,
                Serializer = this.Serializer,
                Address = this.Address,
                Persisted = this.Persisted,
                Identifier = this.Identifier
            };
        }
    }
}
