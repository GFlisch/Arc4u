using Arc4u.Dependency.Attribute;

namespace Arc4u.NServiceBus
{
    [Export, Scoped]
    public class MessagesToPublish
    {
        private readonly List<Object> messages = new List<Object>();

        public static Func<Type, bool> EventsNamingConvention;

        public static Func<Type, bool> CommandsNamingConvention;

        /// <summary>
        /// Register a <see cref="ICommand"/> or <see cref="IEvent"/> to publish.
        /// </summary>
        /// <param name="message">Add a Command or event</param>
        public void Add(Object message)
        {
            if (null == EventsNamingConvention || null == CommandsNamingConvention)
            {
                throw new AppException("No conventions is defined for Commands or Events.");
            }

            if (EventsNamingConvention(message.GetType()) || CommandsNamingConvention(message.GetType()))
            {
                messages.Add(message);
            }
            else
            {
                throw new AppException("Doesn't respect the namespace convention defined for events and commands.");
            }
        }

        /// <summary>
        /// Used to clear any messages already registered.
        /// </summary>
        public void Clear()
        {
            if (null != messages)
            {
                messages.Clear();
            }
        }

        public List<Object> Events => messages.Where((m) => EventsNamingConvention(m.GetType())).ToList();

        public List<Object> Commands => messages.Where((m) => CommandsNamingConvention(m.GetType())).ToList();
    }
}
