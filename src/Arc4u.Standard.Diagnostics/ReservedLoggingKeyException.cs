namespace Arc4u.Diagnostics
{
    public class ReservedLoggingKeyException : Exception
    {
        public ReservedLoggingKeyException(string key) : base(key)
        {

        }
    }
}
