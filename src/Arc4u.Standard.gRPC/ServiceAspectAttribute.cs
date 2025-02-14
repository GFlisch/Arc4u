namespace Arc4u.gRPC;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ServiceAspectAttribute : Attribute
{
    public ServiceAspectAttribute(params int[] operations) : this(string.Empty, operations)
    {
    }

    public ServiceAspectAttribute(string scope, params int[] operations)
    {
        Scope = string.IsNullOrWhiteSpace(scope) ? string.Empty : scope.Trim();
        Operations = operations;
    }

    public string Scope { get; set; }

    public int[] Operations { get; set; }

    public static ServiceAspectAttribute Empty() => new ServiceAspectAttribute([]);
}
