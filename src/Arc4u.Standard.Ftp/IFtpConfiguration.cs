using System;

namespace Arc4u.Standard.Ftp
{
    public interface IFtpConfiguration
    {
        string Username { get; set; }
        string Password { get; set; }
        string Host { get; set; }
        TimeSpan KeepAliveInterval { get; set; }    
    }
}