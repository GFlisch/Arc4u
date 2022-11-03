using System;
using Arc4u.Dependency.Attribute;
using Microsoft.Extensions.Hosting;


namespace Arc4u.Extensions.Hosting
{
    /// <summary>
    /// Used to be injected in the DI.
    /// </summary>
    public interface IHostEnvironment 
    {
        public bool IsEnvironment(string environment);
    }

    [Export(typeof(IHostEnvironment)), Shared]
    public class HostEnvironment : IHostEnvironment
    {
        public HostEnvironment(Microsoft.Extensions.Hosting.IHostEnvironment environment)
        {
            _environment = environment;
        }

        private readonly Microsoft.Extensions.Hosting.IHostEnvironment _environment;

        public bool IsEnvironment(string environmentName)
        {
            ArgumentNullException.ThrowIfNull(environmentName);

            return _environment.IsEnvironment(environmentName);
        }
    }

    public static class Environments
    {
        public static string Localhost { get; } = "Localhost";

        public static string Development { get; set; } = "Dev";

        public static string Test { get; set; } = "Test";

        public static string Acceptance { get; set; } = "Acc";

        public static string Production { get; set; } = "Prod";

        public static string Demo { get; set; } = "Demo";
    }


    /// <summary>
    /// Extension methods for <see cref="IHostEnvironment"/>.
    /// </summary>
    public static class HostEnvironmentExtensions
    {

        /// <summary>
        /// Checks if the current host environment name is <see cref="Environments.Localhost"/>.
        /// </summary>
        /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="Environments.Localhost"/>, otherwise false.</returns>
        public static bool IsLocalhost(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Localhost);
        }

        /// <summary>
        /// Checks if the current host environment name is <see cref="Environments.Development"/>.
        /// </summary>
        /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="Environments.Development"/>, otherwise false.</returns>
        public static bool IsDevelopment(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Development);
        }

        /// <summary>
        /// Checks if the current host environment name is <see cref="Environments.Test"/>.
        /// </summary>
        /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="Environments.Test"/>, otherwise false.</returns>
        public static bool IsTest(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Test);
        }


        /// <summary>
        /// Checks if the current host environment name is <see cref="Environments.Acceptance"/>.
        /// </summary>
        /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="Environments.Acceptance"/>, otherwise false.</returns>
        public static bool IsAcceptance(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Acceptance);
        }

        /// <summary>
        /// Checks if the current host environment name is <see cref="Environments.Demo"/>.
        /// </summary>
        /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="Environments.Demo"/>, otherwise false.</returns>
        public static bool IsDemo(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Demo);
        }

        /// <summary>
        /// Checks if the current host environment name is <see cref="Environments.Production"/>.
        /// </summary>
        /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="Environments.Production"/>, otherwise false.</returns>
        public static bool IsProduction(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Production);
        }

    }
}
