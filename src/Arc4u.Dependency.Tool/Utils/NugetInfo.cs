using System.Reflection;

namespace Arc4u.Dependency.Tool;
class NugetInfo(string nugetFile, AssemblyName assemblyName)
{
    public AssemblyName Assembly { get; private set; } = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));

    public string NugetFile { get; private set; } = nugetFile ?? throw new ArgumentNullException(nameof(nugetFile));

    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }

    private string FullName => NugetFile + Assembly.FullName;

    public override bool Equals(object? obj)
    {
        return obj is NugetInfo other && FullName == other.FullName;
    }
}
