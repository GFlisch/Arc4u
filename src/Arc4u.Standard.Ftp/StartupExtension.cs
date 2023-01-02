using System;
using Arc4u.Configuration;
using Arc4u.Diagnostics;
using Arc4u.Extensions;
using Arc4u.Network.Pooling;
using Arc4u.Security.Cryptography;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Arc4u.Standard.Ftp;

[PublicAPI]
public static class StartupExtension
{
    /// <summary>
    ///     Registering everything needed to use an sFTP pooling
    /// </summary>
    /// <param name="serviceCollection">serviceCollection to be used for registration</param>
    /// <returns>provided <see cref="ServiceCollection" /> in parameter <paramref name="serviceCollection" /></returns>
    /// <remarks>
    ///     The configuration being used internally is the <see cref="FtpConfiguration" />. This will be registered within
    ///     the <paramref name="serviceCollection" /> as <see cref="IFtpConfiguration" />
    /// </remarks>
    public static IServiceCollection UseFtpPooling(this IServiceCollection serviceCollection)
    {
        return UseFtpPooling<FtpConfiguration>(serviceCollection);
    }

    /// <summary>
    ///     Registering everything needed to use an sFTP pooling
    /// </summary>
    /// <typeparam name="TConfiguration">
    ///     Type of ftp configuration to be used and registered within the
    ///     <paramref name="serviceCollection" /> as <see cref="IFtpConfiguration" />
    /// </typeparam>
    /// <param name="serviceCollection">serviceCollection to be used for registration</param>
    /// <returns>provided <see cref="ServiceCollection" /> in parameter <paramref name="serviceCollection" /></returns>
    /// <remarks>
    ///     The configuration will be read and registered  within the <paramref name="serviceCollection" /> as
    ///     <see cref="IFtpConfiguration" />
    /// </remarks>
    public static IServiceCollection UseFtpPooling<TConfiguration>(this IServiceCollection serviceCollection)
        where TConfiguration : IFtpConfiguration, new()
    {
        serviceCollection.AddSingleton<IFtpConfiguration>(provider =>
        {
            var decryptionProvider = provider.GetService<IDecryptionProvider>();
            return (provider.GetService<IConfiguration>() ??
                    throw new InvalidOperationException(
                        $"{typeof(IConfiguration)} not found within the DI container, although it was "))
                .GetAndDecrypt<TConfiguration>(decryptionProvider);
        });
        return RegisterServices(serviceCollection);
    }

    /// <summary>
    ///     Registering everything needed to use an sFTP pooling
    /// </summary>
    /// <param name="serviceCollection">serviceCollection to be used for registration</param>
    /// <param name="configuration">ftp configuration to be used and registered for the ftp pooling</param>
    /// <returns>provided <see cref="ServiceCollection" /> in parameter <paramref name="serviceCollection" /></returns>
    /// <remarks>
    ///     The configuration will registered  within the <paramref name="serviceCollection" /> as
    ///     <see cref="IFtpConfiguration" />
    /// </remarks>
    public static IServiceCollection UseFtpPooling(this IServiceCollection serviceCollection,
        IFtpConfiguration configuration)
    {
        serviceCollection.AddSingleton(configuration);
        return RegisterServices(serviceCollection);
    }

    private static IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<Func<SftpClient>>(provider =>
        {
            return () =>
            {
                var ftpConfig = provider.GetService<IFtpConfiguration>() ??
                                throw new ConfigurationException(
                                    $"{nameof(IFtpConfiguration)} was not found in the DI");
                var logger = provider.GetService<ILogger<SftpClient>>() ??
                             throw new ConfigurationException($"{nameof(ILogger<SftpClient>)} was not found in the DI");
                ;
                var c = new SftpClient(new ConnectionInfo(ftpConfig.Host, ftpConfig.Username,
                    new PasswordAuthenticationMethod(ftpConfig.Username, ftpConfig.Password)));
                c.KeepAliveInterval = ftpConfig.KeepAliveInterval;
                logger.Technical().Debug("Connecting to SFTP host...").Log();
                c.Connect();
                logger.Technical().Debug("Connected.").Log();
                return c;
            };
        });

        serviceCollection.AddSingleton<IClientFactory<SftpClientFacade>, SftpClientFactory>();
        serviceCollection.AddSingleton<ConnectionPool<SftpClientFacade>>();
        return serviceCollection;
    }
}