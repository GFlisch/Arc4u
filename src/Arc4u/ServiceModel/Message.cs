using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace Arc4u.ServiceModel;

/// <summary>
/// Represents a fault message.
/// </summary>
[DataContract]
[Serializable]
public sealed class Message : ICloneable
{
    private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

    /// <summary>
    /// Gets or sets the code of the current <see cref="Message"/> when applicable.
    /// Sometimes we should implement a code for an error following what the business ask.
    /// </summary>     
    [XmlElement(Order = 0)]
    [DataMember(EmitDefaultValue = false, Order = 0)]
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets the subject of the currendt <see cref="Message"/>.
    /// </summary>        
    [XmlElement(Order = 1)]
    [DataMember(EmitDefaultValue = false, Order = 1)]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the text of the current <see cref="Message"/>.
    /// </summary>
    [XmlElement(Order = 2)]
    [DataMember(EmitDefaultValue = false, Order = 2)]
    public string? Text { get; set; }

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
    internal string SerialisationCategory
    {
        get
        {
            return Category == MessageCategory.Technical ? "Undescribed" : "Described";
        }
        set
        {
            if (value.Equals("Described", StringComparison.InvariantCultureIgnoreCase))
            {
                Category = MessageCategory.Business;
            }
            else
            {
                Category = MessageCategory.Technical;
            }
        }
    }
    /// <summary>
    /// Converts the value of the current <see cref="Message"/> object to its equivalent string representation. 
    /// </summary>
    /// <returns>A string representation of the value of the current <see cref="Message"/> object.</returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        if (Code != null)
        {
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", Code);
        }

        if (Subject != null)
        {
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", Subject);
        }

        if (Text != null)
        {
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", Text);
        }

        //remove last delimiter
        if (builder.Length != 0)
        {
            builder.Remove(builder.Length - 1, 1);
        }
        else
        {
            return base.ToString() ?? string.Empty;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Add the stacktrace to receive the information on the messsage but not persist it (back to
    /// other app. This is only used when we want to log the message.
    /// </summary>
    [IgnoreDataMember]
    public string? StackTrace { get; set; }

    /// <summary>
    /// USefull to serialize with XmlSerialization.
    /// </summary>
    public Message()
        : this(MessageCategory.Technical, MessageType.Information, null, null, string.Empty, null)
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
    /// <param name="type">The message type of the message <see cref="MessageType"/></param>
    /// <param name="message">The expression returning the field we want to display from a resource file.</param>
    /// <param name="args">arguments of the message.</param>
    /// <remarks>Thecategory will be set to <see cref="MessageCategory.Business"/></remarks>
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
    public Message(MessageCategory category, MessageType type, string? code, string? subject, string? text, params object[]? args)
    {
        Code = code;
        Subject = subject;
        Text = null == args ? text : string.Format(CultureInfo.InvariantCulture, text ?? string.Empty, args);
        Type = type;
        Category = category;
    }

    public Message LocalizeMessage(CultureInfo? culture = null)
    {
        if (!string.IsNullOrWhiteSpace(Text) && Text!.StartsWith("{") && Text.EndsWith("}"))
        {
            var serializer = new DataContractJsonSerializer(typeof(LocalizedMessage));
            var msg = serializer.ReadObject<LocalizedMessage>(Text);

            if (msg != null && !string.IsNullOrEmpty(msg.Message) && msg.Type != null)
            {
                var textOut = GetResourceFromLocalizableMessage(msg, culture);
                if (msg.Parameters != null && msg.Parameters.Any() && !string.IsNullOrWhiteSpace(textOut))
                {
                    textOut = string.Format(CultureInfo.InvariantCulture, textOut, msg.Parameters);
                }

                // Clone the message so the MessageSource is the same!
                var clone = Clone() as Message;
                if (null != clone)
                {
                    clone.Text = textOut;
                    return clone;
                }

                return new Message(Category, Type, Code, Subject, textOut);
            }
        }
        return this;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
    /// <summary>
    /// Log a message and throw an exception if needed.
    /// </summary>
    /// <typeparam name="T">The class type used to log the information.</typeparam>
    /// <param name="level">Message level</param>
    /// <param name="message">The message.</param>
    /// <param name="stringFormatParameters">The parameters to inject into the message.</param>
    /// <param name="logger">Logger to log messages if necessary <see cref="LogAndThrowIfNecessary{T}(ILogger{T}, MessageType, Expression{Func{string}}, object[])"/></param>
    public static void LogAndThrowIfNecessary<T>(ILogger<T> logger, MessageType level, Expression<Func<string>> message, params object[] stringFormatParameters)
    {
        var messages = new Messages { new Message(level, message, stringFormatParameters) };
        messages.LogAndThrowIfNecessary(logger);
    }

    private static string GetResourceFromLocalizableMessage(LocalizedMessage msg, CultureInfo? culture)
    {
        var type = GetTypeFromstring(msg.Type);

        if (null == type)
        {
            throw new AppException($"The given type({msg.Type}) does not exist.");
        }

        // Check the resource file contains at least the property.
        var resourceManagerProp = type.GetProperty(msg.Message);

        if (resourceManagerProp == null ||
            resourceManagerProp.PropertyType != typeof(string))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The given type ({0}) does not have member {1}", type.FullName, msg.Message));
        }
        string textOut;
        // If we have a culture, use it to avoid a culture switching at the thread level which is heavier.
        if (null != culture)
        {
            resourceManagerProp = type.GetProperty("ResourceManager");

            var rm = resourceManagerProp?.GetValue(null) as ResourceManager;

            textOut = rm?.GetString(msg.Message, culture) ?? string.Empty;
        }
        else
        {
            textOut = resourceManagerProp?.GetValue(null)?.ToString() ?? string.Empty;
        }

        return textOut;
    }

    private static string ConvertToCode(Expression<Func<string>> message, params object[] stringFormatParameters)
    {
        var memberExpression = (MemberExpression)message.Body;
        var propertyInfo = (PropertyInfo)memberExpression.Member;
        var name = propertyInfo.Name;
        var type = string.Empty;

        if (propertyInfo.DeclaringType != null)
        {
            var typeName = propertyInfo.DeclaringType.FullName;
            var assemblyName = propertyInfo.DeclaringType.Assembly.GetName().Name;
            type = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", typeName, assemblyName);
        }

        var msg = new LocalizedMessage { Type = type, Message = name, Parameters = stringFormatParameters };

        var serializer = new DataContractJsonSerializer(typeof(LocalizedMessage));
        serializer.WriteObject(msg, out var result);

        return result;
    }

    private static Type GetTypeFromstring(string typeAsstring)
    {
        lock (TypeCache)
        {
            if (!TypeCache.TryGetValue(typeAsstring, out var type))
            {
                type = System.Type.GetType(typeAsstring);
                if (null != type)
                {
                    TypeCache.Add(typeAsstring, type);
                }
            }
            return type ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No type found:"));
        }
    }

}
