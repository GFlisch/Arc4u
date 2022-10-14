[ :back: ](..\General.md)
# Caching

The abstraction introduces by the framework is not to implement what exist already in the .NET framework but to enhance it by adding more flecibility.
But to offer the following capabilities:
- Setup the caching by config.
- Allow multiple caches
- Define the cache for the Principal.
- Use the standards define by the .NET framework
    - Memory
    - Sql
    - Redis

Add integration with [Dapr Store](https://dapr.io/).


The data is serialized in the caching => so no need to do this by yourself.
## Details

### Arc4u.Standard.Caching

Define the interface to abstract the caching.<br>
Define the model and the class to read the configuration.

```json
{
  "Caching": {
    "Default": "Volatile",
    "Principal": {
      "IsEnabled": "True",
      "CacheName": "Volatile",
      "Duration": "10:00:00"
    },
    "Caches": [
      {
        "Name": "Volatile",
        "Kind": "Memory",
        "IsAutoStart": "True"
      }
    ]
  }
}
```

CacheContext is the class to use to retrieve a cache.

Any cache is using the concept of the Serialization introduces in Arc4u.<br>
So the data stored is serialized (even for the memory caching).<br>
The number of cache is not limited and as the configuration is defined in a config file, the store can be different following the environment. This is defined with the Kind property.



### Arc4u.Standard.Caching.Memory

Store the data in the memory. Kind is Volatile.

### Arc4u.Standard.Caching.Redis

Store the data in [Redis](https://redis.io/). Kind is Redis.

### Arc4u.Standard.Caching.Sql

Store the date in Microsoft Sql Server. Kind is Sql.

### Arc4u.Standard.Caching.Dapr

[Dapr](https://dapr.io/) is a modern sidecar framework which contains the capability to store the data in a multitude of repository. See [Dapr stores](https://docs.dapr.io/reference/components-reference/supported-state-stores/).