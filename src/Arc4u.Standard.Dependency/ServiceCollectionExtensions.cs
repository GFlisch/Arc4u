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
                void throwForbiddenDoubleUsage() => throw new Exception($"{implementationType.FullName} has both [Shared] and [Scoped] attributes. Choose one.");
                
                // note that we look up attribute by name, since ExportAttribute and SharedAttribute are also defined in System.ComponentModel.Composition
                // which we don't include here, but can be used in other assemblies defining exportable types
                var exports = implementationType.GetCustomAttributes().Where(attribute => attribute.GetType().Name == "ExportAttribute");
                if (exports.Any())
                {
                    ServiceLifetime lifetime = ServiceLifetime.Transient;

                    // iterate through the custom attributes once to get both Shared and Scoped attributes if they are present
                    foreach (var attribute in implementationType.GetCustomAttributes())
                        if (attribute.GetType().Name == "SharedAttribute")
                            if (ServiceLifetime.Scoped == lifetime)
                                throwForbiddenDoubleUsage();
                            else
                                lifetime = ServiceLifetime.Singleton;
                        else if (attribute.GetType().Name == "ScopedAttribute")  // we also test for ScopedAttribute by name, even though it's currently only we who are defining it.
                            if (ServiceLifetime.Singleton == lifetime)
                                throwForbiddenDoubleUsage();
                            else
                                lifetime = ServiceLifetime.Scoped;

                    foreach (var export in exports)
                    {
                        const string ContractType = "ContractType";
                        const string ContractName = "ContractName";
                        var contractTypeProperty = export.GetType().GetProperty(ContractType);
                        if(contractTypeProperty is null)
                            throw new MissingMemberException(export.GetType().Name, ContractType);
                        var contractNameProperty = export.GetType().GetProperty(ContractName);
                        if (contractNameProperty is null)
                            throw new MissingMemberException(export.GetType().Name, ContractName);
                        var contractType = (Type)contractTypeProperty.GetValue(export);
                        var contractName = (string)contractNameProperty.GetValue(export);    
                        var serviceDescriptor = new ServiceDescriptor(contractType ?? implementationType, implementationType, lifetime);
                        if (contractName is null)
                            services.Add(serviceDescriptor);
                        else
                            services.Add(serviceDescriptor, contractName);
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
