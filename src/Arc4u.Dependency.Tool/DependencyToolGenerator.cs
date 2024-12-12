using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Arc4u.Dependency.Tool;

[Generator]
public class DependencyToolGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }
#endif

        // Get class declarations having the Export attribute.
        var exportClasses = context.SyntaxProvider
                                    .CreateSyntaxProvider(
                                        predicate: static (s, _) => s is ClassDeclarationSyntax c,
                                        transform: static (ctx, _) => GetClassInfo(ctx))
                                    .Where(m => m is not null);

        var compilationAndData = context.CompilationProvider
                                        .Combine(exportClasses.Collect());

        context.RegisterSourceOutput(
                                     compilationAndData,
                                     (spc, source) =>
                                     {
                                         spc.AddSource("Arc4u.Dependencies.g.cs", SourceText.From(GenerateRegisterTypes(source.Left.AssemblyName, source.Right), Encoding.UTF8));
                                     });
    }

    private string GenerateRegisterTypes(string? assemblyName, ImmutableArray<(INamedTypeSymbol Type, Location Location)?> classes)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine();
        sb.AppendLine("namespace Arc4u.Dependency;");
        sb.AppendLine();
        sb.AppendLine("public static partial class RegisterExtensions");
        sb.AppendLine("{");
        if (assemblyName is not null)
        {
            sb.AppendLine($"    public static void Register{assemblyName.Split('.').Last().Trim()}Types(this IServiceCollection services)");
            sb.AppendLine("    {");
            foreach (var _class in classes)
            {
                // Read the Export attribute information.
                var type = _class!.Value.Type.GetAttributes().Select(a => a.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToList();
                var exportAttribute = _class?.Type.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Arc4u.Dependency.Attribute.ExportAttribute");
                var isScoped = IsScoped(_class?.Type);
                var isShared = IsShared(_class?.Type);

                if (exportAttribute is not null)
                {
                    var (contractName, contractType) = GetExportAttribute(exportAttribute);
                    var action = isScoped ? "Scoped" : isShared ? "Singleton" : "Transient";

                    if (contractType is null)
                    {
                        if (null != contractName)
                        {
                            sb.AppendLine($"        services.AddKeyed{action}<{_class?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(\"{contractName}\");");
                        }
                        else
                        {
                            sb.AppendLine($"        services.Add{action}<{_class?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();");
                        }
                    }
                    else
                    {
                        if (null != contractName)
                        {
                            sb.AppendLine($"        services.AddKeyed{action}<{contractType}, {_class?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(\"{contractName}\");");
                        }
                        else
                        {
                            sb.AppendLine($"        services.Add{action}<{contractType}, {_class?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();");
                        }

                    }
                }
            }
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    // Fetch the ContractName property value from the Export attribute.
    private static (string? contractName, string? contractType) GetExportAttribute(AttributeData attribute)
    {
        var constructorArguments = attribute.ConstructorArguments;
        string? contractName = null;
        string? contractType = null;

        // Iterate over the constructor arguments and retrieve their values
        foreach (var constructorArgument in constructorArguments)
        {
            if (constructorArgument.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "string")
            {
                contractName = constructorArgument.Value?.ToString();
            }
            if (constructorArgument.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Type")
            {
                contractType = constructorArgument.Value?.ToString();
            }
        }

        return (contractName, contractType);
    }

    private static bool IsScoped(INamedTypeSymbol? _class)
    {
        return _class?.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Arc4u.Dependency.Attribute.ScopedAttribute") is not null;
    }

    private static bool IsShared(INamedTypeSymbol? _class)
    {
        return _class?.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Arc4u.Dependency.Attribute.SharedAttribute") is not null;
    }

    private static (INamedTypeSymbol Type, Location Location)? GetClassInfo(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return null;
        }

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                if (name is "Export")
                {
                    var symbol = (INamedTypeSymbol?)context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                    if (symbol != null)
                    {
                        return (symbol, classDeclaration.GetLocation());
                    }
                }
            }
        }

        return null;
    }
}
