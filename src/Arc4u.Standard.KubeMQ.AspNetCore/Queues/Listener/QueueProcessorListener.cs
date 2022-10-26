using Arc4u.Diagnostics;
using Arc4u.KubeMQ.AspNetCore.Configuration;
using KubeMQ.SDK.csharp.QueueStream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.KubeMQ.AspNetCore.Queues
{
    public class QueueProcessorListener
    {

        public QueueProcessorListener(CancellationToken stoppingToken,
                                      QueueParameters queueParameters,
                                      IQueueStreamManager queueStreamManager,
                                      IMessageHandlerTypes messageHandlers,
                                      IServiceProvider provider)
        {
            _stoppingToken = stoppingToken;
            _queueParameters = queueParameters;
            _queueStreamManager = queueStreamManager;
            _logger = provider.GetRequiredService<ILogger<QueueProcessorListener>>();
            _messageHandlers = messageHandlers;
            _provider = provider;
            _hasThreadFinishToProcess = new AutoResetEvent(false);
        }

        private readonly CancellationToken _stoppingToken;
        private readonly QueueParameters _queueParameters;
        private readonly IQueueStreamManager _queueStreamManager;
        private readonly ILogger<QueueProcessorListener> _logger;
        private readonly IServiceProvider _provider;
        private readonly IMessageHandlerTypes _messageHandlers;
        private readonly AutoResetEvent _hasThreadFinishToProcess;


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

        public AutoResetEvent HasThreadFinishToProcess => _hasThreadFinishToProcess;

        public void ProcessTxQueue()
        {

            Policy.Handle<Exception>()
                  .WaitAndRetryForever((ctx) => TimeSpan.FromSeconds(2))
                  .Execute(async () =>
                  {
                      var queue = _queueStreamManager.Get(_queueParameters);
                      if (null == queue)
                      {
                          _logger.Technical().Error($"Queue {_queueParameters.Address} is not created. Listening is stopped.").Log();
                          return;
                      }

                      PollRequest pollRequest = new()
                      {
                          Queue = _queueParameters.Namespace,
                          WaitTimeout = (int)_queueParameters.WaitTimeMilliSecondsQueueMessages,
                          MaxItems = (int)_queueParameters.MaxNumberOfMessagesRead,
                          AutoAck = false
                      };

                      while (!_stoppingToken.IsCancellationRequested)
                      {
                          PollResponse response = await queue.Poll(pollRequest);

                          if (!response.HasMessages) continue;

                          if (!string.IsNullOrWhiteSpace(response.Error))
                          {
                              _logger.Technical().Error(response.Error).Log();
                          }
                          else
                          {
                              var count = response.Messages.Count();
                              _logger.Technical().System($"Receive {count} message(s).").Add("Messages", count)
                                                                                        .Add("Namespace", _queueParameters.Namespace)
                                                                                        .Add("MaxNumberOfMessagesRead", (int)_queueParameters.MaxNumberOfMessagesRead)
                                                                                        .Add("WaitTimeMilliSecondsQueueMessages", (int)_queueParameters.WaitTimeMilliSecondsQueueMessages)
                                                                                        .Add("KubeMQAddress", _queueParameters.Address).Log();
                          }

                          // must check the  CancellationToken.
                          // Nack each non process messages before leaving.
                          // Give the CancellationToken to the Handler!
                          Parallel.ForEach(response.Messages, msg =>
                          {
                              if (_stoppingToken.IsCancellationRequested)
                              {
                                  // Fast enough to finish in the 5 seconds.
                                  msg.NAck();
                              }
                              else
                                  try
                                  {
                                      var extract = BuildTagsFromTags(msg.Tags);
                                      var interfaceType = _messageHandlers.GetOrAddType(extract.messageType + _queueParameters.Namespace, out var handlerDataType);

                                      var parameter = _messageHandlers.Serializer(extract.messageType + _queueParameters.Namespace).Deserialize(msg.Body, handlerDataType);

                                      using (var scope = _provider.CreateScope())
                                      {
                                          var handler = scope.ServiceProvider.GetRequiredService(interfaceType);
                                          var context = scope.ServiceProvider.GetService<HandlerContext>();

                                          if (null != context)
                                          {
                                              context.ActivityId = extract.activityId;
                                              context.Tags = extract.tags;
                                          }

                                          //todo: Add the CancellationToken
                                          handler.GetType().InvokeMember("Handle",
                                                                         BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                                                         null,
                                                                         handler,
                                                                         new object[] { parameter });
                                      }

                                      msg.Ack();

                                  }
                                  catch (Exception ex)
                                  {
                                      // Send to DeadLetterQueue if failed.
                                      //msg.ReQueue(_queueParameters.DeadLetterQueue);
                                      // Or if no DeadLetterQueue
                                      msg.NAck();

                                      _logger.Technical().Exception(ex).Log();
                                  }
                          });

                      }

                      _queueStreamManager.Close(_queueParameters);

                      _logger.Technical().System($"Stop the listening of the Queue {_queueParameters.Namespace}.").Log();

                      _hasThreadFinishToProcess.Set();
                  });
        }
    }
}
