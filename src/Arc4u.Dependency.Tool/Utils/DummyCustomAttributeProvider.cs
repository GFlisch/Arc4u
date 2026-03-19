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
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("The name must refer to a fully qualified name and contain at least one comma.", nameof(name));
        }

        return ParseSerializedTypeName(name);
    }

    /// <summary>
    /// Parses a .NET serialized type name, handling generic types with any number of type arguments.
    /// Format for non-generic: "Namespace.Name, Assembly"
    /// Format for generic: "Namespace.Name`N[[Arg1TypeName, Arg1Assembly, ...],[Arg2TypeName, Arg2Assembly, ...]], Assembly"
    /// </summary>
    private static TypeInfo ParseSerializedTypeName(string name)
    {
        var backtickIndex = name.IndexOf('`');
        if (backtickIndex == -1)
        {
            // Non-generic type: extract the type name before the first comma (assembly separator)
            var commaIndex = name.IndexOf(',');
            var fullName = commaIndex >= 0 ? name.Substring(0, commaIndex).Trim() : name.Trim();
            return new TypeInfo(fullName);
        }

        // Generic type: extract the base type name (before the backtick)
        var baseFullName = name.Substring(0, backtickIndex);

        // Find the start of the generic arguments (first '[[')
        var argsStart = name.IndexOf("[[", backtickIndex, StringComparison.Ordinal);
        if (argsStart == -1)
        {
            // Malformed generic, fall back to treating as non-generic
            return new TypeInfo(baseFullName);
        }

        // Parse each generic type argument enclosed in [ ]
        var genericArgs = new List<TypeInfo>();
        var i = argsStart + 1; // position after the outer '['
        while (i < name.Length)
        {
            if (name[i] == '[')
            {
                // Find the matching closing bracket, accounting for nested generics
                var depth = 1;
                var argStart = i + 1;
                i++;
                while (i < name.Length && depth > 0)
                {
                    if (name[i] == '[') depth++;
                    else if (name[i] == ']') depth--;
                    i++;
                }
                var argContent = name.Substring(argStart, i - argStart - 1);
                genericArgs.Add(ParseSerializedTypeName(argContent));
            }
            else if (name[i] == ']')
            {
                // End of the outer generic argument list
                break;
            }
            else
            {
                i++;
            }
        }

        // Split the base name into namespace and name
        var lastDotIndex = baseFullName.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            throw new ArgumentException($"The type name '{baseFullName}' must contain a namespace.", nameof(name));
        }

        var ns = baseFullName.Substring(0, lastDotIndex);
        var typeName = baseFullName.Substring(lastDotIndex + 1);

        return new TypeInfo(ns, typeName, genericArgs);
    }

    public PrimitiveTypeCode GetUnderlyingEnumType(object? type) => default;

    public bool IsSystemType(object? type) => true;
}
