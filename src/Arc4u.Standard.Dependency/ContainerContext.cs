using System;

namespace Arc4u.Dependency
{
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
}
