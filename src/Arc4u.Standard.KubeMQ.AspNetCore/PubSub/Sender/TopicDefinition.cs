using Arc4u.KubeMQ.AspNetCore.Configuration;
using System;

namespace Arc4u.KubeMQ.AspNetCore.PubSub
{
    public class PubSubDefinition : ChannelParameter
    {
        /// <summary>
        /// The type of the event mapped to this publisher
        /// </summary>
        public Type EventType { get; set; }

        /// <summary>
        /// The serializer to use.
        /// </summary>
        public string Serializer { get; set; }

        /// <summary>
        /// Define if the message is stored or not in KubeMQ.
        /// </summary>
        public bool Persisted { get; set; }

        public bool IsValid()
        {
            return !String.IsNullOrWhiteSpace(Namespace) && !String.IsNullOrWhiteSpace(Address) && null != EventType;
        }
    }
}
