using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Arc4u.KubeMQ
{
    public class MessagesScope : IDisposable
    {
        public MessagesScope(MessagesToPublish messagesToPublish, ILogger<MessagesScope> logger, IQueueMessageSender queueMessageSender, IPublisher publisher, IActivitySourceFactory activitySourceFactory)
        {
            _messagesToPublish = messagesToPublish;
            _logger = logger;
            _queueMessageSender = queueMessageSender;
            _publisher = publisher;
            _activitySource = activitySourceFactory.GetArc4u();
        }

        private readonly MessagesToPublish _messagesToPublish;
        private readonly ILogger<MessagesScope> _logger;
        private readonly IQueueMessageSender _queueMessageSender;
        private readonly IPublisher _publisher;
        private readonly ActivitySource _activitySource;

        public async Task CompleteAsync()
        {
            var messages = QueueMessages;
            var pubSubEvents = PubSubEvents;

            if (messages.Count > 0 || pubSubEvents.Count > 0)
            {
                using (var activity = _activitySource?.StartActivity("Send to KubeMQ"))
                {

                    if (messages.Count > 0)
                    {
                        activity?.AddTag("QueueMessages", messages.Count);
                        _logger.Technical().System($"{messages.Count} messages will be sent to queues.").Log();
                        _queueMessageSender.SendBatch(messages);
                    }



                    if (pubSubEvents.Count > 0)
                    {
                        activity?.AddTag("PubSubMessages", pubSubEvents.Count);
                        _logger.Technical().System($"{pubSubEvents.Count} event(s) will be sent to publishers.").Log();
                        await _publisher.PublishBatchAsync(pubSubEvents);
                    }

                    // Clear because messages have been processed.
                    _messagesToPublish.Clear();
                }
            }
        }

        private IList<PubSubEvent> PubSubEvents => _messagesToPublish.Get.OfType<PubSubEvent>().ToList();

        private IList<QueueMessage> QueueMessages => _messagesToPublish.Get.OfType<QueueMessage>().ToList();


        public void Dispose()
        {
            _messagesToPublish.Clear();
        }
    }
}
