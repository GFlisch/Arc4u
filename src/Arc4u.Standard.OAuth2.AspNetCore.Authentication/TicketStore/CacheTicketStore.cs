using System;
using System.Threading.Tasks;
using Arc4u.Caching;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TicketStore;

public class CacheTicketStore : ITicketStore
{
    public CacheTicketStore(ILogger<CacheTicketStore> logger, ICacheContext cacheContext, IOptionsMonitor<CacheTicketStoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options.CurrentValue.KeyPrefix);
        ArgumentNullException.ThrowIfNull(options.CurrentValue.CacheName);

        _logger = logger;
        _cacheContext = cacheContext;
        _cache = GetCache(options.CurrentValue.CacheName);
        _keyPrefix = options.CurrentValue.KeyPrefix;
    }

    private readonly ILogger<CacheTicketStore> _logger;
    private readonly ICacheContext _cacheContext;
    private readonly ICache _cache;
    private readonly string _keyPrefix;

    private ICache GetCache(string key)
    {
        if (_cacheContext.Exist(key))
        {
            _logger.Technical().LogDebug($"Cache with name {key} is used for the Authentication tickets.");
            return _cacheContext[key];
        }

        _logger.Technical().LogWarning($"Cache with name {key} doesn't exist, fallback on the default one for the Authentication tickets!");
        return _cacheContext.Default;
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key).ConfigureAwait(false);

        _logger.Technical().LogDebug($"Authentication ticket with key {key} has been deleted.");
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var expiresUtc = ticket.Properties.ExpiresUtc;
        if (expiresUtc.HasValue)
        {
            var time = expiresUtc - DateTime.UtcNow ?? TimeSpan.FromHours(4);
            await _cache.PutAsync<byte[]>(key, time, TicketSerializer.Default.Serialize(ticket)).ConfigureAwait(false);
            _logger.Technical().LogDebug($"Authentication ticket with key {key} has been created with validity period of {{timeSpan}}.", time);
            return;
        }
        await _cache.PutAsync(key, TimeSpan.FromHours(4), TicketSerializer.Default.Serialize(ticket)).ConfigureAwait(false);
        _logger.Technical().LogDebug($"Authentication ticket with key {key} has been created.");

    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        try
        {
            var content = await _cache.GetAsync<byte[]>(key).ConfigureAwait(false);

            var ticket = TicketSerializer.Default.Deserialize(content);
            if (ticket is null)
            {
                _logger.Technical().LogError($"Authentication ticket from the cache is null!");
            }

            return ticket;
        }
        catch (DataCacheException)
        {
            _logger.Technical().LogError($"No Authentication ticket from the cache.");

            return null;
        }
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();
        await RenewAsync(key, ticket).ConfigureAwait(false);

        return key;
    }
}
