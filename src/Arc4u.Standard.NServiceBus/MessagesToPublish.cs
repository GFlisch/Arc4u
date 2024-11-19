using NServiceBus;

namespace Arc4u.NServiceBus
{
    /// <summary>
    /// The class implement the unit of work pattern so any business logic can add
    /// events or commands to send. The messages are not sent immediately but more
    /// when all is done and no errors arose during the process.
    /// From a developer perspective only the Add method is used.
    /// </summary>
    public sealed class MessagesToPublish
    {
        static readonly AsyncLocal<List<Object>> messages = new AsyncLocal<List<Object>>();

        public static Func<Type, bool> EventsNamingConvention;

        public static Func<Type, bool> CommandsNamingConvention;
        /// <summary>
        /// Create a unit of work collection of messages.
        /// </summary>
        internal static void Create()
        {
            messages.Value = new List<Object>();
        }

        /// <summary>
        /// Register a <see cref="ICommand"/> or <see cref="IEvent"/> to publish.
        /// </summary>
        /// <param name="message">Add a Command or event</param>
        public static void Add(Object message)
        {
            if (null == EventsNamingConvention || null == CommandsNamingConvention)
            {
                throw new AppException("No conventions is defined for Commands or Events.");
            }

            if (EventsNamingConvention(message.GetType()) || CommandsNamingConvention(message.GetType()))
            {
                messages.Value.Add(message);
            }
            else
            {
                throw new AppException("Doesn't respect the namespace convention defined for events and commands.");
            }
        }

        /// <summary>
        /// Used to clear any messages already registered.
        /// </summary>
        public static void Clear()
        {
            if (null != messages.Value)
            {
                messages.Value.Clear();
            }
        }

        internal static List<Object> Events => messages.Value.Where((m) => EventsNamingConvention(m.GetType())).ToList();

        internal static List<Object> Commands => messages.Value.Where((m) => CommandsNamingConvention(m.GetType())).ToList();
    }
}
