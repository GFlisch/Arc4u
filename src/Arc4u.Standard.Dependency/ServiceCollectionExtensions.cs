using Arc4u.Dependency.Attribute;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arc4u.Dependency
{
    /// <summary>
    /// Provide a way to explicitly add types and assemblies to the service collection.
    /// This is compatible with <see cref="IServiceCollection"/>, but named services are only supported if the service collection has been properly initialized by calling AddNamedServicesSupport()
    /// This allows the use of named services even when we are only passing a <see cref="IServiceCollection"/> around.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private static void AddExportableType(this IServiceCollection services, Type implementationType, bool throwOnError = false)
        {
            if (implementationType.IsClass && !implementationType.IsAbstract)
            {
                var exports = implementationType.GetCustomAttributes<ExportAttribute>();
                if (exports.Any())
                {
                    var shared = implementationType.GetCustomAttribute<SharedAttribute>();
                    var scoped = implementationType.GetCustomAttribute<ScopedAttribute>();
                    // Normally, shared and scoped cannot both be defined for the same type. For backwards compatibility with the old code, we don't throw an exception but we interpret this as "Shared": 
                    // This is bad practice!
                    //if (shared is not null && scoped is not null)
                    //    throw new Exception($"{implementationType.FullName} has both [Shared] and [Scoped] attributes. Choose one.");
                    ServiceLifetime lifetime;
                    if (shared is not null)
                        lifetime = ServiceLifetime.Singleton;
                    else if (scoped is not null)
                        lifetime = ServiceLifetime.Scoped;
                    else
                        lifetime = ServiceLifetime.Transient;
                    foreach (var export in exports)
                    {
                        var serviceDescriptor = new ServiceDescriptor(export.ContractType ?? implementationType, implementationType, lifetime);
                        if (export.ContractName is null)
                            services.Add(serviceDescriptor);
                        else
                            services.Add(serviceDescriptor, export.ContractName);
                    }
                }
                else if (throwOnError)
                    throw new Exception($"Type {implementationType.GetType().Name} is not exportable. It must be a non-abstract reference type");
            }
        }


        private static IServiceCollection Add(this IServiceCollection services, IEnumerable<Type> implementationTypes, bool throwOnError = false) 
        {
            foreach (var implementationType in implementationTypes)
                services.AddExportableType(implementationType, throwOnError);
            return services;
        }


        public static IServiceCollection AddExportableTypes(this IServiceCollection services, IEnumerable<Type> implementationTypes) 
        {
            return services.Add(implementationTypes, throwOnError: true);
        }

        public static IServiceCollection AddExportableTypes(this IServiceCollection services, IEnumerable<Assembly> assemblies) 
        {
            return services.Add(GetAllTypes(), throwOnError: false);

            IEnumerable<Type> GetAllTypes()
            {
                foreach (var assembly in assemblies)
                    foreach (var type in assembly.GetTypes())
                        yield return type;
            }
        }
    }
}
