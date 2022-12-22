using Arc4u.KubeMQ.AspNetCore.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.KubeMQ.AspNetCore.Queues
{
    public class QueueListenerHostedService : BackgroundService
    {
        public QueueListenerHostedService(IServiceProvider provider,
                                          IMessageHandlerTypes messageHandlers,
                                          IUniqueness unique,
                                          IQueueStreamManager queueStreamManager,
                                          ILogger<QueueListenerHostedService> logger)
        {
            _provider = provider;
            _messageHandlers = messageHandlers;
            _queueParameters = unique.GetQueues();
            _logger = logger;
            _queueStreamManager = queueStreamManager;
        }

        private readonly IMessageHandlerTypes _messageHandlers;
        private readonly IServiceProvider _provider;
        private readonly IEnumerable<QueueParameters> _queueParameters;
        private readonly ILogger<QueueListenerHostedService> _logger;
        private readonly IQueueStreamManager _queueStreamManager;

        private List<AutoResetEvent> WaitHandles;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            WaitHandles = new List<AutoResetEvent>(_queueParameters.Count());

            foreach (var queue in _queueParameters)
            {
                var processQueue = new QueueProcessorListener(stoppingToken, queue, _queueStreamManager, _messageHandlers, _provider);

                WaitHandles.Add(processQueue.HasThreadFinishToProcess);

                var thread = new Thread(processQueue.ProcessTxQueue);

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
