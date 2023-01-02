#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.Network.Pooling;

/// <summary>
///     Pool of connection objects of the defined type.
/// </summary>
/// <typeparam name="T">Connection type, that should be pooled</typeparam>
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

    /// <summary>
    ///     Current number of clients in the pool
    /// </summary>
    public int ConnectionsCount => _standbyClients.Count(item => item.IsActive);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Take a client out of the pool
    /// </summary>
    /// <returns>requested object</returns>
    /// <remarks>
    ///     The return object is active and exclusive for the requester, meaning the pool does not have access to that object
    ///     anymore. It will be made available again, once the object is being disposed
    /// </remarks>
    public T GetClient()
    {
        return GetNextClient();
    }

    private bool IsPooled(T client)
    {
        return _standbyClients.Any(pooledItem => ReferenceEquals(pooledItem, client));
    }

    /// <summary>
    ///     Releasing the provided client and putting it back to the pool
    /// </summary>
    /// <param name="client">Client that should be released and put back into the pool</param>
    /// <remarks>
    ///     The client will be put back into the pool, if it is still active and not in the pool yet
    /// </remarks>
    private Task ReleaseClient(T? client)
    {
        if (client != null && client.IsActive && !IsPooled(client))
        {
            _standbyClients.Add(client);
            _logger.Technical().Debug("Release client to pool.").Log();
        }

        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        foreach (var item in _standbyClients.ToList())
        {
            item.Dispose();
        }

        while (!_standbyClients.IsEmpty)
        {
            if (_standbyClients.TryTake(out _))
            {
            }
        }
    }

    private T GetNextClient()
    {
        while (!_standbyClients.IsEmpty)
            if (_standbyClients.TryTake(out var client))
            {
                if (client.IsActive)
                {
                    _logger.Technical().Debug("Get client from pool.").Log();
                    return client;
                }

                client.Dispose();
            }

        var newClient = _clientFactory.CreateClient(ReleaseClient);
        _logger.Technical().Debug("Create new client.").Log();
        return newClient;
    }
}
#nullable restore