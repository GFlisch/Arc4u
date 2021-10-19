using System;
using System.Collections.Generic;

namespace Arc4u.Dependency.ComponentModel
{
    public class MultipleRegistrationException : MultipleRegistrationException<object>
    {
        public MultipleRegistrationException(Type type, IEnumerable<object> instances) : base(instances) { }
    }
}
