using System;
using Arc4u.Extensions;
using Arc4u.Network.Pooling;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Standard.Ftp;

public static class StartupExtension
{
    public static IServiceCollection UseFtpPooling(this IServiceCollection serviceCollection)
    {
        return UseFtpPooling<FtpConfiguration>(serviceCollection);
    }


    public static IServiceCollection UseFtpPooling<TConfiguration>(this IServiceCollection serviceCollection)
        where TConfiguration : IFtpConfiguration, new()
    {
        serviceCollection.AddSingleton<IFtpConfiguration>(provider =>
        {
            var decryptionProvider = provider.GetService<IDecryptionProvider>();
            return (provider.GetService<IConfiguration>() ?? throw new InvalidOperationException($"{typeof(IConfiguration)} not found within the DI container, although it was ")).GetAndDecrypt<TConfiguration>(decryptionProvider);
        });
        return RegisterServices(serviceCollection);
    }

    private static IServiceCollection RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IClientFactory<SftpClientFacade>, SftpClientFactory>();
        serviceCollection.AddSingleton<ConnectionPool<SftpClientFacade>>();
        return serviceCollection;
    }

    public static IServiceCollection UseFtpPooling<TConfiguration>(this IServiceCollection serviceCollection,
        IFtpConfiguration configuration)
    {
        serviceCollection.AddSingleton(configuration);
        return RegisterServices(serviceCollection);
    }
}