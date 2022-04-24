using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Logging;
using System;

namespace Arc4u.Caching.Sql
{
    /// <summary>
    /// See Documentation how to create a database here: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.0
    /// </summary>
    [Export("Sql", typeof(ICache))]
    public class SqlCache : BaseDistributeCache, ICache
    {
        public const string ConnectionStringKey = "ConnectionString";
        public const string TableNameKey = "Name";
        public const string SchemaNameKey = "Schema";
        private const string SerializerNameKey = "SerializerName";

        private String Name { get; set; }

        private readonly ILogger Logger;

        public SqlCache(ILogger logger, IContainerResolve container) : base(container)
        {
            Logger = logger;
        }

        public override void Initialize(string store)
        {
            lock (_lock)
            {
                if (IsInitialized)
                {
                    Logger.Technical().From<SqlCache>().Debug($"Sql Cache {store} is already initialized.").Log();
                    return;
                }
                Name = store;

                if (Container.TryResolve<IKeyValueSettings>(store, out var settings))
                {
                    if (settings.Values.ContainsKey(ConnectionStringKey))
                        ConnectionString = settings.Values[ConnectionStringKey];

                    if (settings.Values.ContainsKey(TableNameKey))
                        TableName = settings.Values[TableNameKey];

                    if (settings.Values.ContainsKey(SchemaNameKey))
                        SchemaName = settings.Values[SchemaNameKey];

                    if (settings.Values.ContainsKey(SerializerNameKey))
                        SerializerName = settings.Values[SerializerNameKey];
                    else
                        SerializerName = store;

                    var option = new SqlServerCacheOptions
                    {
                        ConnectionString = ConnectionString,
                        TableName = TableName,
                        SchemaName = SchemaName
                    };

                    DistributeCache = new SqlServerCache(option);

                    if (!Container.TryResolve<IObjectSerialization>(SerializerName, out var serializerFactory))
                    {
                        SerializerFactory = Container.Resolve<IObjectSerialization>();
                    }
                    else SerializerFactory = serializerFactory;

                    IsInitialized = true;
                    Logger.Technical().From<SqlCache>().System($"Sql Cache {store} is initialized.").Log();

                }
            }
        }

        public override string ToString() => Name;


        private String ConnectionString { get; set; }
        private String TableName { get; set; } = "Default";
        private String SchemaName { get; set; } = "dbo";
        private String SerializerName { get; set; }
    }
}
