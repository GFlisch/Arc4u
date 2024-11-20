using NServiceBus;

namespace Arc4u.NServiceBus
{
    /// <summary>
    /// Define the base code to create a sender.
    /// We only need one sender by application and define routes by types to send a command or event.
    /// </summary>
    public abstract class SenderEndpointConfigBase : IEndpointConfiguration
    {
        private IEndpointInstance _endpointInstance;

        /// <summary>
        /// Return the instance allowing to send a <see cref="ICommand"/> or
        /// publish an <see cref="IEvent"/>.
        /// </summary>
        public IEndpointInstance Instance => _endpointInstance;

        /// <summary>
        /// Abstract method to implement to configure the endpoint regarding the
        /// transport layer (ServiceBus, RabbitMQ, Sql, etc...)
        /// </summary>
        /// <param name="endpointConfiguration">The NServiceBus endpoint to configure, <see cref="EndpointConfiguration"/>.</param>
        /// <returns></returns>
        public abstract Task ConfigureTransportAndRoutingAsync(EndpointConfiguration endpointConfiguration);

        /// <summary>
        /// Connect and configure to the queueing system.
        /// </summary>
        /// <param name="applicationName">The name of the application. This is sent in the headers of the message.</param>
        public async Task StartAsync(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentException("applicationName");
            }

            var endpointConfiguration = new EndpointConfiguration(applicationName);
            // for to be a sender only!
            endpointConfiguration.SendOnly();

            // Cancel diagnostics by file defined by default with NServiceBus. We use Logger.
            endpointConfiguration.CustomDiagnosticsWriter(
                diagnostics =>
                {
                    return Task.CompletedTask;
                });

            await ConfigureTransportAndRoutingAsync(endpointConfiguration);

            _endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

        }

        /// <summary>
        /// Stop the connection to the queueing system.
        /// </summary>
        public async Task StopAsync()
        {
            if (null != _endpointInstance)
            {
                await _endpointInstance.Stop().ConfigureAwait(false);
            }
        }
    }
}
