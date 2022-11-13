using Arc4u.KubeMQ.AspNetCore.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Arc4u.KubeMQ.AspNetCore.Queues
{
    public class DefaultUniqueness : IUniqueness
    {
        public DefaultUniqueness(IEnumerable<QueueParameters> queueParameters)
        {
            _queueParameters = queueParameters;
        }

        private readonly IEnumerable<QueueParameters> _queueParameters;

        public IEnumerable<QueueParameters> GetQueues()
        {
            return _queueParameters.Distinct(QueueParametersEqualityComparer.Default);
        }
    }

    public class QueueParametersEqualityComparer: IEqualityComparer<QueueParameters>
    {
        public static readonly QueueParametersEqualityComparer Default = new();

        public bool Equals(QueueParameters x, QueueParameters y)
        {
            if (x == null || y == null)
                return x == null && y == null;
            return x.Namespace.Equals(y.Namespace, StringComparison.InvariantCultureIgnoreCase) &&
                   x.Address.Equals(y.Address, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode([DisallowNull] QueueParameters obj)
        {
            return obj.GetHashCode();
        }
    }
}
