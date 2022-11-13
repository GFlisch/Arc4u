using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private bool TryGetInstances(string name, Type serviceType, out IReadOnlyList<object> instances)
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

        private bool TryGetTypes(string name, Type serviceType, out IReadOnlyList<Type> types)
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

        public IEnumerable<object> GetServices(IServiceProvider provider, Type type, string name)
        {
            // we should really disallow this.
            if (name == null)
                return provider.GetServices(type);

            if (TryGetInstances(name, type, out var singletons))
                return singletons;

            if (!TryGetTypes(name, type, out var to))
                throw new NullReferenceException($"No type found registered with the name: {name}.");

            var instances = Array.CreateInstance(type, to.Count);
            for (int index = 0; index < to.Count; ++index)
                instances.SetValue(provider.GetService(to[index]), index);

            // covariance at work here. This is really a IEnumerable<type>
            return (IEnumerable<object>)instances;
        }

        public bool TryGetService(IServiceProvider provider, Type type, string name, bool throwIfError, out object value)
        {
            value = null;

            if (name == null)
                // we should really disallow this
                value = provider.GetService(type);
            else
            {
                IEnumerable<object> instances;

                if (TryGetInstances(name, type, out var oto))
                {
                    if (oto.Count != 1)
                        if (throwIfError)
                            throw new IndexOutOfRangeException($"More than one instance type is registered for name {name}.");
                        else
                            return false;
                    instances = oto;
                }
                else
                {
                    if (!TryGetTypes(name, type, out var to))
                        if (throwIfError)
                            throw new NullReferenceException($"No type found registered with the name: {name}.");
                        else
                            return false;

                    if (to.Count != 1)
                        if (throwIfError)
                            throw new IndexOutOfRangeException($"More than one type is registered for name {name}.");
                        else
                            return false;

                    instances = provider.GetServices(to[0]);
                }

                // the happy flow is the single-instance case. Don't waste time counting
                try
                {
                    value = instances.SingleOrDefault();
                }
                catch (Exception e)
                {
                    if (throwIfError)
                        throw new MultipleRegistrationException(type, instances, e);
                    else
                        return false;
                }
            }
            return value != null;
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
