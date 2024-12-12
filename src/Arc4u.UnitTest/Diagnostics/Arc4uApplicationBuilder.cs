using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arc4u.UnitTest.Diagnostics;

public class Arc4uApplicationBuilder<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private Action<IServiceCollection> _configureServices = _ => { };
    private Action<IApplicationBuilder> _configure = _ => { };
    private IEnumerable<KeyValuePair<string, string?>> _additionalConfiguration = [];

    /// <summary>
    /// Adds additional configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    public Arc4uApplicationBuilder<TEntryPoint> WithAdditionalConfiguration(params KeyValuePair<string, string?>[] configuration)
        => WithConfiguration(configuration);

    /// <summary>
    /// Adds additional configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public Arc4uApplicationBuilder<TEntryPoint> WithConfiguration(IEnumerable<KeyValuePair<string, string?>> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _additionalConfiguration = configuration.UnionBy(_additionalConfiguration, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
        return this;
    }

    /// <summary>
    /// Configures the application.
    /// </summary>
    /// <param name="configure">The configure.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public Arc4uApplicationBuilder<TEntryPoint> Configure(Action<IApplicationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _configure += configure;
        return this;
    }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="configureServices">The configure services.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public Arc4uApplicationBuilder<TEntryPoint> ConfigureServices(Action<IServiceCollection> configureServices)
    {
        ArgumentNullException.ThrowIfNull(configureServices);
        _configureServices += configureServices;
        return this;
    }

    /// <inheritdoc/>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureHostConfiguration(cfg =>
            cfg.AddInMemoryCollection(_additionalConfiguration));

        return base.CreateHost(builder);
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        const string env = "SystemTest";

        builder.UseEnvironment(env)
            .ConfigureAppConfiguration(cfg => cfg.AddInMemoryCollection(_additionalConfiguration))
            .ConfigureTestServices(_configureServices)
            .Configure(_configure);
    }
}
