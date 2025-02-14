namespace Arc4u.Dependency.ComponentModel;

public class NameResolver
{
    public Dictionary<Tuple<string, Type>, List<Type>> NameResolution { get; private set; }
    public Dictionary<Tuple<string, Type>, List<object>> InstanceNameResolution { get; private set; }

    public NameResolver()
    {
        NameResolution = new Dictionary<Tuple<string, Type>, List<Type>>();
        InstanceNameResolution = new Dictionary<Tuple<string, Type>, List<object>>();
    }
}
