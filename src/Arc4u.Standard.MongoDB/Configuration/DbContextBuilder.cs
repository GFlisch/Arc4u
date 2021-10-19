using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc4u.MongoDB
{
    public class DbContextBuilder
    {
        public DbContextBuilder(IServiceCollection services, string databaseName, string connectionStringKey)
        {
            Services = services;
            DatabaseName = databaseName;
            ConnectionStringKey = connectionStringKey;
            EntityCollectionTypes = new Dictionary<Type, List<string>>();
        }

        internal readonly IServiceCollection Services;
        internal readonly string DatabaseName;
        internal readonly string ConnectionStringKey;

        internal readonly Dictionary<Type, List<string>> EntityCollectionTypes;

        public WithType MapCollection(string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentNullException(nameof(collectionName));

            return new WithType(collectionName, this);

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

        public WithType With<TEntity>() where TEntity : class, new()
        {
            var type = typeof(TEntity);

            // Add if not exist!
            if (_contextBuilder.EntityCollectionTypes.ContainsKey(type)
                &&
                !_contextBuilder.EntityCollectionTypes[type]
                    .Any(c => c.Equals(_collectionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                _contextBuilder.EntityCollectionTypes[type].Add(_collectionName);
                return this;
            }

            _contextBuilder.EntityCollectionTypes.Add(type, new List<string>() { _collectionName });

            return this;
        }
    }
}
