using Polly;
using Polly.Retry;
using System;

namespace Arc4u.KubeMQ.AspNetCore.Configuration
{


    /// <summary>
    /// Define the parameters to listen on a Queue.
    /// </summary>
    public abstract class QueueParameters : ChannelParameter
    {
        public ushort WaitTimeMilliSecondsQueueMessages { get; set; } = 1000;

        public ushort MaxNumberOfMessagesRead { get; set; } = 100;

        /// <summary>
        /// 2 <see cref="QueueParameters"/> with the same Name and address are considered as equal.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return  HashCode.Combine(Namespace,Address, WaitTimeMilliSecondsQueueMessages, MaxNumberOfMessagesRead);
        }

        public override bool Equals(object obj)
        {
            if (null == obj) return false;

            if (obj is QueueParameters parameters)
            {
                return parameters.Namespace.Equals(Namespace) && parameters.Address.Equals(Address) 
                    && parameters.WaitTimeMilliSecondsQueueMessages == WaitTimeMilliSecondsQueueMessages && parameters.MaxNumberOfMessagesRead == MaxNumberOfMessagesRead;
            }

            return false;
        }

        public Func<PolicyBuilder, RetryPolicy> RetryPolicy = policyBuilder =>
        {
            return policyBuilder.WaitAndRetryForever(_ => TimeSpan.FromSeconds(2));
        };
    }
}
