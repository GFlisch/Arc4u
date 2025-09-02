// using System.Diagnostics.CodeAnalysis;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace Arc4u.Dependency.ComponentModel;
//
// [Obsolete("Use the standard IServiceProvider implementation.")]
// [RequiresUnreferencedCode("Not AOT compatible")]
// [RequiresDynamicCode("Not AOT compatible")]
// public class DependencyFactory : IServiceProviderFactory<IServiceProvider>
// {
//     public IServiceProvider CreateBuilder(IServiceCollection services)
//     {
//         var container = new ComponentModelContainer(services);
//
//         container.CreateContainer();
//
//         return container;
//     }
//
//     public IServiceProvider CreateServiceProvider(IServiceProvider containerBuilder)
//     {
//         return containerBuilder;
//     }
// }
