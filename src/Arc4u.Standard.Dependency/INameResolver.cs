using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    public interface INameResolver
    {
        bool TryGetInstances(string name, Type serviceType, out IReadOnlyList<object> instances);
        bool TryGetTypes(string name, Type serviceType, out IReadOnlyList<Type> types);

        void Freeze();
        ServiceDescriptor Add(ServiceDescriptor serviceDescriptor, string name);
    }
}
