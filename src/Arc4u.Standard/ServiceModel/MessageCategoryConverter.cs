using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arc4u.ServiceModel
{
    /// <summary>
    /// This class is used with json to Deserialize a <see cref="Message"/> class and change the Category from a 
    /// <see cref="String"/> to a <see cref="MessageCategory"/> type.
    /// </summary>
    public class MessageCategoryConverter : JsonConverter<Message>
    {
        /// <summary>
        /// This is what the serialized message looks like
        /// </summary>
        private sealed class OldMessage
        {
            private const string DescribedCategory = "Described";
            private const string UndescribedCategory = "Undescribed";

            public string Code { get; set; }
            public string Text { get; set; }
            public string Subject { get; set; }
            public string Category { get; set; }
            public int Type { get; set; }

            /// <summary>
            /// Default constructor for serialization
            /// </summary>
            public OldMessage()
            {
            }

            /// <summary>
            /// Conversion constructor
            /// </summary>
            /// <param name="message"></param>
            public OldMessage(Message message)
            {
                Code = message.Code;
                Text = message.Text;
                Subject = message.Subject;
                Category = message.Category == MessageCategory.Business ? DescribedCategory : UndescribedCategory;
                Type = (int)message.Type;
            }

            /// <summary>
            /// Conversion operator for the actual message formar
            /// </summary>
            /// <returns></returns>
            public Message ToMessage()
            {
                return new Message(StringComparer.OrdinalIgnoreCase.Equals(Category, DescribedCategory) ? MessageCategory.Business : MessageCategory.Technical, (MessageType)Type, Code, Subject, Text);
            }
        }

        public override Message Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var oldMessage = JsonSerializer.Deserialize<OldMessage>(ref reader, options);
            // the previous implementation always returned a non-null message which is dangerous.
            return oldMessage?.ToMessage();
        }

        public override void Write(Utf8JsonWriter writer, Message message, JsonSerializerOptions options)
        {
            var oldMessage = message is null ? null : new OldMessage(message);
            JsonSerializer.Serialize(writer, oldMessage, options);
        }
    }
}
