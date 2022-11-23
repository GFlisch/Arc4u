using System;
using Arc4u.Diagnostics;
using Arc4u.Network.Pooling;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Arc4u.Standard.Ftp
{
    public class SftpClientFactory : IClientFactory<SftpClientFacade>
    {
        private readonly ILogger<SftpClientFactory> _logger;
        private readonly IFtpConfiguration _ftpConfig;

        public SftpClientFactory (IFtpConfiguration ftpConfig, ILogger<SftpClientFactory> logger)
        {
            _ftpConfig = ftpConfig;
            _logger = logger;
        }

        public SftpClientFacade CreateClient()
        {
            var c = new SftpClient(new ConnectionInfo(_ftpConfig.Host, _ftpConfig.Username, new PasswordAuthenticationMethod(_ftpConfig.Username, _ftpConfig.Password)));
            c.KeepAliveInterval = _ftpConfig.KeepAliveInterval;
            _logger.Technical().Debug("Connecting to SFTP host...").Log();
            c.Connect();
            _logger.Technical().Debug("Connected.").Log();
            return new SftpClientFacade(c);
        }

    }
}

