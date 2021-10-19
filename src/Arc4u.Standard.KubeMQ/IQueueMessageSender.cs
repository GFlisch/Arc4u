using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    /// <summary>
    /// Used to send a command to a queue.
    /// Pub Sub is used for events. Not commands.
    /// </summary>
    public interface IQueueMessageSender
    {
        /// <summary>
        /// Send one command message.
        /// </summary>
        /// <typeparam name="TMessage">The command or event message to send.</typeparam>
        /// <param name="message"></param>
        void Send<TMessage>(TMessage message, Dictionary<string, string> tags = null, uint? sendAfterSeconds = null, uint? expireAfterSeconds = null) where TMessage : notnull;

        /// <summary>
        /// Send commands in a batch mode.
        /// </summary>
        /// <typeparam name="TMessage">the command or event message type to send.</typeparam>
        /// <param name="messages"></param>
        void SendBatch(IEnumerable<QueueMessage> messages);

        void SendBatch(IEnumerable<QueueMessage> messages, int splitByQueues);

    }




}
