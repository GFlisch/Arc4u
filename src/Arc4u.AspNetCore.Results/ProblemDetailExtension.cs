using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Arc4u.AspNetCore.Results;
using Arc4u.Results.Validation;
using Arc4u.ServiceModel;
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
        if (error is ValidationError validationError)
        {
            return new ProblemDetails()
                .WithTitle("Error from validation.")
                .WithDetail(validationError.Message)
                .WithStatusCode(StatusCodes.Status422UnprocessableEntity)
                .WithSeverity(validationError.Severity.ToString());
        }

        return new ProblemDetails()
                .WithTitle("Error.")
                .WithDetail(error.Message)
                .WithStatusCode(StatusCodes.Status400BadRequest)
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
        if (result.IsFailed)
        {
            result.Log();

            if (activityId is not null)
            {
                return new ProblemDetails()
                    .WithTitle("A technical error occured!")
                    .WithDetail($"Contact the application owner. A message has been logged with id: {activityId}.")
                    .WithStatusCode(StatusCodes.Status400BadRequest);
            }

            return new ProblemDetails()
                    .WithTitle("A technical error occured!")
                    .WithDetail("Contact the application owner. A message has been logged.")
                    .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        return new ProblemDetails()
            .WithTitle("A technical error occured!")
            .WithDetail("Contact the application owner.")
            .WithStatusCode(StatusCodes.Status400BadRequest);

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
            return new List<ProblemDetails>();
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
            return result.Reasons.Select(reason => new ProblemDetails().WithDetail(reason.Message).WithSeverity(Severity.Info.ToString())).ToList();
        }

        if (result.IsFailed && result.Errors.OfType<IExceptionalError>().Any())
        {
            result.Log();
            return new List<ProblemDetails>([result.ToGenericMessage()]);
        }

        return result.Errors.Select(error => ProblemDetailExtension.FromError(error)).ToList();

    }
}
