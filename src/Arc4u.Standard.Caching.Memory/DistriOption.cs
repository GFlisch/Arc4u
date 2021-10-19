using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Arc4u.Caching.Memory
{
    class DistriOption : IOptions<MemoryDistributedCacheOptions>
    {
        public DistriOption(MemoryDistributedCacheOptions options)
        {
            _options = options;
        }

        readonly MemoryDistributedCacheOptions _options;

        public MemoryDistributedCacheOptions Value => _options;
    }
}
