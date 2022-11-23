using System;

namespace Arc4u.Standard.Ftp
{
    /// <summary>
    /// Configuration for connecting to a ftp server
    /// </summary>
    public interface IFtpConfiguration
    {
        /// <summary>
        /// Username to be used, when connecting to the ftp server
        /// </summary>
        string Username { get; set; }
        /// <summary>
        /// Password to be used, when connecting to the ftp server
        /// </summary>
        string Password { get; set; }
        /// <summary>
        /// Address of the ftp server
        /// </summary>
        /// <remarks>
        /// This is just the host, excluding the path or the protocol 
        /// </remarks>
        string Host { get; set; }
        /// <summary>The keep-alive interval.</summary>
        /// <value>
        /// The keep-alive interval. Specify negative one (-1) milliseconds to disable the
        /// keep-alive. This is the default value.
        /// </value>
        TimeSpan KeepAliveInterval { get; set; }    
    }
}