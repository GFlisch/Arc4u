using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Arc4u.UnitTest.SourceCodeGenerator;

public class TestAdditionalFile : AdditionalText
{
    private readonly SourceText _jsonText;

    public TestAdditionalFile(string path, string json)
    {
        Path = path;
        _jsonText = SourceText.From(json);
    }

    public override SourceText GetText(CancellationToken cancellationToken = new()) => _jsonText;

    public override string Path { get; }
}
