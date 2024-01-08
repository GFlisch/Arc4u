using FluentValidation.Results;

namespace Arc4u.Results.Validation;

public static class ValidationExtensions
{
    public static Dictionary<string, object> ToMetadata(this ValidationFailure failure)
    {
        var metadata = new Dictionary<string, object>
        {
            { "Code", failure.ErrorCode },
            { "State", failure.CustomState },
            { "PropertyName", failure.PropertyName },
            { "Severity", failure.Severity }
        };
        return metadata;
    }

    public static List<ValidationError> ToFluentResultErrors(this IEnumerable<ValidationFailure> failures)
    {
        return failures.Select(failure => (ValidationError)failure).ToList();
    }
}
