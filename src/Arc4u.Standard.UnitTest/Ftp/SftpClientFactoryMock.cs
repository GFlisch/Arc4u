using Arc4u.Network.Pooling;
using Arc4u.Standard.Ftp;

namespace Arc4u.Standard.UnitTest.Ftp
{
    public class SftpClientFactoryMock : IClientFactory<SftpClientFacade>
    {
        public SftpClientFactoryMock()
        {
        }

        public virtual SftpClientFacade CreateClient()
        {
            return null;
        }
    }
}
