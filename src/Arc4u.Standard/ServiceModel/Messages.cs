using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Arc4u.ServiceModel
{
    public class Messages : List<Message>
    {
        private readonly bool _ignoreWarnings;


        public Messages(bool ignoreWarnings = false)
        {
            _ignoreWarnings = ignoreWarnings;
        }

        public IEnumerable<Message> Information
        {
            get { return this.Where(m => m.Type == MessageType.Information); }
        }

        public IEnumerable<Message> Warnings
        {
            get { return this.Where(m => m.Type == MessageType.Warning); }
        }

        public IEnumerable<Message> Errors
        {
            get { return this.Where(m => m.Type == MessageType.Error || m.Type == MessageType.Critical); }
        }

        public static Messages FromEnum(IEnumerable<Message> messages)
        {
            var newMessages = new Messages();
            newMessages.AddRange(messages);
            return newMessages;
        }

        private bool AlreadyAdded(Message item)
        {
            return this.Any(message => message.Text == item.Text && message.Category == item.Category);
        }

        public new void Add(Message message)
        {
            if (!AlreadyAdded(message))
            {
                base.Add(message);
            }
        }

        public bool WouldThrow()
        {
            return Errors.Any() || (Warnings.Any() && !_ignoreWarnings);
        }

        public void LogAndThrowIfNecessary<T>(ILogger<T> logger, [CallerMemberName] string methodName = "")
        {
            if (WouldThrow())
                throw new AppException(this);

            LogAll(logger, methodName);

        }
        /// <summary>
        /// Localized and log all messages.
        /// </summary>
        public void LogAll<T>(ILogger<T> logger, [CallerMemberName] string methodName = "")
        {
            if (null == logger)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            var _type = typeof(T);

            ForEach((m) =>
            {
                var lm = m.LocalizeMessage(Threading.Culture.Neutral);
                CommonMessageLogger categoryLogger = m.Category == MessageCategory.Business ? logger.Business(methodName) : logger.Technical(methodName);
                CommonLoggerProperties property = null;
                switch (m.Type)
                {
                    case MessageType.Information:
                        property = categoryLogger.Information(lm.Text);
                        break;
                    case MessageType.Warning:
                        property = categoryLogger.Warning(lm.Text);
                        break;
                    case MessageType.Error:
                        property = categoryLogger.Error(lm.Text);
                        break;
                    case MessageType.Critical:
                        property = categoryLogger.Fatal(lm.Text);
                        break;
                }
                if (!String.IsNullOrWhiteSpace(m.Code)) property.Add("Code", m.Code);
                if (!String.IsNullOrWhiteSpace(m.Subject)) property.Add("Subject", m.Subject);

                property.Log();
            });
        }

        public void ThrowIfNecessary()
        {
            Messages translatedMessages = new Messages();

            this.ForEach((m) => translatedMessages.Add(m.LocalizeMessage()));

            if (WouldThrow())
                throw new AppException(translatedMessages);
        }

        public void LocalizeAll()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i] = this[i].LocalizeMessage();
            }
        }
    }
}

