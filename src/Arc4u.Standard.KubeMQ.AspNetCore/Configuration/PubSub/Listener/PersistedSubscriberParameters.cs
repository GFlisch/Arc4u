using KubeMQ.SDK.csharp.Subscription;

namespace Arc4u.KubeMQ.AspNetCore.Configuration
{
    public class PersistedSubscriberParameters : SubscriberParameters
    {
        public EventsStoreType EventsStoreType { get; set; } = EventsStoreType.StartNewOnly;
    }
}
