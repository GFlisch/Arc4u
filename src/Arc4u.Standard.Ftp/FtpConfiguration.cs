using System;

namespace Arc4u.Standard.Ftp;

internal class FtpConfiguration : IFtpConfiguration
{
    /// <inheritdoc />
    public string Username { get; set; } = String.Empty;
    /// <inheritdoc />
    public string Password { get; set; }= String.Empty;
    /// <inheritdoc />
    public string Host { get; set; }= String.Empty;
    /// <inheritdoc />
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(10);
}