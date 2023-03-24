using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using Arc4u.Caching;
using System.Text.Json;
using Arc4u.Diagnostics;

namespace Arc4u.OAuth2.DataProtection;

public class CacheStore : IXmlRepository
{
    public CacheStore(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, [DisallowNull] string cacheKey, string? cacheName = null)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        ArgumentNullException.ThrowIfNull(cacheKey, nameof(cacheKey));

        _logger = loggerFactory.CreateLogger<CacheStore>();
        _serviceProvider = serviceProvider;
        _cacheName = cacheName;
        _cacheKey = cacheKey;

        _cache = new Lazy<ICache>(() =>
        {
            var cacheContext = _serviceProvider.GetRequiredService<ICacheContext>();

            // Check if I give a wrong cache name => Exception with clear context!
            return string.IsNullOrWhiteSpace(_cacheName) ? cacheContext.Default : cacheContext[_cacheName];
        });
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheStore> _logger;
    private readonly string _cacheKey;
    private readonly string? _cacheName;
    private readonly Lazy<ICache> _cache;

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return GetElements().AsReadOnly();
    }

    private List<XElement> GetElements()
    {
        var result = new List<XElement>();

        var content = _cache.Value.Get<string>(_cacheKey);

        if (!string.IsNullOrWhiteSpace(content))
        {
            var list = JsonSerializer.Deserialize<List<string>>(content);

            if (list is null)
            {
                _logger.Technical().LogError("The deserialization of the Data Protection xml element failed.");
                return result;
            }

            result.AddRange(list.Where(e => !string.IsNullOrWhiteSpace(e)).Select(XElement.Parse));
        }

        return result;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var result = GetElements();

        result.Insert(0, element);

        var content = JsonSerializer.Serialize<List<string>>(result.Select(e => e.ToString(SaveOptions.DisableFormatting)).ToList());

        _cache.Value.Put(_cacheKey, content);
    }
}
