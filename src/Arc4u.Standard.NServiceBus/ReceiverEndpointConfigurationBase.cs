using NServiceBus;
using System;
using System.Threading.Tasks;

namespace Arc4u.NServiceBus
{
    public abstract class ReceiverEndpointConfigurationBase : IEndpointConfiguration
    {
        private IEndpointInstance _endpointInstance;


        public IEndpointInstance Instance => _endpointInstance;

        public abstract Task ConfigureTransportAndRoutingAsync(EndpointConfiguration endpointConfiguration);

        public async Task StartAsync(string endpointName)
        {
            if (String.IsNullOrEmpty(endpointName))
                throw new ArgumentException("endpointName");

            var endpointConfiguration = new EndpointConfiguration(endpointName);

            // Cancel diagnostics by file. We use Logger.
            endpointConfiguration.CustomDiagnosticsWriter(
                diagnostics =>
                {
                    return Task.CompletedTask;
                });

            await ConfigureTransportAndRoutingAsync(endpointConfiguration);

            _endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            if (null != _endpointInstance)
                await _endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}
