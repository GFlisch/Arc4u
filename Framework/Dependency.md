# Dependency

Modern applications use an Ioc engine to decouple the code from the implementation and allow also a better unit testing.

In Arc4u, the skeleton of the framework is heavily based on the [inversion of control principle](https://en.wikipedia.org/wiki/Inversion_of_control). 

Finaly the framework is a set of functionality that can be injected when needed in your application.

The Api is defined in the nuget package Arc4u.Standard.Dependency and contains principaly 2 concepts.

The first one is the definition (abstraction) of a container.
The second is a singleton class: DependencyContext, used to work with the underneath container.

## IContainer.

The abstration of the Ioc engine is built via the IContainer interface.

````csharp

    public interface IContainer : IContainerRegistry, IContainerResolve, IServiceProvider
    {
        Object Instance { get; }
    }
````

This interface contains mainly 2 parts: the registration and the resolve.

Each Ioc container used in the framework implements this interface.

During the registration process we can do:

````csharp
   public interface IContainerRegistry
    {
        void Register<TFrom, To>() where TFrom : class where To : class, TFrom;
        void Register(Type from, Type to);

        void Register<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        void Register(Type from, Type to, string name);

        void RegisterFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        void RegisterFactory(Func<object> exportedInstanceFactory, Type type);

        void RegisterSingletonFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        void RegisterSingletonFactory(Func<object> exportedInstanceFactory, Type type);

        void RegisterInstance<T>(T instance) where T : class;
        void RegisterInstance(Type type, object instance);

        void RegisterInstance<T>(T instance, string name) where T : class;
        void RegisterInstance(Type type, object instance, string name);

        void RegisterSingleton(Type from, Type to);
        void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom;

        void RegisterSingleton<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        void RegisterSingleton(Type from, Type to, string name);

        void Initialize(Type[] types, params Assembly[] assemblies);

        void CreateContainer();
    }
````


## DependencyContext.

DependencyContext is a singleton class which is used everywhere in the application to resolve any type registered.

DependencyContext will not access the registration part of the container but only the resolve methods.

When we use an Ioc we often inject a type using the constructor of the type. 
Sometimes we need to create a type under certain cirsumstance: on demand. 
Or we need to create a specific implementation based on a criteria.
An example for this is in a rule engine where different implementation exist and based on a string (coming from a database) we can resolve a specific implementation.
In this case, we cannot do this via the constructor!

Each Ioc engine have their specific way of working. The target of the DependencyContext is to homogenize the different behaviors.

The golden rules of the DependencyContext are:
- Once created, we cannot add new type to the container.
- Injecting a type via a constructor cannot be named (via a string to select an implementation).

Basically the pseudo code to work with a container and DependencyContext is:

````csharp
    var container = new ContinerXXX();
    // Add registration.
    container.Register<IObject, Object>();
    container.RegisterSingleton<IObject2, Object2_0>("A");
    container.RegisterSingleton<IObject2, Object2_1>("B");

    container.CreateContainer(); 

    DependencyContext.CreateContext(container);

    // anywhere in the code.
    var instance = DependencyContext.Current.Resolve<IObject2>("A");

    ...
````

2 Containers exist in Arc4u:
- System.Composition (mef2), used before.
- System.ComponentModel used currently in .Net core.

All the types in the framework are defined via the attributed model of mef2. Even if the Ioc is not used, the attributes:
- Export
- Shared
are used to discover this dynamically and register the types in the Ioc.

>! Important
> Arc4u.Dependency also contains a definition of Export and Shared. If you don't use Mef2 implementation
> you can se those attributes. For .Net application, System.Composition.AttributeModel is used and in .Net Core, only 
> the Arc4u attribute will be used. 

### Configuration.

In a real business application we have a huge number of types to register in the container and doing this needs to manage this correctly.

In the Arc4u.Dependency package, there are extension methods to read json file and based on assemblies or defined types, the method will
parse a section and add types where an Export attribute exists. Exactly like in Mef2.

````csharp
    [Export(typeof(IEnvironmentInfoBL)), Shared]
    public class EnvironmentInfoBL : IEnvironmentInfoBL
    {
        [ImportingConstructor]
        public EnvironmentInfoBL(Config config, IAppSettings settings)
        {
            _config = config;
            _appSettings = settings;
        }
        `...
    }
````

When the extension will read all the types with an Export attribute, he will in fact do this:

````csharp
    container.RegisterSingleton<IEnvironmentInfoBL,EnvironmentInfoBL();
````

As the attribute is coming from System.Composition.AttributedModel, we can give a Name and specify if we want to have a
singleton pattern or not by adding the Shared attribute or not.

The json file format is:

````json
{
  "Application.Dependency": {
    "Assemblies": [
      {
        "Assembly": "Solution3.Business"
      },
      {
        "Assembly": "Solution3.Facade"
      },
      {
        "Assembly": "Solution3.Jobs"
      }
    ],
    "RegisterTypes": [
      {
        "Type": "Arc4u.AppSettings, Arc4u.Standard.Configuration"
      },
      {
        "Type": "Arc4u.ConnectionStrings, Arc4u.Standard.Configuration"
      }
    ]
  }
}

````

In this example, three assemblies are discovered and multiple specific types are added.

If you have some types in an assembly that you don't want to add, you can specify which ones you want to reject.

````json
{
  "Application.Dependency": {
    "Assemblies": [
      {
        "Assembly": "Arc4u.Standard.Core.Test",
        "RejectedTypes": [
          { "Type": "Arc4u.Core.Test.IdGenerator" },
          { "Type": "Arc4u.Core.Test.SingletonIdGenerator" }
        ]
      }
    ],
    "RegisterTypes": [
      { "Type": "Arc4u.Caching.Memory.MemoryCache, Arc4u.Standard.Caching.Memory" },
      { "Type": "Arc4u.Caching.CacheContext, Arc4u.Standard.Caching" }
    ]
  }
}
````

The extension methods in the framework are:

````csharp
    public static class ContainerInitializerExtention
    {
        public static IContainer InitializeFromConfig(this IContainer container, IConfiguration configuration)
        {
            ...
        }
    }
````

The Arc4u.standard.Configurtion package defines a helper to read multiple json file or embedded resources and returns an IConfiguration instance.
When you are using .Net Core, this is done for you but not in .Net, Uwp, Xamarin.Forms and Wpf projects.

For Uwp and Xamrin, you need to read the section from an Embedded resource and files for Wpf and .Net projects.

So the regitration process in the Ioc becomes:

````csharp
        var configuration = ConfigurationHelper.GetConfigurationFromFile(@"appSettings.json");
        var config = new Config(configuration);

        var container = new ComponentModelContainer().InitializeFromConfig(configuration);
        container.RegisterInstance(configuration);
        container.RegisterInstance(config);
        container.Register<IObjectSerializerFactory, ProtoBufSerializerFactory>();

        container.CreateContainer();

        DependencyContext.CreateContext(container);
````


### Arc4u.Dependency.Composition

Used the System.Composition (mef2) with all the possibilities from this Ioc:
- Accept constructor with a named version X([Import("A") IObject2])...
- Accept property injection => Strongly not recommended.

### Arc4u.Dependency.ComponentModel

Used the System.ComponentModel (.Net core) implementation:
- Doesn't accept a named constructor X(IObject2 o).
- Doesn't accept a property injection.


