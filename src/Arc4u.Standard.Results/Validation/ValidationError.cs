using FluentResults;
using FluentValidation;
using FluentValidation.Results;

namespace Arc4u.Results.Validation;

public class ValidationError : Error
{
    public ValidationError(ValidationFailure failure)
    {
        _failure = failure;
        Message = failure.ErrorMessage;
        foreach (var m in failure.ToMetadata())
        {
            Metadata.Add(m.Key, m.Value);
        }
    }

    private readonly ValidationFailure _failure;

    public string Code => _failure.ErrorCode;
    public Severity Severity => _failure.Severity;

    public static implicit operator ValidationError(ValidationFailure failure) => new ValidationError(failure);

}

