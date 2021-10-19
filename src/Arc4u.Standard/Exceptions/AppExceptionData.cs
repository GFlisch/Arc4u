using Arc4u.ServiceModel;

namespace Arc4u.Exceptions
{
    /// <summary>
    /// Collection of messages to return in Rest calls.
    /// </summary>
    public class AppExceptionData : ExceptionData
    {
        public Messages messages { get; set; }
    }
}
