using Arc4u.KubeMQ.AspNetCore.PubSub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.KubeMQ.AspNetCore.Configuration
{

    public class PubSubMapperForSyntaxExtension
    {
        public PubSubMapperForSyntaxExtension(IServiceCollection services, IConfiguration configuration, PubSubConfiguration pubSubConfiguration)
        {
            _services = services;
            _pubSubConfiguration = pubSubConfiguration;
            _configuration = configuration;
        }

        private readonly IServiceCollection _services;
        private readonly PubSubConfiguration _pubSubConfiguration;
        private readonly IConfiguration _configuration;

        public PubSubMapperCollectionExtension Map
        {
            get
            {
                return new PubSubMapperCollectionExtension(_services, _configuration, _pubSubConfiguration);
            }
        }

    }
    public class PubSubMapperCollectionExtension
    {
        public PubSubMapperCollectionExtension(IServiceCollection services, IConfiguration configuration, PubSubConfiguration pubSubConfiguration)
        {
            _services = services;
            _pubSubConfiguration = pubSubConfiguration;
            _configuration = configuration;
        }

        private readonly IServiceCollection _services;
        private readonly PubSubConfiguration _pubSubConfiguration;
        private readonly IConfiguration _configuration;

        public (PubSubMapperCollectionExtension Map, KubeMQConfigurationExtension KubeMQ) With<TCommand>(string serializer)
        {
            _services.Configure<PubSubDefinition>(typeof(TCommand).FullName, options =>
            {
                options.Namespace = _pubSubConfiguration.Namespace;
                options.EventType = typeof(TCommand);
                options.Serializer = serializer;
                options.Address = _pubSubConfiguration.Address;
                options.Identifier = _pubSubConfiguration.Identifier;
                options.Persisted = _pubSubConfiguration is PersistedPubSubConfiguration;
            });

            return (this, new KubeMQConfigurationExtension(_services, _configuration));
        }

        public (PubSubMapperCollectionExtension Map, KubeMQConfigurationExtension KubeMQ) With<TCommand>()
        {
            return With<TCommand>(null);
        }
    }
}
