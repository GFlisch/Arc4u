namespace Arc4u.Dependency;

public interface IContainer : IContainerRegistry, IContainerResolve, IServiceProvider
{
    object Instance { get; }
}
