using Arc4u.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Arc4u.ServiceModel
{
    /// <summary>
    /// Represents a fault message.
    /// </summary>
    [DataContract]
    [Serializable]
    public sealed class Message : ICloneable
    {
        private static readonly Dictionary<string, Type> TypeCache;

        static Message()
        {
            TypeCache = new Dictionary<string, Type>();
        }


        /// <summary>
        /// Gets or sets the code of the current <see cref="Message"/> when applicable.
        /// Sometimes we should implement a code for an error following what the business ask.
        /// </summary>     
        [XmlElement(Order = 0)]
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the subject of the currendt <see cref="Message"/>.
        /// </summary>        
        [XmlElement(Order = 1)]
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public String Subject { get; set; }

        /// <summary>
        /// Gets or sets the text of the current <see cref="Message"/>.
        /// </summary>
        [XmlElement(Order = 2)]
        [DataMember(EmitDefaultValue = false, Order = 2)]
        public String Text { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MessageType"/> of the current <see cref="Message"/>.
        /// </summary>
        [XmlElement(Order = 3)]
        [DataMember(EmitDefaultValue = true, Order = 3)]
        public MessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MessageCategory"/> of the current <see cref="Message"/>.
        /// </summary>
        [XmlElement(ElementName = "Category", Order = 4)]
        public MessageCategory Category { get; set; }

        /// <summary>
        /// Used for compatibility with previous version of Arc4u. Technical and Business is new way but the previous category was Described and Undescribed!
        /// So a transformation is done Technical == Undescribed, Business == Described.
        /// </summary>

        [DataMember(Name = "Category", EmitDefaultValue = false, Order = 4)]
        internal String SerialisationCategory
        {
            get
            {
                return Category == MessageCategory.Technical ? "Undescribed" : "Described";
            }
            set
            {
                if (value.Equals("Described", StringComparison.InvariantCultureIgnoreCase))
                    Category = MessageCategory.Business;
                else
                    Category = MessageCategory.Technical;
            }
        }
        /// <summary>
        /// Converts the value of the current <see cref="Message"/> object to its equivalent string representation. 
        /// </summary>
        /// <returns>A string representation of the value of the current <see cref="Message"/> object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Code != null) builder.AppendFormat("{0} ", Code);
            if (Subject != null) builder.AppendFormat("{0} ", Subject);
            if (Text != null) builder.AppendFormat("{0} ", Text);

            //remove last delimiter
            if (builder.Length != 0)
                builder.Remove(builder.Length - 1, 1);
            else
                return base.ToString();

            return builder.ToString();
        }


        /// <summary>
        /// Add the stacktrace to receive the information on the messsage but not persist it (back to
        /// other app. This is only used when we want to log the message.
        /// </summary>
        [IgnoreDataMember]
        public String StackTrace { get; set; }

        /// <summary>
        /// USefull to serialize with XmlSerialization.
        /// </summary>
        public Message()
            : this(MessageCategory.Technical, MessageType.Information, null, null, String.Empty, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public Message(string text)
            : this(MessageCategory.Technical, MessageType.Information, null, null, text, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="args">The arguments associated to the text.</param>
        public Message(string text, params object[] args)
            : this(MessageCategory.Technical, MessageType.Information, null, null, text, args)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="category">The category of the message <see cref="MessageCategory"/></param>
        /// <param name="type">The message type of the message <see cref="MessageType"/></param>
        /// <param name="text">The message himself.</param>
        public Message(MessageCategory category, MessageType type, string text)
            : this(category, type, null, null, text, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="category">The category of the message <see cref="MessageCategory"/></param>
        /// <param name="type">The message type of the message <see cref="MessageType"/></param>
        /// <param name="text">The message himself.</param>
        /// <param name="args">arguments of the message.</param>
        public Message(MessageCategory category, MessageType type, string text, params object[] args)
            : this(category, type, null, null, text, args)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.The Category is Business
        /// </summary>
        /// <param name="category">The category of the message <see cref="MessageCategory"/></param>
        /// <param name="type">The message type of the message <see cref="MessageType"/></param>
        /// <param name="message">The expression returning the field we want to display from a resource file.</param>
        /// <param name="args">arguments of the message.</param>
        public Message(MessageType type, Expression<Func<string>> message, params object[] args)
            : this(MessageCategory.Business, type, ConvertToCode(message, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="category">The category of the message <see cref="MessageCategory"/></param>
        /// <param name="type">The message type of the message <see cref="MessageType"/></param>
        /// <param name="message">The expression returning the field we want to display from a resource file.</param>
        /// <param name="args">arguments of the message.</param>
        public Message(MessageCategory category, MessageType type, Expression<Func<string>> message, params object[] args)
            : this(category, type, ConvertToCode(message, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="category">The category of the message <see cref="MessageCategory"/></param>
        /// <param name="type">The message type of the message <see cref="MessageType"/></param>
        /// <param name="code">A code of a message which can be used to display a localized text.</param>
        /// <param name="subject">A subject.</param>
        /// <param name="text">The message himself.</param>
        /// <param name="args">arguments of the message.</param>
        public Message(MessageCategory category, MessageType type, string code, string subject, string text, params object[] args)
        {
            Code = code;
            Subject = subject;
            Text = null == args ? text : String.Format(text, args);
            Type = type;
            Category = category;
        }



        public Message LocalizeMessage(CultureInfo culture = null)
        {
            if (this.Text.StartsWith("{") && this.Text.EndsWith("}"))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(LocalizedMessage));
                    var msg = serializer.ReadObject<LocalizedMessage>(Text);

                    if (msg != null && !String.IsNullOrEmpty(msg.Message) && msg.Type != null)
                    {
                        var textOut = GetResourceFromLocalizableMessage(msg, culture);
                        if (msg.Parameters != null && msg.Parameters.Any() && !String.IsNullOrWhiteSpace(textOut))
                        {
                            textOut = String.Format(textOut, msg.Parameters);
                        }

                        // Clone the message so the MessageSource is the same!
                        var clone = this.Clone() as Message;
                        if (null != clone)
                        {
                            clone.Text = textOut;
                            return clone;
                        }

                        return new Message(this.Category, this.Type, this.Code, this.Subject, textOut);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Technical.From<Message>().Exception(exception).Log();
                }
            }
            return this;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        /// <summary>
        /// Log a message and throw an exception if needed.
        /// </summary>
        /// <typeparam name="T">The class type used to log the information.</typeparam>
        /// <param name="level">Message level</param>
        /// <param name="message">The message.</param>
        /// <param name="stringFormatParameters">The parameters to inject into the message.</param>
        public static void LogAndThrowIfNecessary<T>(MessageType level, Expression<Func<string>> message, params object[] stringFormatParameters)
        {
            var messages = new Messages { new Message(level, message, stringFormatParameters) };
            messages.LogAndThrowIfNecessary(typeof(T));
        }


        private static string GetResourceFromLocalizableMessage(LocalizedMessage msg, CultureInfo culture)
        {
            var type = GetTypeFromString(msg.Type);

            if (null == type)
                throw new ApplicationException($"The given type({type.FullName}) does not exist.");

            // Check the resource file contains at least the property.
            var resourceManagerProp = type.GetProperty(msg.Message);

            if (resourceManagerProp == null ||
                resourceManagerProp.PropertyType != typeof(string))
            {
                throw new InvalidOperationException(
                    String.Format(" The given type ({0}) does not have member {1}", type.FullName, msg.Message));
            }
            string textOut = String.Empty;
            // If we have a culture, use it to avoid a culture switching at the thread level which is heavier.
            if (null != culture)
            {
                resourceManagerProp = type.GetProperty("ResourceManager");

                var rm = resourceManagerProp.GetValue(null) as ResourceManager;

                textOut = rm.GetString(msg.Message, culture);
            }
            else
                textOut = resourceManagerProp.GetValue(null).ToString();
            return textOut;
        }

        private static string ConvertToCode(Expression<Func<string>> message, params object[] stringFormatParameters)
        {
            var memberExpression = (MemberExpression)message.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var name = propertyInfo.Name;
            string type = null;
            if (propertyInfo.DeclaringType != null)
            {
                var typeName = propertyInfo.DeclaringType.FullName;
                var assemblyName = propertyInfo.DeclaringType.Assembly.GetName().Name;
                type = String.Format("{0}, {1}", typeName, assemblyName);
            }

            var msg = new LocalizedMessage { Type = type, Message = name, Parameters = stringFormatParameters };

            string result;
            var serializer = new DataContractJsonSerializer(typeof(LocalizedMessage));
            serializer.WriteObject(msg, out result);

            return result;
        }

        private static Type GetTypeFromString(string typeAsString)
        {
            try
            {
                lock (TypeCache)
                {
                    if (!TypeCache.ContainsKey(typeAsString))
                    {
                        TypeCache.Add(typeAsString, System.Type.GetType(typeAsString));
                    }
                    return TypeCache[typeAsString];
                }
            }
            catch (Exception ex)
            {
                Logger.Technical.From<Message>().Exception(ex).Log();
                return null;
            }
        }
    }
}
