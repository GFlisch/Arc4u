namespace Arc4u.Exceptions;

/// <summary>
/// Used to serialize an exception in Rest.
/// </summary>
public class ExceptionData
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string message { get; set; }
    public string exceptionMessage { get; set; }
    public string exceptionType { get; set; }
    public string stackTrace { get; set; }
    public ExceptionData innerException { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
