using Arc4u.Dependency.Attribute;
using Arc4u.Dependency.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arc4u.Dependency
{
    public static class ContainerInitializerExtention
    {
        private static readonly object locker = new object();

        public static IServiceCollection InitializeFromConfig(this IServiceCollection container, IConfiguration configuration)
        {
            var dependencies = new Dependencies();
            configuration.Bind("Application.Dependency", dependencies);

            // if the assembly contains rejected types => we have to extract all types from the assembly, register the good one only!.
            var assemblies = GetAssembliesFromConfig(dependencies.Assemblies, out var types);
            types.AddRange(GetRegisterTypesFromConfig(dependencies.RegisterTypes));

            // the lock here is a bit strange, but we replicate existing behavior just in case
            // note that it is unnecessary to lock anything else.
            lock (locker)
            {
                container.AddExportableTypes(types);
                container.AddExportableTypes(assemblies);
            }

            return container;
        }

        [Obsolete("You can call InitializeFromConfig directly on a IServiceCollection")]
        public static IContainer InitializeFromConfig(this IContainer container, IConfiguration configuration)
        {
            var dependencies = new Dependencies();
            configuration.Bind("Application.Dependency", dependencies);

            LoadFromConfig(dependencies, container);

            return container;
        }

        private static void LoadFromConfig(Dependencies dependencies, IContainer container)
        {
            lock (locker)
            {
                // Assert is not null.
                if (null == dependencies)
                    throw new ArgumentNullException(nameof(Dependencies));

                // if the assembly contains rejected types => we have to extract all types from the assembly, register the good one only!.
                var assemblies = GetAssembliesFromConfig(dependencies.Assemblies, out var types);
                types.AddRange(GetRegisterTypesFromConfig(dependencies.RegisterTypes));

                container.AddExportableTypes(types);
                container.AddExportableTypes(assemblies);
            }
        }

        private static List<Assembly> GetAssembliesFromConfig(ICollection<AssemblyConfig> assemblies, out List<Type> types)
        {
            types = new List<Type>();
            var result = new List<Assembly>();

            if (null == assemblies) return result;

            foreach (var assembly in assemblies)
            {
                if (assembly.RejectedTypes?.Count > 0) // fill types selected (!rejected).
                {
                    types.AddRange(GetTypesFromAssembly(assembly.Assembly).FilterList(assembly.RejectedTypes));
                }
                else // full types in the assembly
                {
                    var a = Assembly.Load(assembly.Assembly);
                    if (null != a)
                        result.Add(a);
                }
            }

            return result;
        }

        private static List<Type> GetRegisterTypesFromConfig(ICollection<String> types)
        {
            var _types = new List<Type>();

            if (null == types) return _types;

            foreach (var type in types)
            {
                var t = Type.GetType(type);
                if (null != t)
                    _types.Add(t);
            }
            return _types;

        }

        private static IEnumerable<Type> GetTypesFromAssembly(string assembly)
        {
            var _assembly = Assembly.Load(assembly);

            return from type in _assembly.GetTypes()
                    where type.CustomAttributes.Any(a => a.AttributeType == typeof(ExportAttribute))
                    select type;
        }

        private static IEnumerable<Type> FilterList(this IEnumerable<Type> types, IEnumerable<String> rejectedTypes)
        {
            // the case-insensitive comparison is against language rules, but we need to remain compatible with the old behavior.
            return types.Where(type => !rejectedTypes.Contains(type.FullName, StringComparer.InvariantCultureIgnoreCase)).ToList();
        }
    }
}

