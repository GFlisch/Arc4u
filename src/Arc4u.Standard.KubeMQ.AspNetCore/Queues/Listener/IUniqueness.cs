using Arc4u.KubeMQ.AspNetCore.Configuration;
using System.Collections.Generic;

namespace Arc4u.KubeMQ.AspNetCore.Queues
{
    public interface IUniqueness
    {
        IEnumerable<QueueParameters> GetQueues();
    }
}
