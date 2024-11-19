using Microsoft.Extensions.Logging;

namespace Arc4u.Data
{
    public static class ValidationExtention
    {
        public static void ValidateAll<T>(this IEnumerable<T> entities, ILogger<T> logger) where T : PersistEntity
        {
            var messages = new ServiceModel.Messages();
            foreach (var entity in entities)
            {
                messages.AddRange(entity.TryValidate());
            }
            messages.LogAndThrowIfNecessary(logger);
        }
    }
}
