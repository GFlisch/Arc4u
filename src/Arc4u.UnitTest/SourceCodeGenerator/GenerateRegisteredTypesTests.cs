#if NET9_0
using System.Collections.Immutable;
using System.Reflection;
using Arc4u.Dependency.Tool;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Arc4u.UnitTest.SourceCodeGenerator;


public class GenerateRegisteredTypesTests
{
    private const string DddRegistryText = $@"
{{
""Application.Dependency"": {{
    ""RegisterTypes"": [
      ""Arc4u.AppSettings, Arc4u.Configuration"",
      ""Arc4u.Diagnostics.DefaultLoggingProperties, Arc4u""
    ]
  }}
}}";

    [Fact]
    [Trait("Category", "CI")]
    public void GenerateClassesBasedOnDDDRegistry()
    {
        // Create an instance of the source generator.
        var generator = new GenerateRegisteredTypes();

        // Source generators should be tested using 'GeneratorDriver'.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);


        // Add the additional file separately from the compilation.
        driver = driver.AddAdditionalTexts(
            ImmutableArray.Create<AdditionalText>(
                new TestAdditionalFile(Path.Combine(Directory.GetCurrentDirectory(), "Configs/appsettings.json"), DddRegistryText))
        );

        var dummySourceText = SourceText.From("public class Dummy {}");
        var dummySyntaxTree = CSharpSyntaxTree.ParseText(
            dummySourceText,
            path: Path.Combine(Directory.GetCurrentDirectory(), "Dummy.cs")
        );

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Arc4u.AppSettings).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Arc4u.Diagnostics.DefaultLoggingProperties).Assembly.Location)
        };

        // To run generators, we can use an empty compilation.
        // I have to create a Compiler with Assemblies to test the GenerateRegisteredTypes
        var compilation = CSharpCompilation.Create(nameof(GenerateRegisteredTypes), [dummySyntaxTree], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out _);

        // Retrieve the generate types file.
        var generatedFiles = newCompilation.SyntaxTrees
                                                    .Where(t => Path.GetFileName(t.FilePath).Equals("GeneratedTypes.g.cs"));

        generatedFiles.Should().NotBeNull();
        generatedFiles.Should().HaveCount(1);

        var generatedFile = generatedFiles.First();

        // check we have the 2 registrations!
        generatedFile.ToString().Should().Contain("public static void RegisterTypes(this IServiceCollection services)");
        generatedFile.ToString().Should().Contain("services.AddSingleton<Arc4u.IAppSettings, Arc4u.AppSettings>();");
        generatedFile.ToString().Should().Contain("services.AddKeyedScoped<Arc4u.Diagnostics.IAddPropertiesToLog, Arc4u.Diagnostics.DefaultLoggingProperties>(\"Scoped\");");
    }
}
#endif

