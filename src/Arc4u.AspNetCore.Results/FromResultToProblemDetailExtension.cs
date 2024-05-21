using System.Collections.Immutable;
using System.Diagnostics;
using Arc4u.Results;
using Arc4u.Results.Validation;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arc4u.AspNetCore.Results;

public static class FromResultToProblemDetailExtension
{
    public static Func<IEnumerable<IError>, ProblemDetails> FromError => errors => _fromErrors(errors);

    public static void SetFromErrorFactory(Func<IEnumerable<IError>, ProblemDetails> fromErrors)
    {
        _fromErrors = fromErrors;
    }
    private static Func<IEnumerable<IError>, ProblemDetails> _fromErrors = _from;

    private static ProblemDetails _from(IEnumerable<IError> errors)
    {
        if (errors.OfType<ValidationError>().Any())
        {
            //TODO: what to do when Code is null.
            var orderedErrors = errors.OfType<ValidationError>()
                                      .OrderBy(x => x.Code)
                                      .ThenBy(x => x.Severity)
                                      .GroupBy(x => x.Code)
                                      .ToImmutableSortedDictionary(g => g.Key, g => g.ToImmutableList().Select(vError => $"{vError.Severity}: {vError.Message}").ToArray());

            return new ValidationProblemDetails(orderedErrors)
                        .WithTitle("Error from validation.")
                        .WithStatusCode(StatusCodes.Status422UnprocessableEntity)
                        .WithType(new Uri("https://github.com/GFlisch/Arc4u/wiki/StatusCodes#validation-error"));
        }

        if (errors.OfType<ProblemDetailError>().Any())
        {
            var problemDetailError = errors.OfType<ProblemDetailError>().First();
            var problem = new ProblemDetails()
                        .WithTitle(problemDetailError.Title ?? "Error.")
                        .WithDetail(problemDetailError.Message)
                        .WithStatusCode(problemDetailError.StatusCode ?? StatusCodes.Status500InternalServerError)
                        .WithSeverity(problemDetailError.Severity ?? Severity.Error.ToString())
                        .WithType(problemDetailError.Type ?? new Uri("about:blank"));

            foreach (var metadata in problemDetailError.Metadata)
            {
                problem.WithMetadata(metadata.Key, metadata.Value);
            }

            return problem;

        }

        var error = errors.First();
        return new ProblemDetails()
                    .WithTitle("Error.")
                    .WithDetail(error.Message)
                    .WithStatusCode(StatusCodes.Status500InternalServerError)
                    .WithType(new Uri("about:blank"))
                    .WithSeverity(Severity.Error.ToString());
    }

    public static ProblemDetails ToGenericMessage<TResult>(this Result<TResult> result)
    {
        return ToGenericMessage(result, Activity.Current?.Id);
    }

    public static ProblemDetails ToGenericMessage<TResult>(this Result<TResult> result, string? activityId)
    {
        return result.ToResult().ToGenericMessage(activityId);
    }

    public static ProblemDetails ToGenericMessage(this Result result)
    {
        return result.ToGenericMessage(Activity.Current?.Id);
    }

    public static ProblemDetails ToGenericMessage(this Result result, string? activityId)
    {
        result.LogIfFailed();

        if (activityId is not null)
        {
            return new ProblemDetails()
                .WithTitle("A technical error occured!")
                .WithDetail($"Contact the application owner. A message has been logged with id: {activityId}.")
                .WithType(new Uri("about:blank"))
                .WithStatusCode(StatusCodes.Status500InternalServerError);
        }

        return new ProblemDetails()
                .WithTitle("A technical error occured!")
                .WithDetail("Contact the application owner. A message has been logged.")
                .WithType(new Uri("about:blank"))
                .WithStatusCode(StatusCodes.Status500InternalServerError);

    }

    /// <summary>
    /// If Failure and no exceptions, return the Errors: Message, Code, Severity.
    /// If Failure and exceptions, Log and return the generic messages.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public static ProblemDetails ToProblemDetails<TResult>(this Result<TResult> result)
    {
        // Could not be a valid scenario to call this method! An Exceptional error will be created so the developer is aware of this.
        if (result.IsSuccess)
        {
#if NET6_0
            result.WithError(new ExceptionalError(new Exception("Creating a ProblemDetails on a success Result does not make any sense!")));
#else
            result.WithError(new ExceptionalError(new UnreachableException("Creating a ProblemDetails on a success Result does not make any sense!")));
#endif
        }

        // Generate a generic message and log! No sensitive information can be sent outside directly to a user =. vulnerabilities.
        if (result.IsFailed && result.Errors.OfType<IExceptionalError>().Any())
        {
            return result.ToGenericMessage();
        }

        return FromResultToProblemDetailExtension.FromError(result.Errors);

    }

    /// <summary>
    /// If Success, return the reasons!
    /// If Failure and no exceptions, return the Errors: Message, Code, Severity.
    /// If Failure and exceptions, Log and return the generic messages.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public static ProblemDetails ToProblemDetails(this Result result)
    {
        // Could not be a valid scenario to call this method! An Exceptional error will be created so the developer is aware of this.
        if (result.IsSuccess)
        {
#if NET6_0
            result.WithError(new ExceptionalError(new Exception("Creating a ProblemDetails on a success Result does not make any sense!")));
#else
            result.WithError(new ExceptionalError(new UnreachableException("Creating a ProblemDetails on a success Result does not make any sense!")));
#endif
        }

        if (result.IsFailed && result.Errors.OfType<IExceptionalError>().Any())
        {
            return result.ToGenericMessage();
        }

        return FromResultToProblemDetailExtension.FromError(result.Errors);

    }
}
