using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.Serializer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Arc4u.Caching.Memory
{
    [Export("Memory", typeof(ICache))]
    public class MemoryCache : BaseDistributeCache, ICache
    {
        public const string CompactionPercentageKey = "CompactionPercentage";
        public const string SizeLimitKey = "SizeLimitInMegaBytes";
        public const string SerializerNameKey = "SerializerName";

        private String Name { get; set; }

        private readonly ILogger Logger;

        public MemoryCache(ILogger logger, IServiceProvider container) : base(container)
        {
            Logger = logger;
        }

        public override void Initialize(string store)
        {
            lock (_lock)
            {
                if (IsInitialized)
                {
                    Logger.Technical().From<MemoryCache>().Debug($"Memory Cache {store} is already initialized.").Log();
                    return;
                }
                Name = store;

                if (Container.TryGetService<IKeyValueSettings>(store, out var settings))
                {
                    if (settings.Values.ContainsKey(CompactionPercentageKey))
                    {
                        if (Double.TryParse(settings.Values[CompactionPercentageKey], out var percentage))
                            CompactionPercentage = percentage / 100;
                    }

                    if (settings.Values.ContainsKey(SizeLimitKey))
                    {
                        if (long.TryParse(settings.Values[SizeLimitKey], out var size))
                            SizeLimit = size * 1024 * 1024;
                    }

                    if (settings.Values.ContainsKey(SerializerNameKey))
                        SerializerName = settings.Values[SerializerNameKey];
                    else
                        SerializerName = store;

                    var option = new DistriOption(new MemoryDistributedCacheOptions
                    {
                        CompactionPercentage = CompactionPercentage,
                        SizeLimit = SizeLimit
                    });

                    DistributeCache = new MemoryDistributedCache(option);

                    if (!Container.TryGetService<IObjectSerialization>(SerializerName, out var serializerFactory))
                    {
                        SerializerFactory = Container.GetService<IObjectSerialization>();
                    }
                    else SerializerFactory = serializerFactory;

                    IsInitialized = true;
                    Logger.Technical().From<MemoryCache>().System($"Memory Cache {store} is initialized.").Log();

                }
            }
        }

        public override string ToString() => Name;

        private double CompactionPercentage { get; set; } = 0.2;
        private long? SizeLimit { get; set; } = 1024 * 1024 * 1024;
        private String SerializerName { get; set; }
    }
}
