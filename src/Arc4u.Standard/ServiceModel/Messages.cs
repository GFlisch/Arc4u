using Arc4u.Diagnostics;
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

        public void LogAndThrowIfNecessary(object This, [CallerMemberName] string methodName = "")
        {
            if (WouldThrow())
                throw new AppException(this);

            LogAll(This, methodName);

        }

        public void LogAndThrowIfNecessary(Type type, [CallerMemberName] string methodName = "")
        {
            if (WouldThrow())
                throw new AppException(this);

            LogAll(type, methodName);

        }

        public void LogAll(object This, [CallerMemberName] string methodName = "")
        {
            object _this = null != This ? This : this;
            LogAll(_this.GetType(), methodName);
        }

        /// <summary>
        /// Localized and log all messages.
        /// </summary>
        public void LogAll(Type type, [CallerMemberName] string methodName = "")
        {
            var _type = null == type ? this.GetType() : type;

            ForEach((m) =>
            {
                var lm = m.LocalizeMessage(Threading.Culture.Neutral);
                var logger = m.Category == MessageCategory.Business ? Logger.Business.From(_type, methodName) : Logger.Technical.From(_type, methodName);
                CommonLoggerProperties property = null;
                switch (m.Type)
                {
                    case MessageType.Information:
                        property = logger.Information(lm.Text);
                        break;
                    case MessageType.Warning:
                        property = logger.Warning(lm.Text);
                        break;
                    case MessageType.Error:
                        property = logger.Error(lm.Text);
                        break;
                    case MessageType.Critical:
                        property = logger.Fatal(lm.Text);
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

