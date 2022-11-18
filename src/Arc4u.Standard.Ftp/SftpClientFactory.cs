using Arc4u.Diagnostics;
using Arc4u.Network.Pooling;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Arc4u.Standard.Ftp
{
    public class SftpClientFactory : IClientFactory<SftpClientFacade>
    {
        private readonly ILogger<SftpClientFactory> _logger;
        private readonly IFtpConfiguration _mftConfig;

        public SftpClientFactory (IFtpConfiguration mftConfig, ILogger<SftpClientFactory> logger)
        {
            _mftConfig = mftConfig;
            _logger = logger;
        }

        public SftpClientFacade CreateClient()
        {
            var c = new SftpClient(new ConnectionInfo(_mftConfig.Host, _mftConfig.Username, new PasswordAuthenticationMethod(_mftConfig.Username, _mftConfig.Password)));
            c.KeepAliveInterval = _mftConfig.KeepAliveInterval;
            _logger.Technical().Debug("Connecting to SFTP host...").Log();
            c.Connect();
            _logger.Technical().Debug("Connected.").Log();
            return new SftpClientFacade(c);
        }
    }
}

