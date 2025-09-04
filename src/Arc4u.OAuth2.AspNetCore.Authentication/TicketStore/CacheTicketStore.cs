using Arc4u.Caching;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TicketStore
{
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
                _logger.Technical().LogCacheNameUsed(key);
                return _cacheContext[key];
            }

            _logger.Technical().LogNoCacheExist(key);
            return _cacheContext.Default;
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key).ConfigureAwait(false);

            _logger.Technical().LogDeleteAuthenticationTicket(key);
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                var time = expiresUtc - DateTime.UtcNow ?? TimeSpan.FromHours(4);
                await _cache.PutAsync<byte[]>(key, time, TicketSerializer.Default.Serialize(ticket)).ConfigureAwait(false);
                _logger.Technical().LogCreateAuthenticationTicket(key, time);
                return;
            }
            await _cache.PutAsync(key, TimeSpan.FromHours(4), TicketSerializer.Default.Serialize(ticket)).ConfigureAwait(false);
            _logger.Technical().LogAuthenticationTicketCreated(key);

        }

        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            try
            {
                var content = await _cache.GetAsync<byte[]>(key).ConfigureAwait(false);

                if (content is null)
                {
                    _logger.Technical().LogError($"No Authentication ticket from the cache with key {key}.");
                    return null;
                }

                var ticket = TicketSerializer.Default.Deserialize(content);
                if (ticket is null)
                {
                    _logger.Technical().LogAuthenticationTicketIsNull();
                }

                return ticket;
            }
            catch (DataCacheException)
            {
                _logger.Technical().LogNoAuthenticationTicketFromCache();

                return null;
            }
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = _keyPrefix + Guid.NewGuid().ToString();
            await RenewAsync(key, ticket).ConfigureAwait(false);

            return key;
        }
    }
}
