using System;
using System.Threading.Tasks;
using Arc4u.Network.Pooling;
using Arc4u.Standard.Ftp;

namespace Arc4u.Standard.UnitTest.Ftp
{
    public class SftpClientFactoryMock : IClientFactory<SftpClientFacade>
    {
        
        public virtual SftpClientFacade CreateClient(Func<SftpClientFacade, Task> releaseFunc)
        {
            return null;
        }
    }
}
