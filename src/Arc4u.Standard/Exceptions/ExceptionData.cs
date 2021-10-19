namespace Arc4u.Exceptions
{
    /// <summary>
    /// Used to serialize an exception in Rest.
    /// </summary>
    public class ExceptionData
    {
        public string message { get; set; }
        public string exceptionMessage { get; set; }
        public string exceptionType { get; set; }
        public string stackTrace { get; set; }
        public ExceptionData innerException { get; set; }
    }
}
