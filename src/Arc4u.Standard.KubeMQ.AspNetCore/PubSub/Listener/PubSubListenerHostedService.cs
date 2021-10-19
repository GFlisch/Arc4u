using Arc4u.KubeMQ.AspNetCore.Configuration;
using KubeMQ.SDK.csharp.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.KubeMQ.AspNetCore.PubSub
{
    public class PubSubListenerHostedService : BackgroundService
    {
        public PubSubListenerHostedService(IServiceProvider provider, IMessageHandlerTypes messageHandlers, IEnumerable<SubscriberParameters> subscribers, ILogger<PubSubListenerHostedService> logger)
        {
            _messageHandlers = messageHandlers;
            _logger = logger;
            _provider = provider;
            _subscribers = subscribers;
            _kubemqLogger = _provider.GetService<ILogger<Subscriber>>();
        }

        private readonly IMessageHandlerTypes _messageHandlers;
        private readonly IServiceProvider _provider;
        private readonly IEnumerable<SubscriberParameters> _subscribers;
        private readonly ILogger<PubSubListenerHostedService> _logger;
        private readonly ILogger<Subscriber> _kubemqLogger;

        private List<AutoResetEvent> WaitHandles;


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            WaitHandles = new List<AutoResetEvent>(_subscribers.Count());

            foreach (var subscriber in _subscribers)
            {
                var eventProcessor = new EventProcessorListener(stoppingToken, _provider, _messageHandlers, subscriber);

                WaitHandles.Add(eventProcessor.HasThreadFinishToProcess);

                var thread = new Thread(eventProcessor.ProcessSubscriber);

                thread.Start();
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            var task = base.StopAsync(cancellationToken);

            if (WaitHandles.Count() != 0)
                WaitHandle.WaitAll(WaitHandles.ToArray());

            return task;
        }
    }
}
