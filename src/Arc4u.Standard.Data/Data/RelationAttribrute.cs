using System;

namespace Arc4u.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RelationAttribute : Attribute
    {
        public String NavigationProperty { get; set; }

        public String TargetRelationProperty { get; set; }
    }
}
