using System;

namespace Arc4u.Standard.Ftp;

internal class FtpConfiguration : IFtpConfiguration
{
    public string Username { get; set; } = String.Empty;
    public string Password { get; set; }= String.Empty;
    public string Host { get; set; }= String.Empty;
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(10);
}