using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.MongoDB.Configuration;

public class DbContextBuilder
{
    public DbContextBuilder(IServiceCollection services, string databaseName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(databaseName);

        Services = services;
        DatabaseName = databaseName;
        EntityCollectionTypes = [];
    }

    internal readonly IServiceCollection Services;
    internal readonly string DatabaseName;

    internal readonly Dictionary<Type, List<string>> EntityCollectionTypes;

    public WithType MapCollection(string collectionName)
    {
        return string.IsNullOrWhiteSpace(collectionName)
            ? throw new ArgumentNullException(nameof(collectionName))
            : new WithType(collectionName, this);
    }

}

public class WithType
{
    public WithType(string collectionName, DbContextBuilder contextBuilder)
    {
        _contextBuilder = contextBuilder;
        _collectionName = collectionName;
    }

    private readonly DbContextBuilder _contextBuilder;
    private readonly string _collectionName;

    public WithType With<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        // Add if not exist!
        if (!_contextBuilder.EntityCollectionTypes.TryGetValue(type, out var value) ||
            value.Exists(c => c.Equals(_collectionName, StringComparison.OrdinalIgnoreCase)))
        {
            _contextBuilder.EntityCollectionTypes.Add(type, [_collectionName]);

            return this;
        }

        value.Add(_collectionName);
        return this;
    }
}
