using System;
using Arc4u.Extensions;
using Arc4u.Security.Cryptography;

namespace Arc4u.Standard.Ftp;

internal class FtpConfiguration : IFtpConfiguration
{
    /// <summary>
    ///     Clientsecret defined in provided configuration
    /// </summary>
    /// <remarks>The value will be read by the <see cref="Username" /> and <see cref="Password" /> properties</remarks>
    [Encrypted]
    private string ClientSecret { get; } = string.Empty;

    /// <inheritdoc />
    public string Username => ClientSecret.DetermineUsername();

    /// <inheritdoc />
    public string Password => ClientSecret.DeterminePassword();

    /// <inheritdoc />
    public string Host { get; set; } = string.Empty;

    /// <inheritdoc />
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(10);
}