using System;

namespace Arc4u.Dependency.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScopedAttribute : System.Attribute
    {
        /// <summary>
        /// Mark a part as scoped (one instance by scope).
        /// </summary>
        public ScopedAttribute()
        {
        }

    }
}
