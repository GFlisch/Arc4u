using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Arc4u.Caching;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.DataProtection;

public class CacheStore : IXmlRepository
{
    public CacheStore(ICacheContext cacheContext, ILoggerFactory loggerFactory, [DisallowNull] string cacheKey, string? cacheName = null)
    {
        ArgumentNullException.ThrowIfNull(cacheContext);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(cacheKey);

        _logger = loggerFactory.CreateLogger<CacheStore>();
        _cacheContext = cacheContext;
        _cacheName = cacheName;
        _cacheKey = cacheKey;

        _cache = new Lazy<ICache>(() =>
        {
            // Check if I give a wrong cache name => Exception with clear context!
            return string.IsNullOrWhiteSpace(_cacheName) ? cacheContext.Default : cacheContext[_cacheName];
        });
    }

    private readonly ICacheContext _cacheContext;
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
