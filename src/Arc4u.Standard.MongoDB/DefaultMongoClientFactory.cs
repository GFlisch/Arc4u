using Arc4u.MongoDB.Exceptions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Arc4u.MongoDB
{
    public class DefaultMongoClientFactory<TContext> : IMongoClientFactory<TContext> where TContext : DbContext
    {
        public DefaultMongoClientFactory(IOptionsMonitor<MongoClientSettings> clientSettings, IServiceProvider serviceProvider)
        {
            _clientSettings = clientSettings;
            _mongoContext = (TContext)serviceProvider.GetService(typeof(TContext));
        }

        private IMongoDatabase _database;
        private IMongoClient _client;
        private static object _locker = new object();
        private readonly IOptionsMonitor<MongoClientSettings> _clientSettings;
        private readonly TContext _mongoContext;

        public IMongoClient CreateClient()
        {
            if (null != _client)
                return _client;

            // one creation at a time => block here only. Few calls will arrive here.
            lock (_locker)
            {
                if (null != _client)
                    return _client;

                var settings = _clientSettings.Get(_mongoContext.DatabaseName.ToLowerInvariant());
                if (null != settings)
                {
                    _client = new MongoClient(settings);
                    return _client;
                }

                throw new NullReferenceException($"No mongo client settings defined for key {_mongoContext.DatabaseName}");
            }
        }

        public IMongoClient CreateClient(MongoClientSettings mongoClientSettings)
        {
            if (null == mongoClientSettings) throw new NullReferenceException($"No mongo client settings defined for key {_mongoContext.DatabaseName}");

            lock (_locker)
            {
                if (null != _client)
                    return _client;

                _client = new MongoClient(mongoClientSettings);
                return _client;
            }
        }

        private IMongoDatabase GetDatabase()
        {
            if (null != _database)
                return _database;

            // one creation at a time => block here only. Few calls will arrive here.
            lock (_locker)
            {
                if (null != _database)
                    return _database;

                var client = CreateClient();

                _database = client.GetDatabase(_mongoContext.DatabaseName);
                return _database;
            }
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName)
        {
            var db = GetDatabase();

            return db.GetCollection<TEntity>(collectionName);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            var type = typeof(TEntity);
            if (!_mongoContext.EntityCollectionTypes.ContainsKey(type))
                throw new TypeNotMappedToCollectionException();

            var collectionNames = _mongoContext.EntityCollectionTypes[type];
            if (collectionNames.Count != 1)
                throw new TypeMappedToMoreThanOneCollectionException<TEntity>(collectionNames.Count);

            var db = GetDatabase();

            return db.GetCollection<TEntity>(collectionNames[0]);
        }
    }
}