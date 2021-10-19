using System;
using System.Linq;
using System.Reflection;

namespace Arc4u.Dependency.Attribute
{
    /// <summary>
    /// Parse the type and check if Export, Shared or Scoped attribute exist.
    /// If Shared and Scoped are defined => Shared is used! Normally this is not allowed.
    /// </summary>
    public class AttributeInspector
    {
        public AttributeInspector(IContainer container)
        {
            if (null == container)
                throw new ArgumentNullException(nameof(container));

            _container = container;
        }

        private IContainer _container;

        /// <summary>
        /// Parse the attributes and register in the container.
        /// </summary>
        /// <param name="type"></param>
        public void Register(Type type)
        {
            if (null == type)
                throw new ArgumentNullException(nameof(type));

            if (type.CustomAttributes.Count() == 0)
                return;

            var exports = type.CustomAttributes.Where(a => a.AttributeType.Name.Equals("ExportAttribute")).ToList();

            if (exports.Count == 0) return;

            // Check if we have a SharedAttribute.
            var shared = type.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name.Equals("SharedAttribute"));
            var scoped = type.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name.Equals("ScopedAttribute"));

            bool isSingleton = null != shared;
            bool isScoped = null != scoped;

            foreach (var export in exports)
            {
                var fromArgument = export.ConstructorArguments.FirstOrDefault(a => a.ArgumentType.Name.Equals("Type"));
                Type from = null == fromArgument.Value ? type : (Type)fromArgument.Value;
                Type to = type;
                var nameArgument = export.ConstructorArguments.FirstOrDefault(a => a.ArgumentType.Name.Equals("String"));
                var name = (string)nameArgument.Value;

                if (isScoped)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        _container.RegisterScoped(from, to);
                    else
                        _container.RegisterScoped(from, to, name);

                    continue;
                }

                if (isSingleton)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        _container.RegisterSingleton(from, to);
                    else
                        _container.RegisterSingleton(from, to, name);

                    continue;
                }

                if (string.IsNullOrWhiteSpace(name))
                    _container.Register(from, to);
                else
                    _container.Register(from, to, name);
            }


        }

        public void Register(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(t => CanBeExported(t)).ToList();

            foreach (var type in types)
                Register(type);
        }

        private static bool CanBeExported(Type type) => type.IsClass && !type.IsAbstract && type.CustomAttributes.Count() > 0;
    }
}
