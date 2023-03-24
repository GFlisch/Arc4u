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

namespace Arc4u.OAuth2.DataProtection;

public class CacheStore : IXmlRepository
{
    public CacheStore(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, [DisallowNull] string cacheKey, string? cacheName)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        ArgumentNullException.ThrowIfNull(cacheKey, nameof(cacheKey));

        _logger = loggerFactory.CreateLogger<CacheStore>();
        _serviceProvider = serviceProvider;
        _cacheName = cacheName;
        _cacheKey = cacheKey;
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheStore> _logger;
    private readonly string _cacheKey;
    private readonly string? _cacheName;
    private ICache? _cache;



    public IReadOnlyCollection<XElement> GetAllElements()
    {
        // not thread safe but the cost of a lock regarding the singleton assignment?
        if (_cache is null)
        {
            var cacheContext = _serviceProvider.GetRequiredService<ICacheContext>();

            // Check if I give a wrong cache name => Exception with clear context!
            _cache = string.IsNullOrWhiteSpace(_cacheName) ? cacheContext.Default : cacheContext[_cacheName];
        }

        return GetElements().AsReadOnly();

    }

    private List<XElement> GetElements()
    {
        var result = new List<XElement>();

        var content = _cache!.Get<string>(_cacheKey);

        if (!string.IsNullOrWhiteSpace(content))
        {
            var list = JsonSerializer.Deserialize<List<string>>(content);
            result.AddRange(list.Select(e => XElement.Parse(e)));
        }

        return result;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        // not thread safe but the cost of a lock regarding the singleton assignment?
        if (_cache is null)
        {
            var cacheContext = _serviceProvider.GetRequiredService<ICacheContext>();

            // Check if I give a wrong cache name => Exception with clear context!
            _cache = string.IsNullOrWhiteSpace(_cacheName) ? cacheContext.Default : cacheContext[_cacheName];
        }

        var result = GetElements();

        result.Insert(0, element);

        var content = JsonSerializer.Serialize<List<string>>(result.Select(e => e.ToString(SaveOptions.DisableFormatting)).ToList());

        _cache.Put(_cacheKey, content);
    }
}
