namespace Arc4u.Dependency.Configuration;

public class Dependencies
{
    public ICollection<AssemblyConfig> Assemblies { get; set; } = [];

    public ICollection<string> RegisterTypes { get; set; } = [];
}
