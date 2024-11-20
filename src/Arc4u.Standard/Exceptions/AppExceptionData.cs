using Arc4u.ServiceModel;

namespace Arc4u.Exceptions;

/// <summary>
/// Collection of messages to return in Rest calls.
/// </summary>
public class AppExceptionData : ExceptionData
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Messages messages { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
