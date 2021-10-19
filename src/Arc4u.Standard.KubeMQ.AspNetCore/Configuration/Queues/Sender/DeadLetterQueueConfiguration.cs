namespace Arc4u.KubeMQ.AspNetCore.Configuration
{
    public class DeadLetterQueueConfiguration
    {
        public uint MaxRetry { get; set; }

        public string DeadLetterQueueName { get; set; }
    }
}
