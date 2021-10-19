using NServiceBus;
using System.Threading.Tasks;

namespace Arc4u.NServiceBus
{
    /// <summary>
    /// Define the contract to manage the configuration of a NServiceBus endpoint listener.
    /// </summary>
    public interface IEndpointConfiguration
    {
        // Start the listening on a queue.
        Task StartAsync(string endpointName);

        // Stop listening.
        Task StopAsync();

        // Get instance to send or publish messages.
        IEndpointInstance Instance { get; }
    }
}
