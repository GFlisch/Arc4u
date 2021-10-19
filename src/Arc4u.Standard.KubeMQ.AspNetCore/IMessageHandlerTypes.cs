using Arc4u.Serializer;
using System;

namespace Arc4u.KubeMQ.AspNetCore
{
    /// <summary>
    /// Define the capability to return the MessageHandler of the Message type.
    /// </summary>
    public interface IMessageHandlerTypes
    {
        /// <summary>
        /// Based on the string type of the message structure 
        /// </summary>
        /// <param name="qualifiedTypeName"></param>
        /// <returns></returns>
        Type GetOrAddType(string keyName, out Type handlerDataType);

        /// <summary>
        /// Return for the specific message, the serializer used. => Each message body can have its own serializer!
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        IObjectSerialization Serializer(string keyName);
    }
}
