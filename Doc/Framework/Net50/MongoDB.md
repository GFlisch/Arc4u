# Arc4u.Standard.MongoDB

This package manages the client and Database configuration of MongoDB.

## General

The idea is to take the same concept than EF Core and working with a kind of DbContext.
One DbContext is defined by Database and inside the configuration of a database we can map
different types to a collection.

## How it works.

The solution will contain a Mongo database project having Arc4u.Standard.MongoDB package.

This package references the MongoDB.Driver package and the Arc4u.Standard.Dependency.

The Database project is referenced in the AspNet 5 Service.

### Configuration.

#### Connection string.

Add in your appsettings.json file the connection string entry of you MongoDB server.
Check the [MongoDB documentation](https://docs.mongodb.com/manual/reference/connection-string/).

```json
{
  "ConnectionStrings": {
    "mongo": "mongodb://mongodb0.example.com:27017/Demo"
  }
}
```

>[!IMPORTANT]
>The connection string must contains the Database name: Demo in the current example.

#### DbContext.

The Arc4u MongoDB package will contain an abstract class DbContext.
Basically this class contains 2 methods:

```csharp
    public abstract class DbContext
    {
        protected abstract void OnConfiguring(DbContextBuilder context);

        internal void Configure(DbContextBuilder context)
        {
            ...
        }
    }
```

In you application you will have to create (in the Database project) your context.
For our Demo Database we will declare 2 collections and entities mapped to them.

To do this we will create a DemoContext class.

```csharp
    public class DemoContext : DbContext
    {
        protected override void OnConfiguring(DbContextBuilder context)
        {
            context.MapCollection("Contracts")
                .With<ContractV1>()
                .With<ContractV2>();

            context.MapCollection("Users")
                .With<User>();

        }
    }
```
Now we have associated the entities to a specific collection.

>[!IMPORTANT]
>It is not recommended to use more than one entity by collection.
>It is possible technically and I want to cover this need but this should be the exception.

#### Entities.

At the level of the Entities declared in the Domain.Model project, adds the MongoDB.Bson package so we can define the BsonId attribute.

```csharp

    public class User
    {
        [BsonId]
        public Guid Id { get; set; }

        public String Name { get; set; }
    }
}
```

>[!TIP]
>There is no constraints at the level of the entity and the configuration.

#### Startup.cs

In the startup file we will register the different contexts (if any).

```csharp
public void ConfigureServices(IServiceCollection services)
        {
            ...

            services.AddMongoDatabase<DemoContext>(Configuration, "mongo");
        }
```

During the initialization, the mappings will be registered and the DemoContext will have lifetime set to Singleton.


### Queries.

Now, how do we use this to perform queries.

In the Database project, we will have the following syntax:

```csharp

public class UsersDL : IUsersDL
{
    public Users(IMongoClientFactory<DemoContext> factory)
    {
        _factory = factory;_
    }

    private readonly IMongoClientFactory<DemoContext> _factory;

    public void Add(User user)
    {
        var collection = _factory.GetCollection<User>();

        collection.InsertOne(user);
    }

    public async Task<List<User>> GetAll()
    {
        var collection = _factory.GetCollection<User>();

        return await collection.AsQueryable().ToListAsync();
    }
}

```

>[!IMPORTANT]
>IMongoClientFactory<DemoContext> is registered in the ServiceCollections as a Singleton.
>IMongoClient and IMongoDatabase are instantiated once!

Thanks to the mapping, the GetCollection<T> will be able to find the collection associated to it.
If you don't have mapped this correctly the exception TypeNotMappedToCollectionException will be thrown.

What happens if you have mapped an entity to 2 different collections (on the same context)?
<br>In this case, the following exception will be thrown: TypeMappedToMoreThanOneCollectionException.

>[!TIP]
>You can in this case specify the collection you want by calling the GetCollection\<T\>("name of the collection").


