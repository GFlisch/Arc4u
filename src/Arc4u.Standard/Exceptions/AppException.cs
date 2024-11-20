using System.Text;
using Arc4u.ServiceModel;
using Newtonsoft.Json;

namespace Arc4u;

/// <summary>
/// Represents an application exception.
/// <para>This class is not intented to be used client-side.</para>
/// </summary>
/// <remarks></remarks>
public class AppException : Exception
{

    /// <summary>
    /// Gets or sets the <see cref="Message"/> list of the current <see cref="AppException"/>.
    /// </summary>
    public List<Message> Messages { get; set; }

    private static string ToString(IEnumerable<Message> messages)
    {
        //consider argument
        if (messages == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (Message message in messages)
        {
            builder.AppendLine(message.ToString());
        }

        //remove last Environment.NewLine
        if (builder.Length != 0)
        {
            builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public AppException(Message message)
        : this([message])
    {
    }

    public AppException(Message message, Exception innerException)
        : this([message], innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class.
    /// </summary>
    /// <param name="messages">The messages.</param>
    public AppException(IEnumerable<Message> messages)
        : base(ToString(messages))
    {
        Messages = (messages != null)
            ? new List<Message>(messages)
            : [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class.
    /// </summary>
    /// <param name="messages">The messages.</param>
    public AppException(IEnumerable<Message> messages, Exception innerException)
        : base(ToString(messages), innerException)
    {
        Messages = (messages != null)
            ? new List<Message>(messages)
            : [];
    }

    public AppException(string text)
        : this(new Message(text))
    {

    }

    public AppException(string text, Exception innerException)
        : this(new Message(text), innerException)
    {

    }

    private static void ProcessAppExceptionContent(string content)
    {
        IEnumerable<Message> messages;
        try
        {
            messages = JsonConvert.DeserializeObject<IEnumerable<Message>>(content) ?? [];
        }
        catch (Exception)
        {
            // transform the old format to new one.
            content = content.Replace("\"Category\":\"Described\"", "\"Category\":\"Business\"");
            content = content.Replace("\"Category\":\"Undescribed\"", "\"Category\":\"Technical\"");
            messages = JsonConvert.DeserializeObject<IEnumerable<Message>>(content) ?? [];
        }

        throw new AppException(messages);
    }

    public static async Task ProcessAppExceptionAsync(HttpResponseMessage exception)
    {
        var content = await exception.Content.ReadAsStringAsync().ConfigureAwait(false);
        ProcessAppExceptionContent(content);
    }

    public static void ProcessAppException(HttpResponseMessage exception)
    {
        var content = exception.Content.ReadAsStringAsync().Result;
        ProcessAppExceptionContent(content);
    }
}

