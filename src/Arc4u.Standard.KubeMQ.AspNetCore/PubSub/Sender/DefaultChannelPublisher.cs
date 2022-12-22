using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.KubeMQ.AspNetCore.Configuration;
using Arc4u.Security.Principal;
using Arc4u.Serializer;
using KubeMQ.SDK.csharp.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KubeMQEvent = KubeMQ.SDK.csharp.Events.Event;

namespace Arc4u.KubeMQ.AspNetCore.PubSub
{
    public class DefaultChannelPublisher : IPublisher
    {
        public DefaultChannelPublisher(IOptionsMonitor<PubSubDefinition> definitions, IServiceProvider serviceProvider, ILogger<DefaultChannelPublisher> logger, IApplicationContext applicationContext, IActivitySourceFactory activitySourceFactory)
        {
            _definitions = definitions;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _applicationContext = applicationContext;
            _activitySource = activitySourceFactory?.GetArc4u();
        }

        private readonly IOptionsMonitor<PubSubDefinition> _definitions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultChannelPublisher> _logger;
        private readonly IApplicationContext _applicationContext;
        private readonly ActivitySource _activitySource;

        public void Publish<TEvent>(TEvent @event, Dictionary<string, string> tags = null) where TEvent : notnull
        {
            using (var activity = _activitySource?.StartActivity("Send PubSub event to KubeMQ", ActivityKind.Producer))
            {
                if (typeof(TEvent) is IEnumerable)
                    throw new ArgumentException("Collection are not accepted!");

                var definition = GetDefiniton(typeof(TEvent));

                var channel = CreateChannel(definition);

                var serializer = GetSerializer(definition);

                string activityId = _applicationContext?.Principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString();

                KubeMQEvent message = null;
                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                    message = new KubeMQEvent(null, string.Empty, serializer.Serialize(@event).ToArray(), BuildTags(typeof(TEvent), tags, activityId));

                using (var sendActivity = _activitySource?.StartActivity("Send to KubeMQ", ActivityKind.Producer))
                    channel.SendEvent(message);
            }
        }

        public async Task PublishBatchAsync(IEnumerable<PubSubEvent> messages)
        {
            using (var activity = _activitySource?.StartActivity("Send PubSub events to KubeMQ", ActivityKind.Producer))
            {
                activity?.SetTag("Count", messages.Count());

                Dictionary<ChannelParameter, List<PubSubEvent>> channels = SplitByChannels(messages);

                foreach (var channel in channels)
                {
                    await PublishBatchAsync(channel.Key, channel.Value);
                }
            }
        }

        /// <summary>
        /// Organize the messages by channels to stream the send.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private Dictionary<ChannelParameter, List<PubSubEvent>> SplitByChannels(IEnumerable<PubSubEvent> messages)
        {
            // Build list by type of message!
            var messagesByType = messages.GroupBy(m => m.Value.GetType());

            // group the messages based on the address, so we stream the different messages in one send for the same address.
            var channels = new Dictionary<ChannelParameter, List<PubSubEvent>>();
            foreach (var groupedMessages in messagesByType)
            {
                var channel = GetDefiniton(groupedMessages.Key);
                var channelKey = channels.Keys.FirstOrDefault(c => c.Address.Equals(channel.Address, StringComparison.InvariantCultureIgnoreCase) && c.Identifier.Equals(channel.Identifier, StringComparison.InvariantCultureIgnoreCase));
                if (default(ChannelParameter) == channelKey)
                    channels[channel] = groupedMessages.ToList();
                else
                    channels[channelKey].AddRange(groupedMessages);
            }

            return channels;
        }



        private async Task PublishBatchAsync(ChannelParameter channelParameter, IEnumerable<PubSubEvent> events)
        {
            using (var sendActivity = _activitySource?.StartActivity("Send Pub Sub events to KubeMQ", ActivityKind.Producer))
            {
                var definition = channelParameter as PubSubDefinition;

                if (null == definition) throw new InvalidCastException("The channel parameter is not a Publisher one.");

                var channel = CreateChannel(definition);

                sendActivity?.SetTag("Address", definition.Address);

                var serializer = GetSerializer(definition);

                string activityId = _applicationContext?.Principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString();

                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                    foreach (var @event in events)
                    {
                        var message = new KubeMQEvent(null, string.Empty, serializer.Serialize(@event.Value).ToArray(), BuildTags(@event.Value.GetType(), @event.Tags, activityId));

                        await channel.StreamEvent(message);
                    }

                await channel.ClosesEventStreamAsync();
            }

        }
        private Dictionary<string, string> BuildTags(Type messageType, Dictionary<string, string> tags, string activityId)
        {
            var result = tags;
            if (null == tags)
                result = new Dictionary<string, string>();

            result.TryAdd("_ActivityId", activityId);
            result.TryAdd("_Type", messageType.FullName);

            return result;

        }

        private PubSubDefinition GetDefiniton(Type command)
        {
            var key = command.FullName;

            var definition = _definitions.Get(key);

            if (!definition.IsValid()) throw new NullReferenceException($"No PubSub definition found for {key}");

            return definition;
        }

        private IObjectSerialization GetSerializer(PubSubDefinition definition)
        {
            if (null == definition)
                throw new ArgumentNullException(nameof(definition));

            if (null == definition.Serializer || !_serviceProvider.TryGetService<IObjectSerialization>(definition.Serializer, out var serializerFactory))
            {
                return _serviceProvider.GetService<IObjectSerialization>();
            }

            return serializerFactory;

        }


        private Channel CreateChannel(PubSubDefinition definition)
        {
            if (null == definition)
                throw new ArgumentNullException(nameof(definition));

            return new Channel(definition.Namespace, definition.Identifier, definition.Persisted, definition.Address, _logger);
        }
    }
}
