using System.Reflection;
using Arc4u.Dependency.Configuration;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Dependency
{
    public static class ContainerInitializerExtention
    {
        private static readonly object locker = new object();

        public static IContainer InitializeFromConfig(this IContainer container, IConfiguration configuration)
        {
            var dependencies = new Dependencies();
            configuration.Bind("Application.Dependency", dependencies);

            LoadFromConfig(dependencies, container);

            return container;
        }

        private static void LoadFromConfig(Dependencies dependencies, IContainer container)
        {
            // Assert is not null.
            if (null == dependencies)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            lock (locker)
            {
                // if the assembly contains rejected types => we have to extract all types from the assembly, register the good one only!.
                var assemblies = GetAssembliesFromConfig(dependencies.Assemblies, out var types);
                types.AddRange(GetRegisterTypesFromConfig(dependencies.RegisterTypes));

                container.Initialize(types.ToArray(), assemblies.ToArray());
            }
        }

        private static List<Assembly> GetAssembliesFromConfig(ICollection<AssemblyConfig> assemblies, out List<Type> types)
        {
            types = new List<Type>();
            var result = new List<Assembly>();

            if (null == assemblies)
            {
                return result;
            }

            foreach (var assembly in assemblies)
            {
                if (assembly.RejectedTypes?.Count > 0) // fill types selected (!rejected).
                {
                    types.AddRange(GetTypesFromAssembly(assembly.Assembly).FilterList(assembly.RejectedTypes.ToList()));
                }
                else // full types in the assembly
                {
                    var a = Assembly.Load(assembly.Assembly);
                    if (null != a)
                    {
                        result.Add(a);
                    }
                }
            }

            return result;
        }

        private static List<Type> GetRegisterTypesFromConfig(ICollection<String> types)
        {
            var _types = new List<Type>();

            if (null == types)
            {
                return _types;
            }

            foreach (var type in types)
            {
                var t = Type.GetType(type);
                if (null != t)
                {
                    _types.Add(t);
                }
            }
            return _types;

        }

        private static List<Type> GetTypesFromAssembly(string assembly)
        {
            var _assembly = Assembly.Load(assembly);

            return (from type in _assembly.GetTypes()
                    where type.CustomAttributes.Where(a => a.AttributeType.Name.Equals("ExportAttribute")).Count() > 0
                    select type).ToList();
        }

        public static List<Type> FilterList(this IEnumerable<Type> types, List<String> rejectedTypes)
        {
            return types.Where(type => !rejectedTypes.Contains(type.FullName, StringComparer.InvariantCultureIgnoreCase)).ToList();
        }
    }
}

