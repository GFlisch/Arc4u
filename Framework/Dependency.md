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

### Arc4u.Dependency.Composition

Used the System.Composition (mef2) with all the possibilities from this Ioc:
- Accept constructor with a named version X([Import("A") IObject2])...
- Accept property injection => Strongly not recommended.

### Arc4u.Dependency.ComponentModel

Used the System.ComponentModel (.Net core) implementation:
- Doesn't accept a named constructor X(IObject2 o).
- Doesn't accept a property injection.


