using FluentResults;
using FluentValidation;
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

    public static async ValueTask<Result<T>> ValidateWithResultAsync<T>(this AbstractValidator<T> validator, T value)
    {
        var validationResult = await validator.ValidateAsync(value).ConfigureAwait(false);

        if (validationResult.IsValid)
        {
            return Result.Ok(value);
        }

        return Result.Fail(validationResult.Errors.ToFluentResultErrors());
    }

    public static Result<T> ValidateWithResult<T>(this AbstractValidator<T> validator, T value)
    {
        var validationResult = validator.Validate(value);

        if (validationResult.IsValid)
        {
            return Result.Ok(value);
        }

        return Result.Fail(validationResult.Errors.ToFluentResultErrors());
    }
}
