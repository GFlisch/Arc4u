using System;
using System.Linq;
using System.Reflection;

namespace Arc4u.Dependency.Attribute
{
    /// <summary>
    /// Registers in the DI container every classes decorated with the <see cref="ExportAttribute"/>.
    /// <see cref="AttributeInspector"/> determines the lifetime of the registration
    /// based on the presence of the <see cref="SharedAttribute"/> or <see cref="ScopedAttribute"/>.
    /// If none of these attributes are present, the lifetime is transient.
    /// </summary>
    public class AttributeInspector
    {
        public AttributeInspector(IContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        private readonly IContainer _container;

        /// <summary>
        /// Registers a type in the DI container if it is decorated with the <see cref="ExportAttribute"/>.
        /// The lifetime of the registration is determined by the presence of the <see cref="SharedAttribute"/>
        /// or <see cref="ScopedAttribute"/>.
        /// If none of these attributes are present, the lifetime is transient.
        /// </summary>
        /// <param name="type">The type to register.</param>
        public void Register(Type type)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(type);
#else
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
#endif
            if (!type.CustomAttributes.Any())
            {
                return;
            }

            var exports = type.GetCustomAttributes<ExportAttribute>().ToArray();

            if (exports.Length == 0)
            {
                return;
            }

            // Check if we have a SharedAttribute, ScopeAttribute or nothing.
            var isSingleton = type.CustomAttributes.Any(a => a.AttributeType == typeof(SharedAttribute));
            var isScoped = type.CustomAttributes.Any(a => a.AttributeType == typeof(ScopedAttribute));

            foreach (var export in exports)
            {
                var fromType = export.ContractType ?? type;
                var contractName = export.ContractName;

                if (isScoped)
                {
                    if (string.IsNullOrWhiteSpace(contractName))
                    {
                        _container.RegisterScoped(fromType, type);
                    }
                    else
                    {
                        _container.RegisterScoped(fromType, type, contractName);
                    }
                }
                else if (isSingleton)
                {
                    if (string.IsNullOrWhiteSpace(contractName))
                    {
                        _container.RegisterSingleton(fromType, type);
                    }
                    else
                    {
                        _container.RegisterSingleton(fromType, type, contractName);
                    }
                }
                else if (string.IsNullOrWhiteSpace(contractName))
                {
                    _container.Register(fromType, type);
                }
                else
                {
                    _container.Register(fromType, type, contractName);
                }
            }
        }

        public void Register(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(t => CanBeExported(t)).ToList();

            foreach (var type in types)
            {
                Register(type);
            }
        }

        private static bool CanBeExported(Type type) => type.IsClass && !type.IsAbstract && type.CustomAttributes.Count() > 0;
    }
}
