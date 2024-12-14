namespace Arc4u.Dependency.Tool;
sealed class ExportDescription
{
    public ExportDescription(TypeInfo service, TypeInfo implementation, bool isScoped, bool isShared, string? contractName)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));
        Implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
        IsScoped = isScoped;
        IsShared = isShared;
        ContractName = contractName;
    }

    public TypeInfo Service { get; private set; }
    public TypeInfo Implementation { get; private set; }
    public bool IsScoped { get; private set; }
    public bool IsShared { get; private set; }

    public string? ContractName { get; private set; }
}
