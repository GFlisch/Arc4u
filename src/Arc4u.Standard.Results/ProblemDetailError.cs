using FluentResults;

namespace Arc4u.Results;

/// <summary>
/// Create an Error that will be translated into a ProblemDetails.
/// Detail will be assigned to the existing Message property of <see cref="Error"/>.
/// </summary>
public class ProblemDetailError : Error
{
    private ProblemDetailError()
    {
    }

    private ProblemDetailError(string detail)
    {
        Message = detail;
    }

    public static ProblemDetailError Create(string detail)
    {
        return new ProblemDetailError(detail);
    }

    public string? Title { get; private set; }

    public string? Instance { get; private set; }

    public int? StatusCode { get; private set; }

    public Uri? Type { get; private set; }

    public string? Severity { get; private set; }

    public ProblemDetailError WithType(Uri type)
    {
        Type = type;
        return this;
    }

    public ProblemDetailError WithInstance(string instance)
    {
        Instance = instance;
        return this;
    }

    public ProblemDetailError WithStatusCode(int statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public ProblemDetailError WithTitle(string title)
    {
        Title = title;
        return this;
    }

    public ProblemDetailError WithSeverity(string severity)
    {
        Severity = severity;
        return this;
    }

    public new ProblemDetailError WithMetadata(string key, object value)
    {
        Metadata.TryAdd(key, value);
        return this;
    }

}
