using Arc4u.Diagnostics;
using Arc4u.Standard.UnitTest.Infrastructure;
using KubeMQ.Grpc;
using KubeMQ.SDK.csharp.Events;
using KubeMQ.SDK.csharp.QueueStream;
using KubeMQ.SDK.csharp.Subscription;
using KubeMQ.SDK.csharp.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Event = KubeMQ.SDK.csharp.Events.Event;

namespace Arc4u.Standard.UnitTest.KubeMQ
{
    public class KubeMQFundamentalsTest : BaseContainerFixture<KubeMQFundamentalsTest, BasicFixture>
    {
        public KubeMQFundamentalsTest(BasicFixture fixture) : base(fixture)
        {
        }


        [Fact]
        public async Task TestEventSendAndReceive()
        {
            var ChannelName = "Order.Event";
            var ClientID = "Demo.Test";
            var KubeMQServerAddress = "localhost:50300";
            var subscriber = new Subscriber(KubeMQServerAddress);

            long counter = 0;

            var channel = new Channel(new ChannelParameters
            {
                ChannelName = ChannelName,
                ClientID = ClientID,
                KubeMQAddress = KubeMQServerAddress
            });

            try
            {
                subscriber.SubscribeToEvents(new SubscribeRequest
                {
                    Channel = ChannelName,
                    SubscribeType = SubscribeType.Events,
                    ClientID = ClientID
                }, (eventReceive) =>
                {
                    Interlocked.Increment(ref counter);

                    Logger.Technical().Debug($"Event Received: EventID:{eventReceive.EventID} Channel:{eventReceive.Channel} Metadata:{eventReceive.Metadata} Body:{ Converter.FromByteArray(eventReceive.Body)} ").Log();
                },
                (errorHandler) =>
                {
                    Logger.Technical().Error(errorHandler.Message).Log();
                });
            }
            catch (Exception ex)
            {
                Logger.Technical().Exception(ex).Log();
            }

            try
            {
                _ = channel.StreamEvent(new Event
                {
                    Body = Converter.ToByteArray("hello kubemq - sending stream event")
                });
            }
            catch (Exception ex)
            {
                Logger.Technical().Exception(ex).Log();
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                await Task.Delay(10);
            } while (stopwatch.ElapsedMilliseconds < 2000);

            stopwatch.Stop();

            Logger.Technical().Debug($"Number of retry = {counter}.").Log();

            Assert.True(counter == 1);
        }


        [Fact]
        public async Task TestBasicSendAndReceive()
        {
            QueueStream queue = new QueueStream("localhost:50300", "Demo.Test.Basic", null);

            var messages = new List<Message>();
            var tags = new Dictionary<string, string>
            {
                { "key", "value" }
            };

            for (int i = 0; i < 1000; i++)
                messages.Add(
                    new Message("Order.Test.OutQ.Basic", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), string.Empty, Guid.NewGuid().ToString(), tags)
                );

            var res = await queue.Send(new SendRequest(messages));

            PollRequest pollRequest = new()
            {
                Queue = "Order.Test.OutQ.Basic",
                WaitTimeout = 1000,
                MaxItems = 100,
                AutoAck = false
            };

            var read = 0;

            while (read < 1000)
            {
                PollResponse response = await queue.Poll(pollRequest);

                if (!response.HasMessages) continue;

                Logger.Technical().Debug($"{response.Messages.Count()} message(s) received from the PollResponse.").Log();

                Parallel.ForEach(response.Messages, msg =>
                {
                    try
                    {
                        Assert.Equal(1, msg.Tags.Count());

                        Assert.True(Guid.TryParse(Encoding.UTF8.GetString(msg.Body), out var guid));

                        msg.Ack();

                    }
                    catch (Exception)
                    {
                        msg.NAck();
                    }

                });

                read += response.Messages.Count();
            }

            Assert.Equal(1000, read);
            queue.Close();
        }


        [Fact]
        public async Task TestBasicSendWithDelayAndReceive()
        {
            QueueStream queue = new QueueStream("localhost:50300", "Demo.Test.Delay", null);

            var messages = new List<Message>();
            var tags = new Dictionary<string, string>
            {
                { "key", "value" }
            };

            for (int i = 0; i < 1000; i++)
                messages.Add(
                    new Message("Order.Test.OutQ.Delay", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), string.Empty, Guid.NewGuid().ToString(), tags)
                    {
                        Policy = new QueueMessagePolicy() { DelaySeconds = 11 }
                    }
                );

            var res = await queue.Send(new SendRequest(messages));

            PollRequest pollRequest = new()
            {
                Queue = "Order.Test.OutQ.Delay",
                WaitTimeout = 1000,
                MaxItems = 100,
                AutoAck = false
            };

            var read = 0;
            var waitCount = 0;

            while (read < 1000)
            {
                PollResponse response = await queue.Poll(pollRequest);

                if (!response.HasMessages)
                {
                    waitCount++;
                    continue;
                }

                Logger.Technical().Debug($"{response.Messages.Count()} message(s) received from the PollResponse.").Log();

                Parallel.ForEach(response.Messages, msg =>
                {
                    try
                    {
                        Assert.Equal(1, msg.Tags.Count());

                        Assert.True(Guid.TryParse(Encoding.UTF8.GetString(msg.Body), out var guid));

                        msg.Ack();

                    }
                    catch (Exception)
                    {
                        msg.NAck();
                    }

                });

                read += response.Messages.Count();
            }

            Assert.Equal(1000, read);
            Assert.True(waitCount > 9, $"Counted = {waitCount}");
            queue.Close();
        }


        private static long touched;

        [Fact]
        public async Task TestIisHostReceiver()
        {
            touched = 0;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Start the listener on another thread.
            Task t = await Task.Factory.StartNew(Listener2, tokenSource.Token);

            var stopwatch = new Stopwatch();

            Assert.Equal(0, Interlocked.Read(ref touched));

            var messages = new List<Message>();
            var tags = new Dictionary<string, string>
            {
                { "key", "value" }
            };

            for (int i = 0; i < 1000; i++)
                messages.Add(
                    new Message("Order.Test.OutQ.Listener", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), string.Empty, Guid.NewGuid().ToString(), tags)
                );


            Assert.Equal(0, Interlocked.Read(ref touched));

            QueueStream queue = new QueueStream("localhost:50300", "Demo.Test.IISHost", null);

            stopwatch.Start();

            await queue.Send(new SendRequest(messages));

            stopwatch.Stop();

            Logger.Technical().Debug($"Messages sent in {stopwatch.ElapsedMilliseconds} ms.").Log();

            stopwatch.Reset();
            stopwatch.Start();


            do
            {
                await Task.Delay(10);
            } while (Interlocked.Read(ref touched) < 1000 && stopwatch.ElapsedMilliseconds < 10000);

            stopwatch.Stop();

            Logger.Technical().Debug($"Messages read in {stopwatch.ElapsedMilliseconds} ms.").Log();

            tokenSource.Cancel();

            await Task.Delay(150);

            Assert.Equal(1000, Interlocked.Read(ref touched));
            Task.WaitAll(t);
            queue.Close();
        }

        private async Task Listener2(object parameter)
        {
            CancellationToken cancellationToken = (CancellationToken)parameter;

            QueueStream queue = new QueueStream("localhost:50300", "Demo.Test.Listener", null);

            PollRequest pollRequest = new()
            {
                Queue = "Order.Test.OutQ.Listener",
                WaitTimeout = 1000,
                MaxItems = 100,
                AutoAck = false
            };

            var messagesToAck = new ConcurrentBag<Message>();
            do
            {
                PollResponse response = await queue.Poll(pollRequest);

                if (!response.HasMessages) continue;

                Logger.Technical().Debug($"{response.Messages.Count()} message(s) received from the PollResponse.").Log();

                Parallel.ForEach(response.Messages, async msg =>
                {
                    try
                    {
                        Assert.Equal(1, msg.Tags.Count());

                        Assert.True(Guid.TryParse(Encoding.UTF8.GetString(msg.Body), out var guid));
                        await Task.Delay(100);

                        msg.Ack();
                        Interlocked.Increment(ref touched);
                    }
                    catch (Exception ex)
                    {
                        Logger.Technical().Exception(ex).Log();
                        msg.NAck();
                    }
                });

            } while (!cancellationToken.IsCancellationRequested);

            queue.Close();
        }
    }
}
