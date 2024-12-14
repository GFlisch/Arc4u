using System.Reflection.Metadata;

namespace Arc4u.Dependency.Tool;
internal sealed class DummyCustomAttributeProvider : ICustomAttributeTypeProvider<object?>
{
    public static readonly DummyCustomAttributeProvider Instance = new();

    public object? GetPrimitiveType(PrimitiveTypeCode typeCode) => null;

    public object? GetSystemType() => null;

    public object? GetSZArrayType(object? elementType) => null;

    public object? GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => null;

    public object? GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => null;

    public object? GetTypeFromSerializedName(string name)
    {
        var components = name.Split(',');
        if (components.Length > 0)
        {
            return new TypeInfo(components[0]);
        }
        else
        {
            throw new ArgumentException("The name must refer to a fully qualified name and contain at least one comma.", nameof(name));
        }
    }

    public PrimitiveTypeCode GetUnderlyingEnumType(object? type) => default;

    public bool IsSystemType(object? type) => true;
}
