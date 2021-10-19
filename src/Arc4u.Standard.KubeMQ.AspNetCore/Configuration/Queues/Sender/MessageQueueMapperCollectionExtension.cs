using Arc4u.KubeMQ.AspNetCore.Configuration;
using Arc4u.KubeMQ.AspNetCore.Queues;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{

    public class MessageQueueMapperForSyntaxExtension
    {
        public MessageQueueMapperForSyntaxExtension(IServiceCollection services, IConfiguration configuration, QueueConfiguration queueConfiguration)
        {
            _services = services;
            _queueConfiguration = queueConfiguration;
            _configuration = configuration;
        }

        private readonly IServiceCollection _services;
        private readonly QueueConfiguration _queueConfiguration;
        private readonly IConfiguration _configuration;

        public MessageQueueMapperCollectionExtension Map
        {
            get { return new MessageQueueMapperCollectionExtension(_services, _configuration, _queueConfiguration); }
        }
    }

    public class MessageQueueMapperCollectionExtension
    {
        public MessageQueueMapperCollectionExtension(IServiceCollection services, IConfiguration configuration, QueueConfiguration queueConfiguration)
        {
            _services = services;
            _queueConfiguration = queueConfiguration;
            _configuration = configuration;
        }

        private readonly IServiceCollection _services;
        private readonly QueueConfiguration _queueConfiguration;
        private readonly IConfiguration _configuration;

        public (MessageQueueMapperCollectionExtension Map, KubeMQConfigurationExtension KubeMQ) With<TMessage>()
        {
            _services.Configure<QueueDefinition>(typeof(TMessage).FullName, options =>
            {
                options.Namespace = _queueConfiguration.Namespace;
                options.MessageType = typeof(TMessage);
                options.Serializer = _queueConfiguration.Serializer;
                options.Address = _queueConfiguration.Address;
                options.Identifier = _queueConfiguration.Identifier;
                options.Persisted = _queueConfiguration is PersistedQueueConfiguration;
                options.DeadLetterQueueName = _queueConfiguration.DeadLetterQueue?.DeadLetterQueueName;
                options.MaxRetryBeforeSendingToDeadLetterQueue = null == _queueConfiguration.DeadLetterQueue ? -1 : (int)_queueConfiguration.DeadLetterQueue.MaxRetry;
                options.RetryPolicy = _queueConfiguration.RetryPolicy;
            });

            return (this, new KubeMQConfigurationExtension(_services, _configuration));
        }
    }
}
