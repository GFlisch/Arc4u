using Polly;
using Polly.Retry;
using System;

namespace Arc4u.KubeMQ.AspNetCore.Configuration
{
    // Used to configure the queue sending.
    public class QueueConfiguration : ChannelParameter
    {
        public Func<PolicyBuilder, RetryPolicy> RetryPolicy = policyBuilder =>
        {
            return policyBuilder.WaitAndRetry(3, _ => TimeSpan.FromSeconds(2));
        };

        public DeadLetterQueueConfiguration DeadLetterQueue { get; set; } = null;

        public string Serializer { get; set; }

    }
}
