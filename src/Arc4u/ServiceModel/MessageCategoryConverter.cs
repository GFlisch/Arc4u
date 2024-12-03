using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Arc4u.ServiceModel;

/// <summary>
/// This class is used with json to Deserialize a <see cref="Message"/> class and change the Category from a 
/// <see cref="string"/> to a <see cref="MessageCategory"/> type.
/// </summary>
public class MessageCategoryConverter : CustomCreationConverter<Message>
{
    public override Message Create(Type objectType)
    {
        return new Message();
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var code = string.Empty;
        var text = string.Empty;
        var subject = string.Empty;
        var category = MessageCategory.Technical;
        var type = MessageType.Information;

        while (reader?.Read() ?? false)
        {
            var tokenType = reader.TokenType;

            if (tokenType == JsonToken.PropertyName)
            {
                var propertyName = reader?.Value?.ToString();

                if (null == propertyName)
                {
                    continue;
                }

                // Key/value pattern.

                if (propertyName.Equals("code", StringComparison.CurrentCultureIgnoreCase))
                {
                    code = reader?.ReadAsString();
                }
                else if (propertyName.Equals("text", StringComparison.CurrentCultureIgnoreCase))
                {
                    text = reader?.ReadAsString();
                }
                else if (propertyName.Equals("subject", StringComparison.CurrentCultureIgnoreCase))
                {
                    subject = reader?.ReadAsString();
                }
                else if (propertyName.Equals("category", StringComparison.CurrentCultureIgnoreCase))
                {
                    var categoryValue = reader?.ReadAsString();
                    if (categoryValue != null)
                    {
                        category = categoryValue.Equals("described", StringComparison.CurrentCultureIgnoreCase) ? MessageCategory.Business : MessageCategory.Technical;
                    }
                }
                else if (propertyName.Equals("type", StringComparison.CurrentCultureIgnoreCase))
                {
                    type = (MessageType?)reader?.ReadAsInt32() ?? MessageType.Information;
                }
            }

            if (tokenType == JsonToken.EndObject)
            {
                return new Message(category, type, code, subject, text);
            }
        }

        return new Message();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(serializer);

        if (value is not Message message)
        {
            base.WriteJson(writer, value, serializer);
            return;
        }

        // Serialize the message object.           
        writer.WriteStartObject();

        if (!string.IsNullOrWhiteSpace(message.Code))
        {
            writer.WriteToken(JsonToken.PropertyName, "Code");
            writer.WriteValue(message.Code);
        }

        if (!string.IsNullOrWhiteSpace(message.Subject))
        {
            writer.WriteToken(JsonToken.PropertyName, "Subject");
            writer.WriteValue(message.Subject);
        }

        if (!string.IsNullOrWhiteSpace(message.Text))
        {
            writer.WriteToken(JsonToken.PropertyName, "Text");
            writer.WriteValue(message.Text);
        }

        writer.WriteToken(JsonToken.PropertyName, "Type");
        writer.WriteValue(message.Type);

        writer.WriteToken(JsonToken.PropertyName, "Category");
        writer.WriteValue(message.Category == MessageCategory.Business ? "Described" : "Undescribed");

        writer.WriteEndObject();
    }

}
