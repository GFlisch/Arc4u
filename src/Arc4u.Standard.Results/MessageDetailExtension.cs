using System.Diagnostics;
using Arc4u.Results.Validation;
using Arc4u.ServiceModel;
using FluentResults;
using FluentValidation;

namespace Arc4u.Results;

public static class MessageDetailExtension
{
    public static Func<IError, MessageDetail> FromError => error => _fromError(error);

    public static void SetFromErrorFactory(Func<IError, MessageDetail> fromError)
    {
        _fromError = fromError;
    }
    private static Func<IError, MessageDetail> _fromError = _from;

    private static MessageDetail _from(IError error)
    {
        if (error is ValidationError validationError)
        {
            return new MessageDetail(validationError.Message, validationError.Code, validationError.Severity switch
            {
                Severity.Warning => MessageType.Warning,
                Severity.Info => MessageType.Information,
                _ => MessageType.Error
            });
        }

        return new MessageDetail(error.Message, string.Empty, MessageType.Error);
    }

    public static MessageDetail ToGenericMessage<TResult>(this Result<TResult> result)
    {
        return ToGenericMessage(result, Activity.Current?.Id);
    }

    public static MessageDetail ToGenericMessage<TResult>(this Result<TResult> result, string? activityId)
    {
        if (result.IsFailed)
        {
            result.Log();

            if (activityId is not null)
            {
                return new MessageDetail($"A technical error occured, contact the application owner. A message has been logged with id: {activityId}.");
            }

            return new MessageDetail($"A technical error occured, contact the application owner. A message has been logged.");

        }

        return new MessageDetail($"A technical error occured, contact the application owner.");
    }

    public static MessageDetail ToGenericMessage(this Result result)
    {
        return result.ToGenericMessage(Activity.Current?.Id);
    }

    public static MessageDetail ToGenericMessage(this Result result, string? activityId)
    {
        if (result.IsFailed)
        {
            result.Log();

            if (activityId is not null)
            {
                return new MessageDetail($"A technical error occured, contact the application owner. A message has been logged with id: {activityId}.");
            }

            return new MessageDetail($"A technical error occured, contact the application owner. A message has been logged.");

        }

        return new MessageDetail($"A technical error occured, contact the application owner.");
    }

    /// <summary>
    /// If Success, return the reasons!
    /// If Failure and no exceptions, return the Errors: Message, Code, Severity.
    /// If Failure and exceptions, Log and return the generic messages.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public static List<MessageDetail> ToMessageDetails<TResult>(this Result<TResult> result)
    {
        if (result.IsSuccess)
        {
            return result.Reasons.Select(reason => new MessageDetail(reason.Message, string.Empty, MessageType.Information)).ToList();
        }

        if (result.IsFailed && result.Errors.OfType<IExceptionalError>().Any())
        {
            result.Log();
            return new List<MessageDetail>(new[] { result.ToGenericMessage() });
        }
        
        return result.Errors.Select(e => MessageDetailExtension.FromError(e)).ToList();

    }

    /// <summary>
    /// If Success, return the reasons!
    /// If Failure and no exceptions, return the Errors: Message, Code, Severity.
    /// If Failure and exceptions, Log and return the generic messages.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public static List<MessageDetail> ToMessageDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            return result.Reasons.Select(reason => new MessageDetail(reason.Message, string.Empty, MessageType.Information)).ToList();
        }

        if (result.IsFailed && result.Errors.OfType<IExceptionalError>().Any())
        {
            result.Log();
            return new List<MessageDetail>(new[] { result.ToGenericMessage() });
        }

        return result.Errors.Select(error => MessageDetailExtension.FromError(error)).ToList();

    }
}
