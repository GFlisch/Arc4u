namespace Arc4u.Dependency;

public static class ContainerContext
{
    private static Func<IContainer> _creationContainerFunction = default!;

    public static void RegisterCreateContainerFunction(Func<IContainer> createContainer)
    {
        ArgumentNullException.ThrowIfNull(createContainer);

        _creationContainerFunction = createContainer;
    }

    public static IContainer CreateContainer()
    {
        return _creationContainerFunction();
    }
}
