namespace Arc4u.NServiceBus
{
    /// <summary>
    /// Define the contract to manage the configuration of a NServiceBus endpoint listener.
    /// </summary>
    public interface IEndpointConfiguration
    {
        // Start the listening on a queue.
        Task StartAsync();

    }
}
