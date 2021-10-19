using Arc4u.KubeMQ.AspNetCore.PubSub;
using Arc4u.KubeMQ.AspNetCore.Queues;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KubeMQDefaultHostedServicesExtensions
    {
        public static void AddKubeMQServiceHosts(this IServiceCollection services)
        {
            services.AddHostedService<QueueListenerHostedService>();
            services.AddHostedService<PubSubListenerHostedService>();
        }
    }
}
