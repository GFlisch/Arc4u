using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arc4u.Dependency.ComponentModel
{
    public class MultipleRegistrationException<T> : Exception
    {
        public MultipleRegistrationException(IEnumerable<T> instances) : base(MakeMessage(instances)) { }

        public MultipleRegistrationException(IEnumerable<T> instances, Exception inner) : base(MakeMessage(instances), inner) { }

        private static string MakeMessage(IEnumerable<T> instances)
        {
            var content = new StringBuilder();
            content.AppendLine($"{instances.Count()} registrations exist for type {typeof(T).Name}.");
            content.AppendLine("Only one is expected.");
            foreach (var instance in instances)
                content.AppendLine($"{instance.GetType().Name} is registered.");
            content.AppendLine("Use ResolveAll instead.");

            return content.ToString();
        }
    }
}
