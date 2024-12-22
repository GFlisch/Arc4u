using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Diagnostics.Logging;
internal class LoggerWrapperContext
{
    private static Lazy<IServiceScopeFactory> _scopeFactory = default!;

    /// <summary>
    /// Initializes the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static void Initialize(IServiceProvider serviceProvider)
        => _scopeFactory = new Lazy<IServiceScopeFactory>(serviceProvider.GetRequiredService<IServiceScopeFactory>());

    /// <summary>
    /// Creates the scope.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">LoggerContext not initialized</exception>
    public static IServiceScope CreateScope()
    {
        if (!_scopeFactory.IsValueCreated)
        {
            throw new InvalidOperationException("LoggerContext not initialized");
        }

        return _scopeFactory.Value.CreateScope();
    }
}
