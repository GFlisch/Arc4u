using Arc4u.KubeMQ.AspNetCore.Configuration;
using Arc4u.KubeMQ.AspNetCore.Handler;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{

    public class HandlerPubSubForSyntax
    {
        public HandlerPubSubForSyntax(IServiceCollection services, IConfiguration configuration, SubscriberParameters subscriberParameters)
        {
            _services = services;
            _subscriberParameters = subscriberParameters;
            _configuration = configuration;

        }

        private readonly IServiceCollection _services;
        private readonly SubscriberParameters _subscriberParameters;
        private readonly IConfiguration _configuration;

        public HandlerPubSubMapperCollectionExtension Handlers { get => new HandlerPubSubMapperCollectionExtension(_services, _configuration, _subscriberParameters); }
    }

    public class HandlerPubSubMapperCollectionExtension
    {
        public HandlerPubSubMapperCollectionExtension(IServiceCollection services, IConfiguration configuration, SubscriberParameters subscriberParameters)
        {
            _services = services;
            _subscriberParameters = subscriberParameters;
            _configuration = configuration;

        }

        private readonly IServiceCollection _services;
        private readonly SubscriberParameters _subscriberParameters;
        private readonly IConfiguration _configuration;

        public (HandlerPubSubMapperCollectionExtension Handlers, KubeMQConfigurationExtension KubeMQ) Add<TMessage>(string handlerName, string serializer)
        {
            _services.Configure<HandlerDefintion>(handlerName + _subscriberParameters.Namespace, options =>
            {
                options.Name = _subscriberParameters.Namespace;
                options.HandlerType = typeof(TMessage);
                options.Serializer = serializer;
            });

            return (this, new KubeMQConfigurationExtension(_services, _configuration));
        }

        public (HandlerPubSubMapperCollectionExtension Handlers, KubeMQConfigurationExtension KubeMQ) Add<TMessage>()
        {
            return Add<TMessage>(typeof(TMessage).FullName, null);
        }

        public (HandlerPubSubMapperCollectionExtension Handlers, KubeMQConfigurationExtension KubeMQ) Add<TMessage>(string handlerName)
        {
            return Add<TMessage>(handlerName, null);
        }
    }
}

