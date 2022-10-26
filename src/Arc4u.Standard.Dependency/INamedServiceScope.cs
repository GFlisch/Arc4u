using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dependency
{
    public interface INamedServiceScope : IServiceScope
    {
        new INamedServiceProvider ServiceProvider { get; }
    }
}
