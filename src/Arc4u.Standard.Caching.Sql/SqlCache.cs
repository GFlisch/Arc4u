using Arc4u.Configuration.Sql;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Arc4u.Caching.Sql;

/// <summary>
/// See Documentation how to create a database here: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.0
/// </summary>
[Export("Sql", typeof(ICache))]
public class SqlCache : BaseDistributeCache, ICache
{
    private string? Name { get; set; }

    private readonly ILogger<SqlCache> _logger;
    /// <summary>
    /// This is a named options => the configuration named instance for this instance will be taken during the intialize method. 
    /// </summary>
    private readonly IOptionsMonitor<SqlCacheOption> _options;

    public SqlCache(ILogger<SqlCache> logger, IContainerResolve container, IOptionsMonitor<SqlCacheOption> options) : base(container)
    {
        _logger = logger;
        _options = options;
    }

    public override void Initialize([DisallowNull] string store)
    {
#if NET6_0
        ArgumentNullException.ThrowIfNull(store, nameof(store));
#endif
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNullOrEmpty(store, nameof(store));
#endif

        lock (_lock)
        {
            if (IsInitialized)
            {
                _logger.Technical().System($"Sql Cache {store} is already initialized.").Log();
                return;
            }

            Name = store;

            var config = _options.Get(store);

            var option = new SqlServerCacheOptions
            {
                ConnectionString = config.ConnectionString,
                TableName = config.TableName,
                SchemaName = config.SchemaName
            };

            DistributeCache = new SqlServerCache(option);

            if (!Container.TryResolve<IObjectSerialization>(config.SerializerName, out var serializerFactory))
            {
                SerializerFactory = Container.Resolve<IObjectSerialization>();
            }
            else
            {
                SerializerFactory = serializerFactory;
            }

            IsInitialized = true;
            _logger.Technical().System($"Sql Cache {store} is initialized.").Log();
        }
    }
    public override string ToString() => Name ?? throw new NullReferenceException();
}
