[ :back: ](../General.md)
# Caching

The abstraction introduces by the framework is not to implement what exist already in the .NET Core framework but to enhance it by adding more flecibility.

It offers the following capabilities:
- Setup the caching by config.
- Allow multiple caches
- Define the cache for the Principal.
- Use the standards define by the .NET Core framework
    - Memory
    - Sql
    - Redis

Add integration with [Dapr Store](https://dapr.io/).


The data is serialized in the caching => so no need to do this by yourself.
## Details

### Arc4u.Standard.Caching

Define the interface to abstract the caching.<br>
Define the model and the classes to read the configuration.

Before Arc4u 6.0.14.3, the cache and the definition of the store was splitted and finally not easy to manage.
You had to find the section for the memory cache defined and check the code to see which settings was associated to the "Volatile" definition (in the code)...

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
  },
  "Memory.Settings": {
    "SizeLimitInMegaBytes": "10"
  }
}
```

From 6.0.14.3, this is now consistent. The settings to configure the cache used are now part of the definition of the cache itself!

Here for the Memory cache and a redis one we have this config:

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
        "Settings": {
          "SizeLimit": "100"
        },
        "IsAutoStart": "True"
      },
      {
        "Name": "Distributed",
        "Kind": "Redis",
        "Settings": {
          "ConnectionString": "localhost:6379",
          "InstanceName": "DemoArc4u.ApiGtw.Development-"
        },
        "IsAutoStart": "True"
      }
    ]
  }
}
```

Before 6.0.14.3, to access the caches, you had to resolve the CacheContext class directly. After, you have to resolve the ICacheContext.

Any cache is using the concept of the Serialization introduces in Arc4u.<br>
So the data stored is serialized (even for the memory caching).<br>
The number of cache is not limited and as the configuration is defined in a config file, the store can be different following the environment. This is defined with the Kind property.

=> JSonSerializers are the recommended ones, the Protobuf ones will disappear and will be marked as Obsolete.

### Arc4u.Standard.Caching.Memory

Store the data in the memory. Kind is Memory.

### Arc4u.Standard.Caching.Redis

Store the data in [Redis](https://redis.io/). Kind is Redis.

### Arc4u.Standard.Caching.Sql

Store the date in Microsoft Sql Server. Kind is Sql.

### Arc4u.Standard.Caching.Dapr

[Dapr](https://dapr.io/) is a modern sidecar framework which contains the capability to store the data in a multitude of repository. See [Dapr stores](https://docs.dapr.io/reference/components-reference/supported-state-stores/).