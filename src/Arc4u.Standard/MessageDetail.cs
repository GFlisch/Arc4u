using Arc4u.ServiceModel;

namespace Arc4u;

/// <summary>
/// Return information back to the caller with only business content:
///  - validation of Data sent not correct.
///  Technical informationa are logged and a generic message with the activity id is sent! The target is to avoid to send detail that can be used by an hacker.
/// </summary>
public class MessageDetail
{
    private MessageDetail()
    {
        Message = string.Empty;
    }

    public MessageDetail(string message, MessageType type = MessageType.Error) : this(message, string.Empty, type)
    {
    }

    public MessageDetail(string message, string code, MessageType type = MessageType.Error)
    {
        Message = message;
        Code = code;
        Type = type;
    }

    public string Message { get; set; }

    public string? Code { get; set; }

    public MessageType Type { get; set; }

}
