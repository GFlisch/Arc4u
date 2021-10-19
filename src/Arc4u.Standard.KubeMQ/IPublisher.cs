using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arc4u.KubeMQ
{
    public interface IPublisher
    {
        /// <summary>
        /// Define the contract to publish on a channel for an event.
        /// </summary>
        /// <typeparam name="TEvent">The type mapped to a channel to publish.</typeparam>
        /// <param name="event">The event.</param>
        void Publish<TEvent>(TEvent @event, Dictionary<string, string> tags = null) where TEvent : notnull;

        /// <summary>
        /// Define the contract to publish on a channel for an event.
        /// </summary>
        /// <param name="event">The collection of events.</param>
        Task PublishBatchAsync(IEnumerable<PubSubEvent> messages);
    }
}
