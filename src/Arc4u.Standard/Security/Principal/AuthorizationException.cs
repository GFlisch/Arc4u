namespace Arc4u.Security.Principal
{
    /// <summary>
    /// The exception warn a problem to retrieve the authorization data used to check the rights on a program.
    /// </summary>
    public class AuthorizationException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> class.
        /// </summary>
        public AuthorizationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public AuthorizationException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public AuthorizationException(string message, Exception exception) : base(message, exception)
        {

        }
    }
}
