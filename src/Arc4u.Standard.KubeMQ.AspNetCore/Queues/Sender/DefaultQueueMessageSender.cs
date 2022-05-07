using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.KubeMQ.AspNetCore.Configuration;
using Arc4u.Security.Principal;
using Arc4u.Serializer;
using KubeMQ.SDK.csharp.QueueStream;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QueueMessagePolicy = KubeMQ.Grpc.QueueMessagePolicy;

namespace Arc4u.KubeMQ.AspNetCore.Queues
{
    public class DefaultQueueMessageSender : IQueueMessageSender
    {
        public DefaultQueueMessageSender(IOptionsMonitor<QueueDefinition> definitions,
                                         IContainerResolve serviceProvider,
                                         IQueueStreamManager queueStreamManager,
                                         IApplicationContext applicationContext,
                                         ILogger<DefaultQueueMessageSender> logger,
                                         IActivitySourceFactory activitySourceFactory)
        {
            _definitions = definitions;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _applicationContext = applicationContext;
            _queueStreamManager = queueStreamManager;
            _activitySource = activitySourceFactory.GetArc4u();
        }

        private readonly IOptionsMonitor<QueueDefinition> _definitions;
        private readonly IContainerResolve _serviceProvider;
        private readonly ILogger<DefaultQueueMessageSender> _logger;
        private readonly IApplicationContext _applicationContext;
        private readonly IQueueStreamManager _queueStreamManager;
        private readonly ActivitySource _activitySource;

        public void Send<TMessage>(TMessage message, Dictionary<string, string> tags = null, uint? sendAfterSeconds = null, uint? expireAfterSeconds = null) where TMessage : notnull
        {
            using (var activity = _activitySource?.StartActivity("Send message to KubeMQ", ActivityKind.Producer))
            {
                activity?.SetTag("Delay message [s]", sendAfterSeconds);
                activity?.SetTag("Expire message after [s]", expireAfterSeconds);

                if (message is IEnumerable)
                    throw new ArgumentException("Collection are not accepted!");

                var messageType = typeof(TMessage);

                var definition = GetDefiniton(messageType);

                var policy = definition.RetryPolicy(Policy.Handle<Exception>());

                QueueStream queue = null;
                policy.Execute(() => queue = _queueStreamManager.Get(definition));

                if (null == queue)
                    throw new NullReferenceException($"No queue was created for Address: {definition.Address} and Identitfier {definition.Identifier}");

                var serializer = GetSerializer(definition);

                string activityId = _applicationContext?.Principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString();

                Message _message = null;
                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                    _message = new Message(definition.Namespace, serializer.Serialize(message), String.Empty, Guid.NewGuid().ToString(), BuildTags(messageType, tags, activityId))
                    {
                        Policy = BuildPolicy(definition, sendAfterSeconds, expireAfterSeconds)
                    };

                using (var sendActivity = _activitySource?.StartActivity("Send to KubeMQ", ActivityKind.Producer))
                    queue.Send(new SendRequest(new List<Message>(1) { _message }));
            }
        }

        private QueueMessagePolicy BuildPolicy(QueueDefinition definition, uint? sendAfterSeconds, uint? expireAfterSeconds)
        {
            QueueMessagePolicy result = null;

            if (definition.MaxRetryBeforeSendingToDeadLetterQueue > 0 && !String.IsNullOrWhiteSpace(definition.DeadLetterQueueName))
            {
                result = result ?? new QueueMessagePolicy();
                result.MaxReceiveCount = (int)definition.MaxRetryBeforeSendingToDeadLetterQueue;
                result.MaxReceiveQueue = definition.DeadLetterQueueName;
            }

            if (expireAfterSeconds.HasValue)
            {
                result = result ?? new QueueMessagePolicy();
                result.ExpirationSeconds = (int)expireAfterSeconds.Value;
            }


            if (sendAfterSeconds.HasValue)
            {
                result = result ?? new QueueMessagePolicy();
                result.DelaySeconds = (int)sendAfterSeconds.Value;
            }


            return result;
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

        public void SendBatch(IEnumerable<QueueMessage> messages)
        {
            Dictionary<ChannelParameter, List<QueueMessage>> channels = SplitByChannels(messages);

            foreach (var channel in channels)
            {
                SendToQueueInBatch(channel.Value);
            }
        }

        public void SendBatch(IEnumerable<QueueMessage> messages, int splitByQueues)
        {
            Dictionary<ChannelParameter, List<QueueMessage>> channels = SplitByChannels(messages);

            foreach (var channel in channels)
            {
                SendToQueueInBatch(channel.Value, splitByQueues);
            }
        }

        /// <summary>
        /// Organize the messages by channels to stream the send.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private Dictionary<ChannelParameter, List<QueueMessage>> SplitByChannels(IEnumerable<QueueMessage> messages)
        {
            // Build list by type of message!
            var messagesByType = messages.GroupBy(m => m.Value.GetType());

            // group the messages based on the address, so we stream the different messages in one send for the same address.
            var channels = new Dictionary<ChannelParameter, List<QueueMessage>>();
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

        /// <summary>
        /// All the messages must be of the same type.
        /// </summary>
        /// <param name="messages"></param>
        private void SendToQueueInBatch(IEnumerable<QueueMessage> messages)
        {
            using (var activity = _activitySource?.StartActivity("Send messages to KubeMQ", ActivityKind.Producer))
            {
                var definition = GetDefiniton(messages.First().Value.GetType());

                var policy = definition.RetryPolicy(Policy.Handle<Exception>());

                QueueStream queue = null;
                policy.Execute(() => queue = _queueStreamManager.Get(definition));

                if (null == queue)
                    throw new NullReferenceException($"No queue was created for Address: {definition.Address} and Identitfier {definition.Identifier}");



                var serializer = GetSerializer(definition);
                var _messages = new List<Message>(messages.Count());

                string activityId = _applicationContext?.Principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString();


                using (var serializerActivity = _activitySource?.StartActivity("Serialize.", ActivityKind.Producer))
                {
                    serializerActivity?.SetTag("Count", messages.Count());
                    // I use messageType because we consider that we send a message of the same type...
                    // Doing it by ChannelParameter will be more efficient...
                    foreach (var message in messages)
                        _messages.Add(
                            new Message(definition.Namespace, serializer.Serialize(message.Value), String.Empty, Guid.NewGuid().ToString(), BuildTags(message.Value.GetType(), message.Tags, activityId))
                            {
                                Policy = BuildPolicy(definition, message.SendAfterSeconds, message.ExpireAfterSeconds)
                            }
                        );
                }

                policy.Execute(() =>
                {
                    using (var sendActivity = _activitySource?.StartActivity("Send to KubeMQ", ActivityKind.Producer))
                        queue.Send(new SendRequest(_messages));
                });
            }
        }

        private void SendToQueueInBatch(IEnumerable<QueueMessage> messages, int splitByQueues)
        {
            using (var activity = _activitySource?.StartActivity("Send messages to KubeMQ", ActivityKind.Producer))
            {
                activity?.SetTag("Split", splitByQueues);

                if (splitByQueues < 2)
                    throw new ArgumentOutOfRangeException("Number of queues must be greater than 1!");

                if (splitByQueues > 10)
                {
                    splitByQueues = 10;
                    _logger.Technical().Warning("Cap the number of queues to 10").Log();
                }

                var messageType = messages.First().Value.GetType();
                // All the messages are assumed to be of the same type.
                var definition = GetDefiniton(messageType);

                var policy = definition.RetryPolicy(Policy.Handle<Exception>());

                QueueStream queue = null;
                policy.Execute(() => queue = _queueStreamManager.Get(definition));

                if (null == queue)
                    throw new NullReferenceException($"No queue was created for Address: {definition.Address} and Identitfier {definition.Identifier}");

                var serializer = GetSerializer(definition);

                string activityId = _applicationContext?.Principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString();

                var messagesByQueue = Math.DivRem(messages.Count(), splitByQueues, out var rem);
                var _messages = new List<Message>(messagesByQueue + rem);

                int idx = 0;
                int queueIdx = 1;
                foreach (var message in messages)
                {
                    using (var createactivity = _activitySource?.StartActivity("Create KubeMQ messages", ActivityKind.Producer))
                    {
                        createactivity?.SetTag("Count", messages.Count());
                        createactivity?.SetTag("QueueName", $"{definition.Namespace}.{queueIdx}");

                        _messages.Add(
                            new Message($"{definition.Namespace}.{queueIdx}", serializer.Serialize(message.Value), String.Empty, Guid.NewGuid().ToString(), BuildTags(messageType, message.Tags, activityId))
                            {
                                Policy = BuildPolicy(definition, message.SendAfterSeconds, message.ExpireAfterSeconds)
                            }
                        );
                    }
                    idx++;

                    if (0 == idx % messagesByQueue) // we have on batch.
                    {

                        policy.Execute(() =>
                        {
                            using (var sendActivity = _activitySource?.StartActivity("Send to KubeMQ", ActivityKind.Producer))
                                queue.Send(new SendRequest(_messages));
                        });
                    
                        queueIdx++;
                        idx = 0;
                        _messages.Clear();
                    }
                }
            }
        }

        private QueueDefinition GetDefiniton(Type command)
        {
            var key = command.FullName;

            var definition = _definitions.Get(key);

            if (!definition.IsValid()) throw new NullReferenceException($"No command definition found for {key}");

            return definition;
        }

        private IObjectSerialization GetSerializer(QueueDefinition definition)
        {
            if (null == definition)
                throw new ArgumentNullException(nameof(definition));

            if (null == definition.Serializer || !_serviceProvider.TryResolve<IObjectSerialization>(definition.Serializer, out var serializer))
            {
                return _serviceProvider.Resolve<IObjectSerialization>();
            }

            return serializer;
        }
    }
}
