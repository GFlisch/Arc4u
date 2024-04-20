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

    public static async ValueTask<Result<T>> ValidateWithResultAsync<T>(this IValidator<T> validator, T value)
    {
        return await validator.ValidateWithResultAsync(value, CancellationToken.None).ConfigureAwait(false);
    }

    public static async ValueTask<Result<T>> ValidateWithResultAsync<T>(this IValidator<T> validator, T value, CancellationToken cancellation)
    {
        var validationResult = await validator.ValidateAsync(value, cancellation).ConfigureAwait(false);

        if (validationResult.IsValid)
        {
            return Result.Ok(value);
        }

        return Result.Fail(validationResult.Errors.ToFluentResultErrors());
    }

    public static Result<T> ValidateWithResult<T>(this IValidator<T> validator, T value)
    {
        var validationResult = validator.Validate(value);

        if (validationResult.IsValid)
        {
            return Result.Ok(value);
        }

        return Result.Fail(validationResult.Errors.ToFluentResultErrors());
    }
}
