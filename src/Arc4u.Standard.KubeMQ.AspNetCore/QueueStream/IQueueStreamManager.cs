using Arc4u.KubeMQ.AspNetCore.Configuration;
using KubeMQ.SDK.csharp.QueueStream;

namespace Arc4u.KubeMQ.AspNetCore
{
    public interface IQueueStreamManager
    {
        QueueStream Get(ChannelParameter channel);

        /// <summary>
        /// Close and remove the queue from the internal collection.
        /// </summary>
        /// <param name="channel">Identify the channel.</param>
        /// <returns>true is removed and false if was not in the collection</returns>
        bool Close(ChannelParameter channel);
    }
}
