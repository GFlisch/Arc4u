using System;

namespace Arc4u.Dependency.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SharedAttribute : System.Attribute
    {
        /// <summary>
        /// Mark a part as globally shared => singleton.
        /// </summary>
        public SharedAttribute()
        {
        }

    }
}
