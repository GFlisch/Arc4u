using Arc4u.Diagnostics;
using NServiceBus.Transport;
using NServiceBus.Transport.RabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Arc4u.NServiceBus.RabbitMQ.Routing
{
    public class PublisherRootingTopology : IRoutingTopology
    {
        public PublisherRootingTopology(Func<Type, String> exchangeName)
        {
            ExchangeName = exchangeName;
        }

        private Func<Type, String> ExchangeName;

        public void SetupSubscription(IModel channel, Type type, string subscriberName)
        {
        }

        public void TeardownSubscription(IModel channel, Type type, string subscriberName)
        {
        }



        public void Publish(IModel channel, Type type, OutgoingMessage message, IBasicProperties properties)
        {
            channel.BasicPublish(ExchangeName(type), String.Empty, false, properties, message.Body);
        }

        public void Send(IModel channel, string address, OutgoingMessage message, IBasicProperties properties)
        {
            channel.BasicPublish(address, String.Empty, true, properties, message.Body);
        }

        public void RawSendInCaseOfFailure(IModel channel, string address, byte[] body, IBasicProperties properties)
        {
            channel.BasicPublish(address, String.Empty, true, properties, body);
        }

        public void BindToDelayInfrastructure(IModel channel, string address, string deliveryExchange, string routingKey)
        {
            //channel.ExchangeBind(address, deliveryExchange, routingKey);
        }

        public void Initialize(IModel channel, IEnumerable<string> receivingAddresses, IEnumerable<string> sendingAddresses)
        {
        }

        public void RawSendInCaseOfFailure(IModel channel, string address, ReadOnlyMemory<byte> body, IBasicProperties properties)
        {
        }
    }
}
