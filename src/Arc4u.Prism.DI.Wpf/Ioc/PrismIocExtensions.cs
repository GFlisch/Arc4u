using Arc4u.Dependency;
using Prism.Ioc;
using IContainerRegistry = Prism.Ioc.IContainerRegistry;

namespace Prism.DI.Ioc;

public static class PrismIocExtensions
{
    public static IContainer GetContainer(this IContainerProvider containerProvider)
    {
        return ((IContainerExtension<IContainer>)containerProvider).Instance;
    }

    public static IContainer GetContainer(this IContainerRegistry containerRegistry)
    {
        return ((IContainerExtension<IContainer>)containerRegistry).Instance;
    }
}
