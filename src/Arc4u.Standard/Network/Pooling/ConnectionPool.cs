using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.Network.Pooling
{
    public class ConnectionPool<T> : IDisposable where T : PoolableItem
    {
        private readonly IClientFactory<T> _clientFactory;
        private readonly ILogger<ConnectionPool<T>> _logger;
        private readonly ConcurrentBag<T> _standbyClients = new();

        public ConnectionPool(ILogger<ConnectionPool<T>> logger, IClientFactory<T> clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public T GetClient()
        {
            return GetNextClient();
        }

        public Task ReleaseClient(T? client)
        {
            if (client != null && client.IsActive && !client.IsPooled)
            {
                _standbyClients.Add(client);
                client.IsPooled = true;
                _logger.Technical().Debug("Release client to pool.").Log();
            }
            return Task.CompletedTask;
        }

        public int ConnectionsCount =>_standbyClients.Count;
            

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            while (!_standbyClients.IsEmpty)
            {
                if (_standbyClients.TryTake(out T? client))
                {
                    client.Dispose();
                }
            }
        }

        private T GetNextClient()
        {
            while (!_standbyClients.IsEmpty)
            {
                if (_standbyClients.TryTake(out T? client))
                {
                    if (client.IsActive)
                    {
                        _logger.Technical().Debug("Get client from pool.").Log();
                        client.IsPooled = false;
                        return client;
                    }
                    client.Dispose();
                }
            }
            var newClient = _clientFactory.CreateClient();
            newClient.ReleaseClient = client => ReleaseClient((T)client);
            _logger.Technical().Debug("Create new client.").Log();
            return newClient;
        }
    }
}
