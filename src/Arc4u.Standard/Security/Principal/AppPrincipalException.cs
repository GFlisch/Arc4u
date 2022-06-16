using System;

namespace Arc4u.Security.Principal
{
    /// <summary>
    /// The AppPrincipalException is thrown when a problem occurs during the collection of information needed to have a valid principal.
    /// </summary>
    [Serializable]
    public class AppPrincipalException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppPrincipalException"/> class.
        /// </summary>
        public AppPrincipalException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPrincipalException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public AppPrincipalException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPrincipalException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The e.</param>
        public AppPrincipalException(string message, Exception e)
            : base(message, e)
        {

        }
    }
}
