using Arc4u.Diagnostics;
using Arc4u.KubeMQ.AspNetCore.Configuration;
using KubeMQ.SDK.csharp.Events;
using KubeMQ.SDK.csharp.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Arc4u.KubeMQ.AspNetCore.PubSub
{
    public class EventProcessorListener
    {
        public EventProcessorListener(CancellationToken stoppingToken,
                                      IServiceProvider provider,
                                      IMessageHandlerTypes messageHandlers,
                                      SubscriberParameters subscriberParameters)
        {
            _stoppingToken = stoppingToken;
            _subscriberParameters = subscriberParameters;
            _provider = provider;
            _messageHandlers = messageHandlers;
            _logger = provider.GetService<ILogger<EventProcessorListener>>();
            _hasThreadFinishToProcess = new AutoResetEvent(false);
        }

        private readonly CancellationToken _stoppingToken;
        private readonly SubscriberParameters _subscriberParameters;
        private readonly IServiceProvider _provider;
        private readonly IMessageHandlerTypes _messageHandlers;
        private readonly AutoResetEvent _hasThreadFinishToProcess;
        private readonly ILogger<EventProcessorListener> _logger;

        public AutoResetEvent HasThreadFinishToProcess => _hasThreadFinishToProcess;


        public void ProcessSubscriber()
        {
            try
            {
                var subscriber = new Subscriber(_subscriberParameters.Address);

                var eventStore = EventsStoreType.Undefined;
                var subscribeType = SubscribeType.Events;
                if (_subscriberParameters is PersistedSubscriberParameters persistedOne)
                {
                    eventStore = persistedOne.EventsStoreType;
                    subscribeType = SubscribeType.EventsStore;
                }

                var policy = _subscriberParameters.RetryPolicy(Policy.Handle<Exception>());

                policy.Execute(cancellationToken =>
                {
                    _logger.Technical().System($"Try subscribing connection to publisher {_subscriberParameters.Namespace}.")
                                       .Add("Address", _subscriberParameters.Address)
                                       .Add("GroupName", _subscriberParameters.GroupName)
                                       .Add("Identifier", _subscriberParameters.Identifier)
                                       .AddIf(_subscriberParameters is PersistedSubscriberParameters, "EventsStoreType", (_subscriberParameters as PersistedSubscriberParameters)?.EventsStoreType.ToString())
                                       .Add("EventsStore", _subscriberParameters is PersistedSubscriberParameters)
                                       .Log();

                    subscriber.SubscribeToEvents(new SubscribeRequest
                    {
                        Channel = _subscriberParameters.Namespace,
                        SubscribeType = subscribeType,
                        ClientID = _subscriberParameters.Identifier,
                        EventsStoreType = eventStore,
                        Group = _subscriberParameters.GroupName,
                    }, (eventReceive) =>
                    {
                        ThreadPool.QueueUserWorkItem<Tuple<EventReceive, SubscriberParameters>>(Handle, Tuple.Create(eventReceive, _subscriberParameters), true);
                    },
                    (errorHandler) =>
                    {
                        _logger.Technical().Exception(errorHandler.GetBaseException()).Add("Ex", "true").Log();

                    }, _stoppingToken);
                }, _stoppingToken);

                _logger.Technical().System($"Connection to publisher {_subscriberParameters.Namespace} done.").Log();

                while (!_stoppingToken.IsCancellationRequested)
                {
                    Thread.Sleep(10);
                }

                _logger.Technical().System($"Stop the listening of the subscription {_subscriberParameters.Namespace}.").Log();

                _hasThreadFinishToProcess.Set();

            }
            /// CancellationToken is canceled.
            catch (OperationCanceledException)
            {
                // Log we skip this.
                _logger.Technical().System($"Exit listening publisher {_subscriberParameters.Namespace}.").Log();
            }
            catch (Exception ex)
            {
                // Log we skip this.
                _logger.Technical().Exception(ex).Log();
            }
        }

        private (Guid activityId, string messageType, Dictionary<string, string> tags) BuildTagsFromTags(Dictionary<string, string> tags)
        {
            if (null == tags)
                throw new ArgumentNullException(nameof(tags));

            var tagsResult = new Dictionary<string, string>();
            Guid activityId = Guid.NewGuid();
            Guid.TryParse(tags["_ActivityId"], out activityId);

            // mus be faster than linq.
            foreach (var tag in tags)
                if (tag.Key[0] != '_') tagsResult.Add(tag.Key, tag.Value);

            return (activityId, tags["_Type"], tagsResult);
        }

        private void Handle(Tuple<EventReceive, SubscriberParameters> tuple)
        {
            try
            {
                EventReceive @event = tuple.Item1;
                SubscriberParameters subscriberParameter = tuple.Item2;

                var extract = BuildTagsFromTags(@event.Tags);

                var key = extract.messageType + subscriberParameter.Namespace;

                var interfaceType = _messageHandlers.GetOrAddType(key, out var handlerDataType);

                var parameter = _messageHandlers.Serializer(key).Deserialize(@event.Body, handlerDataType);

                using (var scope = _provider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetService(interfaceType);

                    var context = scope.ServiceProvider.GetService<HandlerContext>();

                    if (null != context)
                    {
                        context.ActivityId = extract.activityId;
                        context.Tags = extract.tags;
                    }

                    handler.GetType().InvokeMember("Handle",
                                                   BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                                   null,
                                                   handler,
                                                   new object[] { parameter });
                }
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();
            }
        }
    }
}
