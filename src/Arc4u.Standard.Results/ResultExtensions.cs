using System.Diagnostics.CodeAnalysis;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Arc4u.Results;
public static class ResultExtension
{
    #region OnSuccess

    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess)
        {
            action();
        }
        return result;
    }

    public static Result<TValue> OnSuccess<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value);
        }
        return result;
    }

    public static Result<TValue> OnSuccess<TValue>(this Result<TValue> result, Action action)
    {
        if (result.IsSuccess)
        {
            action();
        }
        return result;
    }

    public static async Task<Result> OnSuccess(this Task<Result> result, Action action)
    {
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action();
        }
        return r;
    }

    public static async ValueTask<Result<TValue>> OnSuccess<TValue>(this ValueTask<Result<TValue>> result, Action<TValue> action)
    {
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action(r.Value);
        }
        return r;
    }



    public static async Task<Result> OnSuccessAsync(this Task<Result> result, Func<Task> action)
    {
        if (result.Result.IsSuccess)
        {
            await action().ConfigureAwait(false);
        }

        return result.Result;
    }
    public static async Task<Result> OnSuccessAsync(this Task<Result> result, Func<Task<Result>> action)
    {
        if (result.Result.IsSuccess)
        {
            return await action().ConfigureAwait(false);
        }

        return result.Result;
    }

    public static async ValueTask<Result<TValue>> OnSuccessAsync<TValue>(this ValueTask<Result<TValue>> result, Func<TValue, ValueTask<Result<TValue>>> action)
    {
        var r = await result.ConfigureAwait(false);
        if (r.IsSuccess)
        {
            return await action(r.Value).ConfigureAwait(false);
        }

        return r;
    }

    public static async ValueTask<Result<TValue>> OnSuccessAsync<TValue>(this ValueTask<Result<TValue>> result, Func<TValue, Task> action)
    {
        var r = await result.ConfigureAwait(false);
        if (r.IsSuccess)
        {
            await action(r.Value).ConfigureAwait(false);
        }

        return r;
    }

    #endregion

    #region OnFailed

    public static Result OnFailed(this Result result, Action<List<IError>> action)
    {
        if (result.IsFailed)
        {
            action(result.Errors);
        }
        return result;
    }

    public static Result OnFailed(this Result result, Result globalResult)
    {
        if (result.IsFailed)
        {
            globalResult.WithErrors(result.Errors);
        }

        return result;
    }

    public static Result<TValue> OnFailed<TValue>(this Result<TValue> result, [DisallowNull] Result<TValue> globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        if (result.IsFailed)
        {
            globalResult.WithErrors(result.Errors);
        }

        return result;
    }
    public static Result<TValue> OnFailed<TValue>(this Result<TValue> result, Action<List<IError>> action)
    {
        if (result.IsFailed)
        {
            action(result.Errors);
        }
        return result;
    }

    public static Result<TValue> OnFailed<TValue>(this Result<TValue> result, Result globalResult)
    {
        if (result.IsFailed)
        {
            globalResult.WithErrors(result.Errors);
        }
        return result;
    }

    public static async Task<Result> OnFailed(this Task<Result> result, [DisallowNull] Result globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);
        ArgumentNullException.ThrowIfNull(result);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(r.Errors);
        }

        return r;
    }
    public static async Task<Result> OnFailed<TReturnValue>(this Task<Result> result, Result<TReturnValue> globalResult)
    {
        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(r.Errors);
        }

        return r;
    }

    public static Task<Result> OnFailed(this Task<Result> result, Action<List<IError>> action)
    {
        if (result.Result.IsFailed)
        {
            action(result.Result.Errors);
        }

        return result;
    }

    public static async ValueTask<Result<TValue>> OnFailed<TValue>(this ValueTask<Result<TValue>> result, Result<TValue> globalResult)
    {
        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(result!.Result.Errors);
        }

        return r;
    }

    public static async ValueTask<Result<TValue>> OnFailed<TValue>(this ValueTask<Result<TValue>> result, Result globalResult)
    {
        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(result!.Result.Errors);
        }

        return r;
    }

    public static async ValueTask<Result<TValue>> OnFailed<TValue>(this ValueTask<Result<TValue>> result, Action<List<IError>> action)
    {
        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            action(r.Errors);
        }
        return r;
    }

    public static async Task<Result> OnFailedAsync(this Task<Result> result, Func<List<IError>, Task> action)
    {
        if (result.Result.IsFailed)
        {
            await action(result.Result.Errors).ConfigureAwait(false);
        }
        return result.Result;
    }

    public static async ValueTask<Result<TValue>> OnFailedAsync<TValue>(this ValueTask<Result<TValue>> result, Func<List<IError>, ValueTask<Result<TValue>>> action)
    {
        var r = await result.ConfigureAwait(false);
        if (r.IsFailed)
        {
            await action(r.Errors).ConfigureAwait(false);
        }
        return r;
    }


    #endregion

    public static Task<Result> LogIfFailedAsync(this Task<Result> result, LogLevel logLevel = LogLevel.Information)
    {
        result.Result.LogIfFailed(logLevel);

        return result;
    }

    public static async Task<Result> LogIfFailedAsync(this Task<Result> result, Result globalResult, LogLevel logLevel = LogLevel.Information)
    {
        var r = await result.ConfigureAwait(false);
        r.LogIfFailed(logLevel);

        if (r.IsFailed)
        {
            globalResult.WithErrors(r.Errors);
        }

        return r;
    }

    public static async ValueTask<Result<TValue>> LogIfFailedAsync<TValue>(this ValueTask<Result<TValue>> result, LogLevel logLevel = LogLevel.Information)
    {
        var r = await result.ConfigureAwait(false);

        r.LogIfFailed(logLevel);
        return r;
    }

    public static async ValueTask<Result<TValue>> LogIfFailedAsync<TValue>(this ValueTask<Result<TValue>> result, Result<TValue> globalResult, LogLevel logLevel = LogLevel.Information)
    {
        var r = await result.ConfigureAwait(false);
        r.LogIfFailed(logLevel);

        if (r.IsFailed)
        {
            globalResult.WithErrors(r.Errors);
        }

        return r;
    }
}
