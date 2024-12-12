using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Arc4u.UnitTest")]

namespace Arc4u.Diagnostics;

/// <summary>
/// Default Arc4u implementation of the <see cref="IActivitySourceFactory"/>.
/// Thread Safe implementation.
/// </summary>
public class DefaultActivitySourceFactory : IActivitySourceFactory
{
    public DefaultActivitySourceFactory()
    {
        _activitySources = new ConcurrentDictionary<string, ActivitySource>();
    }

    private readonly ConcurrentDictionary<string, ActivitySource> _activitySources;

    /// <summary>
    /// Get and Create an <see cref="ActivitySource"/> based on the name and version.
    /// </summary>
    /// <param name="name">The name of the <see cref="ActivitySource"/>.</param>
    /// <param name="version">An optinal parameter to add a version to an <see cref="ActivitySource"/>.</param>
    /// <returns>A specific instance of an <see cref="ActivitySource"/>.</returns>
    public ActivitySource Get(string name, string? version = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(name);
        }

        var key = string.IsNullOrWhiteSpace(version) ? name.Trim() : $"{name.Trim()}_{version!.Trim()}";

        return _activitySources.GetOrAdd(key, _ => new ActivitySource(name, version));
    }
}

internal static class ActivitySourceExtension
{
    public static ActivitySource GetArc4u(this IActivitySourceFactory activitySourceFactory)
    {
        return activitySourceFactory.Get("Arc4u");
    }
}
