using Arc4u.KubeMQ.AspNetCore.Configuration;
using Arc4u.KubeMQ.AspNetCore.Handler;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{

    public class HandlerQueueForSyntax
    {
        public HandlerQueueForSyntax(IServiceCollection services, IConfiguration configuration, QueueParameters queueParameters)
        {
            _services = services;
            _queueParameters = queueParameters;
            _configuration = configuration;

        }

        private readonly IServiceCollection _services;
        private readonly QueueParameters _queueParameters;
        private readonly IConfiguration _configuration;

        public HandlerQueueMapperCollectionExtension Handlers { get => new HandlerQueueMapperCollectionExtension(_services, _configuration, _queueParameters); }
    }

    public class HandlerQueueMapperCollectionExtension
    {
        public HandlerQueueMapperCollectionExtension(IServiceCollection services, IConfiguration configuration, QueueParameters queueParameters)
        {
            _services = services;
            _queueParameters = queueParameters;
            _configuration = configuration;

        }

        private readonly IServiceCollection _services;
        private readonly QueueParameters _queueParameters;
        private readonly IConfiguration _configuration;

        public (HandlerQueueMapperCollectionExtension AddOtherHandler, KubeMQConfigurationExtension KubeMQ) Add<TMessage>(string handlerName, string serializer)
        {
            _services.Configure<HandlerDefintion>(handlerName + _queueParameters.Namespace, options =>
            {
                options.Name = _queueParameters.Namespace;
                options.HandlerType = typeof(TMessage);
                options.Serializer = serializer;
            });

            return (this, new KubeMQConfigurationExtension(_services, _configuration));
        }

        public (HandlerQueueMapperCollectionExtension Handlers, KubeMQConfigurationExtension KubeMQ) Add<TMessage>()
        {
            return Add<TMessage>(typeof(TMessage).FullName, null);
        }
    }
}
