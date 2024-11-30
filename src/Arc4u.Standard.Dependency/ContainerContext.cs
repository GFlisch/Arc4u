namespace Arc4u.Dependency;

public static class ContainerContext
{
    private static Func<IContainer> _creationContainerFunction = default!;

    public static void RegisterCreateContainerFunction(Func<IContainer> createContainer)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(createContainer);
#else
        if (null == createContainer)
        {
            throw new ArgumentNullException(nameof(createContainer));
        }
#endif
        _creationContainerFunction = createContainer;
    }

    public static IContainer CreateContainer()
    {
        return _creationContainerFunction();
    }
}
