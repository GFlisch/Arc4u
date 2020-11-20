# Dependency

Modern applications use an Ioc engine to decouple the code from the implementation and allow also a better unit testing.

In Arc4u, the skeleton of the framework is heavily based on the [inversion of control principle](https://en.wikipedia.org/wiki/Inversion_of_control). 

Finaly the framework is a set of functionality that can be injected when needed in your application.

The Api is defined in the nuget package Arc4u.Standard.Dependency and contains principaly the definition (abstraction) of a container.

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

During the registration process we can do the different registrations:

````csharp
    public interface IContainerRegistry
    {
        void Register<TFrom, To>() where TFrom : class where To : class, TFrom;
        void Register(Type from, Type to);

        void Register<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        void Register(Type from, Type to, string name);

        void RegisterFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        void RegisterFactory(Type type, Func<object> exportedInstanceFactory);

        void RegisterSingletonFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        void RegisterSingletonFactory(Type type, Func<object> exportedInstanceFactory);

        void RegisterScopedFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        void RegisterScopedFactory(Type type, Func<object> exportedInstanceFactory);

        void RegisterInstance<T>(T instance) where T : class;
        void RegisterInstance(Type type, object instance);

        void RegisterInstance<T>(T instance, string name) where T : class;
        void RegisterInstance(Type type, object instance, string name);

        void RegisterSingleton(Type from, Type to);
        void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom;

        void RegisterScoped<TFrom, To>() where TFrom : class where To : class, TFrom;
        void RegisterScoped(Type from, Type to);

        void RegisterScoped<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        void RegisterScoped(Type from, Type to, string name);

        void RegisterSingleton<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        void RegisterSingleton(Type from, Type to, string name);

        void Initialize(Type[] types, params Assembly[] assemblies);

        void CreateContainer();
    }
````

If a Ioc doesn't contains a feature like Scoped, an exception is thrown.

## IContainerResolve.

This interface contains all the methods used to resolve a type.

````csharp
    public interface IContainerResolve : IDisposable
    {
        T Resolve<T>();
        object Resolve(Type type);

        T Resolve<T>(string name);
        object Resolve(Type type, string name);

        bool TryResolve<T>(out T value);
        bool TryResolve(Type type, out object value);

        bool TryResolve<T>(string name, out T value);
        bool TryResolve(Type type, string name, out object value);

        IEnumerable<T> ResolveAll<T>();
        IEnumerable<object> ResolveAll(Type type);

        IEnumerable<T> ResolveAll<T>(string name);
        IEnumerable<object> ResolveAll(Type type, string name);

        IContainerResolve CreateScope();

        bool CanCreateScope { get; }
    }
````

One of the specifity with the interface is the ability to resolve with a name. This is a handy functionality that currently doesn’t exist on .Net 5.0 and is possible on Mef2. 
One example of this usage is for an engine rule. If you have different code to execute based on a criterion, you can use a string (from a database for ex.) and apply the rule based on this name.

In .Net 5.0 the ServiceCollection instance doesn’t allow you to inject in a constructor an interface with a string to differentiate a specific registration.
Arc4u will not help you to inject the specific instance but you can inject the IContainerResolve interface and resolve by name the specific rule.

Each Ioc engine have their specifics way of working. The target of the IContainer is to homogenize the different behaviors.

The golden rules of the DependencyContext abstraction are:
- Once created, we cannot add new type to the container.
- Injecting a type via a constructor cannot be named (via a string to select an implementation).


````csharp
    var container = new ContinerXXX();
    // Add registration.
    container.Register<IObject, Object>();
    container.RegisterSingleton<IObject2, Object2_0>("A");
    container.RegisterSingleton<IObject2, Object2_1>("B");


    // When you are on a non AspNet(Core) service like a Wpf, Uwp, Xamarin.Forms or console application.
    container.CreateContainer(); 

    container.Resolve<IObject2>("A");

    ...
````

2 Containers exist in Arc4u:
- System.Composition (mef2), used before.
- System.ComponentModel used currently in .Net (core).

### Configuration.

In a real business application we have a huge number of types to register in the container and doing this needs to manage this correctly.

The problem with the Ioc is the declaration (registration of the different types). 
It is clear that the extension methods on ServiceCollection are a handy way to configure the Ioc 
but concerning the business code of the application this is a long list to maintain in another location that your code.

This is why the Dependency package of Arc4u contains a method to parse your assemblies and register on the fly the types you want to register.
This is largely inspired from the Mef2.0. An Attribute namespace exists on Dependency with the definition of Export, Shared or Scoped.

3 possinilities exists:
- Export(typeof, name: optional). The behavior will be transient.
- Export(typeof, name: optional), Shared. The behavior will be singleton.
- Export(typeof, name: optional), Scoped. The behavior will be scoped.

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
- Defining 2 Export with Shared attribute will not reference the same instance!

````csharp
    [Export(typeof(IGenerator)), Shared]
    [Export]
    public class TestParser : IGenerator
    {
        public TestParser()
        {
            _id = Guid.NewGuid();
        }

        private Guid _id;
        public Guid Id => _id;
    }
````

Resolving <IGenerator> or <TestParser> will not give the same instance even if the shared attribute is defined.
This behavior is different from mef2. Mef2 will return the same instance!


