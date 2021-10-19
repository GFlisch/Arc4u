using Arc4u.Diagnostics;
using NServiceBus;
using System;
using System.Threading.Tasks;

namespace Arc4u.NServiceBus
{
    /// <summary>
    /// The base class will wrap the handle of a NServiceBus message by
    /// adding the unit of work behavior to add events and commands during the
    /// process of a message. See <see cref="MessagesToPublish"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class HandleMessageBase<T> : IHandleMessages<T>
    {
        /// <summary>
        /// Method that will be used by NServiceBus to process a message.
        /// The real work is implemented in the Handle(T message) abstract method.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Handle(T message, IMessageHandlerContext context)
        {
            try
            {
                // Create the unit of work list of messages.
                MessagesToPublish.Create();

                // business work to implement.
                await Handle(message);

                // Publish events.
                foreach (Object _event in MessagesToPublish.Events)
                {
                    try
                    {
                        await context.Publish(_event);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Technical.From(typeof(HandleMessageBase<T>)).Exception(ex).Log();
                    }
                }

                // Send commands.
                foreach (Object command in MessagesToPublish.Commands)
                {
                    try
                    {
                        await context.Send(command);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Technical.From(typeof(HandleMessageBase<T>)).Exception(ex).Log();
                    }
                }

            }
            catch (System.Exception ex)
            {
                Logger.Technical.From(typeof(HandleMessageBase<T>)).Exception(ex).Log();
            }
            finally
            {
                // Clear messages to avoid any sending of undesirable messages by
                // another message received on the bus.
                MessagesToPublish.Clear();
            }
        }

        /// <summary>
        /// Method to implement to process the business.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task Handle(T message);
    }
}
