namespace Arc4u.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RelationAttribute : Attribute
    {
        public string NavigationProperty { get; set; }

        public string TargetRelationProperty { get; set; }
    }
}
