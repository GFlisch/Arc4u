using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.AspNetCore;
using Arc4u.OAuth2.Security.Principal;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Extensions;
public static class CredentialResultExtension
{
    public static CredentialsResult ExtractCredential<T>(this CredentialsResult credentials, [DisallowNull] string pair, ILogger<T> logger, string? defaultUpn = null)
    {
        var ix = pair.IndexOf(':');
        if (ix == -1)
        {
            logger.Technical().LogBasicAuthentityBadFormat();

            return new CredentialsResult(false);
        }

        var username = pair.Substring(0, ix);
        var pwd = pair.Substring(ix + 1);

        logger.Technical().LogUserName(username);

        // is username format ok?
        if (!Regex.IsMatch(username, @"([a-zA-Z0-9]+@[a-zA-Z0-9]+\.[a-zA-Z0-9]+)|([a-zA-Z0-9]+\\[a-zA-Z0-9]+)|([a-zA-Z0-9]+/[a-zA-Z0-9]+)", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
        {
            var originalUserName = username;
            username = $"{username}{defaultUpn?.Trim()}";
            logger.Technical().LogChangeUserName(originalUserName, username);
        }

        return new CredentialsResult(true, username, pwd);
    }
}
