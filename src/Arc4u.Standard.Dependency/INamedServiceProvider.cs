using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    /// <summary>
    /// A service provider which also provides named service resolution.
    /// It cannot be as "frugal" as <see cref="IServiceProvider"/>, since we have unique scenarios to implement.
    /// </summary>
    public interface INamedServiceProvider : IServiceProvider
    {
        /// <summary>
        /// This method is a bit of a misnomer: 
        /// If <paramref name="name"/> is not null,  it behaves like "GetRequiredServices" since an exception will be thrown if <paramref name="type"/> is not a known service type.
        /// If <paramref name="name"/> is null, it behaves like GetServices.
        /// This is how the old implementation behaved, and we need to replicate that.
        /// Normally, we would disallow <paramref name="name"/> to be null and avoid such behavior.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerable<object> GetServices(Type type, string name);

        bool TryGetService(Type type, string name, bool throwIfError, out object value);

        INamedServiceScope CreateScope();
    }
}
