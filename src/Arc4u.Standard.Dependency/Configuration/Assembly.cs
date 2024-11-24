namespace Arc4u.Dependency.Configuration;

public class AssemblyConfig
{
    public string Assembly { get; set; } = String.Empty;

    public ICollection<string> RejectedTypes { get; set; } = [];
}
