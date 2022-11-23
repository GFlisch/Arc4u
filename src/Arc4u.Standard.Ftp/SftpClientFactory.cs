using Arc4u.Diagnostics;
using Arc4u.Network.Pooling;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Arc4u.Standard.Ftp
{
    /// <summary>
    /// Factory to create <see cref="PoolableItem"/> in form of an SftpCLient
    /// </summary>
    public class SftpClientFactory : IClientFactory<SftpClientFacade>
    {
        private readonly ILogger<SftpClientFactory> _logger;
        private readonly IFtpConfiguration _ftpConfig;
        
        /// <summary>
        /// Instantiates a new factory 
        /// </summary>
        /// <param name="ftpConfig">configuration to be used when creating new poolable sftp clients</param>
        /// <param name="logger">Logger for this factory</param>
        public SftpClientFactory (IFtpConfiguration ftpConfig, ILogger<SftpClientFactory> logger)
        {
            _ftpConfig = ftpConfig;
            _logger = logger;
        }

        /// <summary>
        /// Builds a new poolable sftp client, by using the provided configuration of the ctor
        /// </summary>
        /// <returns>poolable sftp client</returns>
        /// <remarks>
        /// The created internally created client is using a BasicAuth (username & password) and has a keep alive interval activated. The provided client is already connected to the sftp server
        /// </remarks>
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

