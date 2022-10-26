using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency.ComponentModel
{
    public class NameResolver: INameResolver
    {
        private readonly Dictionary<(string Name, Type ServiceType), List<Type>> _types;
        private readonly Dictionary<(string Name, Type ServiceType), List<object>> _instances;
        private bool _frozen;

        public NameResolver()
        {
            _types = new Dictionary<(string Name, Type ServiceType), List<Type>>();
            _instances = new Dictionary<(string Name, Type ServiceType), List<object>>();
        }

        public void Freeze()
        {
            _frozen = true;
        }

        public bool TryGetInstances(string name, Type serviceType, out IReadOnlyList<object> instances)
        {
            if (_instances.TryGetValue((name, serviceType), out var values))
            {
                instances = values;
                return true;
            }
            else
            {
                instances = null;
                return false;
            }    
        }

        public bool TryGetTypes(string name, Type serviceType, out IReadOnlyList<Type> types)
        {
            if (_types.TryGetValue((name, serviceType), out var values))
            {
                types = values;
                return true;
            }
            else
            {
                types = null; 
                return false;
            }
        }

        public ServiceDescriptor Add(ServiceDescriptor serviceDescriptor, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (serviceDescriptor == null)
                throw new ArgumentNullException(nameof(serviceDescriptor));
            if (_frozen)
                throw new InvalidOperationException("You can't add a service descriptor on a frozen resolver. A named service collection has already been created.");

            var key = (name, serviceDescriptor.ServiceType);

            if (serviceDescriptor.Lifetime == ServiceLifetime.Singleton && serviceDescriptor.ImplementationInstance != null)
            {
                if (_instances.TryGetValue(key, out var list))
                {
                    if (!list.Contains(serviceDescriptor.ImplementationInstance))
                        list.Add(serviceDescriptor.ImplementationInstance);
                }
                else
                    _instances.Add(key, new List<object> { serviceDescriptor.ImplementationInstance });
                // no descriptor to add to the service collection
                return null;
            }
            else if (serviceDescriptor.ImplementationType == null)
                throw new ArgumentException($"For named services, only implementation types are supported. No factories.");
            else
            {
                // use the regular service collection to register the implementation type, appropriately scoped.
                if (_types.TryGetValue(key, out var list))
                {
                    if (!list.Contains(serviceDescriptor.ImplementationType))
                        list.Add(serviceDescriptor.ImplementationType);
                }
                else
                    _types.Add(key, new List<Type> { serviceDescriptor.ImplementationType });
                // add this descriptor to the service collection
                return new ServiceDescriptor(serviceDescriptor.ImplementationType, serviceDescriptor.ImplementationType, serviceDescriptor.Lifetime);
            }
        }
    }
}
