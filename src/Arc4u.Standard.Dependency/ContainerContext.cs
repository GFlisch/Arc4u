using System;

namespace Arc4u.Dependency
{
    /// <summary>
    /// Doesn't seem to be used anywhere else but in unit tests
    /// </summary>
    public static class ContainerContext
    {
        private static Func<IContainer> _creationContainerFunction;

        public static void RegisterCreateContainerFunction(Func<IContainer> createContainer)
        {
            if (null == createContainer)
                throw new ArgumentNullException(nameof(createContainer));

            _creationContainerFunction = createContainer;
        }

        public static IContainer CreateContainer()
        {
            return _creationContainerFunction();
        }
    }


    /// <summary>
    /// Introduced for the same reason as <see cref="ContainerContext"/>
    /// </summary>
    public static class ProviderContext
    {
        private static Func<INamedServiceProvider> _providerCreator;

        public static void RegisterCreateContainerFunction(Func<INamedServiceProvider> providerCreator)
        {
            _providerCreator = providerCreator ?? throw new ArgumentNullException(nameof(providerCreator));
        }

        public static INamedServiceProvider CreateContainer()
        {
            return _providerCreator();
        }
    }
}

