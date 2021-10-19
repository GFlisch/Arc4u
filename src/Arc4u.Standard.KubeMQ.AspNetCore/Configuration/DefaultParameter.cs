namespace Arc4u.KubeMQ.AspNetCore.Configuration
{
    public class DefaultParameter
    {
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
