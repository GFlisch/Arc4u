using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Arc4u.AspNetCore.Results;
using Arc4u.Results.Validation;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arc4u.Results;

public static class ProblemDetailExtension
{
    public static Func<IError, ProblemDetails> FromError => error => _fromError(error);

    public static void SetFromErrorFactory(Func<IError, ProblemDetails> fromError)
    {
        _fromError = fromError;
    }
    private static Func<IError, ProblemDetails> _fromError = _from;

    private static ProblemDetails _from(IError error)
    {
        ProblemDetails? problem = null;

        if (error is ValidationError validationError)
        {
            problem = new ProblemDetails()
                        .WithTitle("Error from validation.")
                        .WithDetail(validationError.Message)
                        .WithStatusCode(StatusCodes.Status422UnprocessableEntity)
                        .WithSeverity(validationError.Severity.ToString())
                        .WithType(new Uri("https://github.com/GFlisch/Arc4u/wiki/StatusCodes#validation-error"))
                        .WithCode(validationError.Code);
        }

        if (error is ProblemDetailError problemDetail)
        {
            problem = new ProblemDetails()
                        .WithTitle(problemDetail.Title ?? "Error.")
                        .WithDetail(problemDetail.Message)
                        .WithStatusCode(problemDetail.StatusCode ?? StatusCodes.Status500InternalServerError)
                        .WithSeverity(problemDetail.Severity ?? Severity.Error.ToString())
                        .WithType(problemDetail.Type ?? new Uri("about:blank"));
        }

        if (null == problem)
        {
            problem = new ProblemDetails()
                    .WithTitle("Error.")
                    .WithDetail(error.Message)
                    .WithStatusCode(StatusCodes.Status500InternalServerError)
                    .WithType(new Uri("about:blank"))
                    .WithSeverity(Severity.Error.ToString());
        }

        foreach (var metadata in error.Metadata)
        {
            problem.WithMetadata(metadata.Key, metadata.Value);
        }

        return problem;
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
        if (result.IsFailed)
        {
            result.Log();

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

        return new ProblemDetails()
            .WithTitle("A technical error occured!")
            .WithDetail("Contact the application owner.")
            .WithType(new Uri("about:blank"))
            .WithStatusCode(StatusCodes.Status500InternalServerError);

    }

    /// <summary>
    /// If Success, return the reasons!
    /// If Failure and no exceptions, return the Errors: Message, Code, Severity.
    /// If Failure and exceptions, Log and return the generic messages.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public static List<ProblemDetails> ToProblemDetails<TResult>(this Result<TResult> result)
    {
        if (result.IsSuccess)
        {
            return result.Successes.Select(reason => new ProblemDetails().WithDetail(reason.Message).WithSeverity(Severity.Info.ToString())).ToList();
        }

        if (result.IsFailed && result.Errors.OfType<IExceptionalError>().Any())
        {
            result.Log();
            return new List<ProblemDetails>(new[] { result.ToGenericMessage() });
        }
        
        return result.Errors.Select(e => ProblemDetailExtension.FromError(e)).ToList();

    }

    /// <summary>
    /// If Success, return the reasons!
    /// If Failure and no exceptions, return the Errors: Message, Code, Severity.
    /// If Failure and exceptions, Log and return the generic messages.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public static List<ProblemDetails> ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            return result.Successes.Select(reason => new ProblemDetails().WithDetail(reason.Message).WithSeverity(Severity.Info.ToString())).ToList();
        }

        if (result.IsFailed && result.Errors.OfType<IExceptionalError>().Any())
        {
            result.Log();
            return new List<ProblemDetails>([result.ToGenericMessage()]);
        }

        return result.Errors.Select(error => ProblemDetailExtension.FromError(error)).ToList();

    }
}
