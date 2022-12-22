using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    public interface INameResolver
    {
        ServiceDescriptor Add(ServiceDescriptor serviceDescriptor, string name);

        void Freeze();

        IEnumerable<object> GetServices(IServiceProvider provider, Type type, string name);

        bool TryGetService(IServiceProvider provider, Type type, string name, bool throwIfError, out object value);
    }
}
