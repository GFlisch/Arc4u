namespace Arc4u.Data;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RelationAttribute : Attribute
{
    public string NavigationProperty { get; set; } = String.Empty;

    public string TargetRelationProperty { get; set; } = String.Empty;
}
