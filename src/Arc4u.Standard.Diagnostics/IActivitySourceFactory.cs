using System.Diagnostics;

namespace Arc4u.Diagnostics
{

    /// <summary>
    /// Interface defining the method to obtain a singleton instance of a ActivitySource to add Telemetry in a .net core project.
    /// </summary>
    public interface IActivitySourceFactory
    {
        /// <summary>
        /// Get or create a Singleton instance of an ActivitySource.
        /// </summary>
        /// <param name="name">The name of the <see cref="ActivitySource"/>.</param>
        /// <param name="version">An optinal parameter to add a version to an <see cref="ActivitySource"/>.</param>
        /// <returns>A specific instance of an <see cref="ActivitySource"/>.</returns>
        ActivitySource Get(string name, string version = null);
    }
}
