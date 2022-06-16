using Arc4u.KubeMQ.AspNetCore.Configuration;
using KubeMQ.SDK.csharp.QueueStream;
using System.Collections.Concurrent;

namespace Arc4u.KubeMQ.AspNetCore
{
    public class DefaultQueueStreamsManager : IQueueStreamManager
    {

        public DefaultQueueStreamsManager()
        {
            Queues = new ConcurrentDictionary<string, QueueStream>();
        }

        private readonly ConcurrentDictionary<string, QueueStream> Queues;
        private readonly object lockObj = new object();
        
        public QueueStream Get(ChannelParameter channel)
        {
            var key = BuildKey(channel);
            if (Queues.TryGetValue(key, out var queue))
                return queue;

            // The object must be instantiated in a Singleton mode.
            lock (lockObj)
            {
                return Queues.GetOrAdd(key, new QueueStream(channel.Address, channel.Identifier, null));
            }
        }

        private static string BuildKey(ChannelParameter channel)
        {
            return $"{channel.Address}_{channel.Identifier}".ToLowerInvariant();
        }

        public bool Close(ChannelParameter channel)
        {
            var key = BuildKey(channel);
            if (Queues.TryRemove(key, out var queue))
            {
                if (queue.Connected)
                    queue.Close();

                return true;
            }

            return false;
        }
    }
}
