namespace Arc4u.KubeMQ.AspNetCore.Configuration
{
    /// <summary>
    /// Channel is the abstract concept that will support the Queuing and PubSub patterns in KubeMQ.
    /// </summary>
    public abstract class ChannelParameter
    {
        /// <summary>
        /// Identify the PubSub or Queue uniquely in the system.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Identify the sender and the receiver => typically the name of the service.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Address used to connect KubeMQ.
        /// </summary>
        public string Address { get; set; }


    }
}
