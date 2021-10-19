using Polly;
using Polly.Retry;
using System;

namespace Arc4u.KubeMQ.AspNetCore.Configuration
{
    public class SubscriberParameters : ChannelParameter
    {
        public string GroupName { get; set; }

        public Func<PolicyBuilder, RetryPolicy> RetryPolicy = policyBuilder =>
        {
            return policyBuilder.WaitAndRetryForever(_ => TimeSpan.FromSeconds(2));
        };
    }
}
