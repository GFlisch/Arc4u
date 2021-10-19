using MongoDB.Driver;

namespace Arc4u.MongoDB
{
    /// <summary>
    /// Define the behavior expected from the factory to create a mongo client and retrieve the database (part of the description).
    /// </summary>
    public interface IMongoClientFactory<TContext> where TContext : DbContext
    {
        IMongoClient CreateClient();

        IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName);

        IMongoCollection<TEntity> GetCollection<TEntity>();
    }

}
