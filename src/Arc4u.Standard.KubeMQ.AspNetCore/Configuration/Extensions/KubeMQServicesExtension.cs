using Arc4u.KubeMQ;
using Arc4u.KubeMQ.AspNetCore;
using Arc4u.KubeMQ.AspNetCore.PubSub;
using Arc4u.KubeMQ.AspNetCore.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KubeMQServicesExtension
    {
        public static KubeMQConfigurationExtension AddKubeMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddScoped<HandlerContext>();
            services.TryAddSingleton<IMessageHandlerTypes, DefaultMessageHandlerTypes>();
            services.TryAddSingleton<IUniqueness, DefaultUniqueness>();
            services.TryAddScoped<IQueueMessageSender, DefaultQueueMessageSender>();
            services.TryAddSingleton<IPublisher, DefaultChannelPublisher>();
            services.TryAddSingleton<IQueueStreamManager, DefaultQueueStreamsManager>();
            services.TryAddScoped<MessagesToPublish>();
            services.TryAddScoped<MessagesScope>();

            return new KubeMQConfigurationExtension(services, configuration);
        }
    }
}
