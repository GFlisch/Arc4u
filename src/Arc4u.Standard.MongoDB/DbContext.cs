using System;
using System.Collections.Generic;
using Arc4u.MongoDB.Configuration;

namespace Arc4u.MongoDB;

public abstract class DbContext
{
    protected abstract void OnConfiguring(DbContextBuilder context);

    internal void Configure(DbContextBuilder context)
    {
        OnConfiguring(context);

        _databaseName = context.DatabaseName;
        _entityCollectionTypes = context.EntityCollectionTypes;
    }

    private string _databaseName;
    private Dictionary<Type, List<string>> _entityCollectionTypes;

    public string DatabaseName { get => _databaseName; }

    public Dictionary<Type, List<string>> EntityCollectionTypes { get => _entityCollectionTypes; }
}
