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
    public static Result OnSuccessAsync(this Result result, Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsSuccess)
        {
            func().Wait();
        }
        return result;
    }

    public static async Task<Result> OnSuccess(this Task<Result> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action();
        }
        return r;
    }
    public static async Task<Result> OnSuccessAsync(this Task<Result> result, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.Result.IsSuccess)
        {
            await action().ConfigureAwait(false);
        }

        return result.Result;
    }

    public static async ValueTask<Result> OnSuccess(this ValueTask<Result> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action();
        }
        return r;
    }
    public static async ValueTask<Result> OnSuccessAsync(this ValueTask<Result> result, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.Result.IsSuccess)
        {
            await action().ConfigureAwait(false);
        }

        return result.Result;
    }

    public static Result<TValue> OnSuccess<TValue>(this Result<TValue> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.IsSuccess)
        {
            action();
        }
        return result;
    }
    public static Result<TValue> OnSuccessAsync<TValue>(this Result<TValue> result, Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsSuccess)
        {
            func().Wait();
        }
        return result;
    }
    public static Result<TValue> OnSuccess<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.IsSuccess)
        {
            action(result.ValueOrDefault);
        }
        return result;
    }
    public static Result<TValue> OnSuccessAsync<TValue>(this Result<TValue> result, Func<TValue, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsSuccess)
        {
            func(result.ValueOrDefault).Wait();
        }
        return result;
    }

    public static async Task<Result<TValue>> OnSuccess<TValue>(this Task<Result<TValue>> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action();
        }
        return r;
    }
    public static async Task<Result<TValue>> OnSuccess<TValue>(this Task<Result<TValue>> result, Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action(r.ValueOrDefault);
        }
        return r;
    }
    public static async Task<Result<TValue>> OnSuccessAsync<TValue>(this Task<Result<TValue>> result, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            await action().ConfigureAwait(false);
        }

        return r;
    }
    public static async Task<Result<TValue>> OnSuccessAsync<TValue>(this Task<Result<TValue>> result, Func<TValue, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            await action(r.Value).ConfigureAwait(false);
        }

        return r;
    }

    public static async ValueTask<Result<TValue>> OnSuccess<TValue>(this ValueTask<Result<TValue>> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action();
        }
        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccess<TValue>(this ValueTask<Result<TValue>> result, Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            action(r.ValueOrDefault);
        }
        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccessAsync<TValue>(this ValueTask<Result<TValue>> result, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            await action().ConfigureAwait(false);
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccessAsync<TValue>(this ValueTask<Result<TValue>> result, Func<TValue, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r.IsSuccess)
        {
            await action(r.Value).ConfigureAwait(false);
        }

        return r;
    }

    #endregion

    #region OnSuccessNull
    public static Result<TValue> OnSuccessNull<TValue>(this Result<TValue> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result is { IsSuccess: true, ValueOrDefault: null })
        {
            action();
        }
        return result;
    }
    public static Result<TValue> OnSuccessNullAsync<TValue>(this Result<TValue> result, Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result is { IsSuccess: true, ValueOrDefault: null })
        {
            func().Wait();
        }
        return result;
    }
    public static async Task<Result<TValue>> OnSuccessNull<TValue>(this Task<Result<TValue>> result,Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: null })
        {
            action();
        }

        return r;
    }
    public static async Task<Result<TValue>> OnSuccessNullAsync<TValue>(this Task<Result<TValue>> result,Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: null })
        {
            await func().ConfigureAwait(false);
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccessNull<TValue>(this ValueTask<Result<TValue>> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: null })
        {
            action();
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccessNullAsync<TValue>(this ValueTask<Result<TValue>> result, Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: null })
        {
            await func().ConfigureAwait(false);
        }

        return r;
    }
    #endregion

    #region OnSuccessNotNull

    public static Result<TValue> OnSuccessNotNull<TValue>(this Result<TValue> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result is { IsSuccess: true, ValueOrDefault: not null })
        {
            action();
        }
        return result;
    }
    public static Result<TValue> OnSuccessNotNullAsync<TValue>(this Result<TValue> result, Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result is { IsSuccess: true, ValueOrDefault: not null })
        {
            func().Wait();
        }
        return result;
    }

    public static Result<TValue> OnSuccessNotNull<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result is { IsSuccess: true, ValueOrDefault: not null })
        {
            action(result.Value);
        }
        return result;
    }
    public static Result<TValue> OnSuccessNotNullAsync<TValue>(this Result<TValue> result, Func<TValue, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result is { IsSuccess: true, ValueOrDefault: not null })
        {
            func(result.Value).Wait();
        }
        return result;
    }

    public static async Task<Result<TValue>> OnSuccessNotNull<TValue>(this Task<Result<TValue>> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            action();
        }

        return r;
    }
    public static async Task<Result<TValue>> OnSuccessNotNull<TValue>(this Task<Result<TValue>> result,Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            action(r.Value);
        }

        return r;
    }
    public static async Task<Result<TValue>> OnSuccessNotNullAsync<TValue>(this Task<Result<TValue>> result,Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            await func().ConfigureAwait(false);
        }

        return r;
    }
    public static async Task<Result<TValue>> OnSuccessNotNullAsync<TValue>(this Task<Result<TValue>> result, Func<TValue, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            await func(r.Value).ConfigureAwait(false);
        }

        return r;
    }

    public static async ValueTask<Result<TValue>> OnSuccessNotNull<TValue>(this ValueTask<Result<TValue>> result, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            action();
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccessNotNull<TValue>(this ValueTask<Result<TValue>> result, Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            action(r.Value);
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccessNotNullAsync<TValue>(this ValueTask<Result<TValue>> result, Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            await func().ConfigureAwait(false);
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnSuccessNotNullAsync<TValue>(this ValueTask<Result<TValue>> result, Func<TValue, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);

        if (r is { IsSuccess: true, ValueOrDefault: not null })
        {
            await func(r.Value).ConfigureAwait(false);
        }

        return r;
    }
    #endregion

    #region OnFailed
    public static Result OnFailed(this Result result, Action<IReadOnlyCollection<IError>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsFailed)
        {
            action(result.Errors);
        }
        return result;
    }
    public static Result OnFailedAsync(this Result result, Func<IReadOnlyCollection<IError>, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsFailed)
        {
            func(result.Errors).Wait();
        }
        return result;
    }
    public static Result OnFailed(this Result result, Result globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        if (result.IsFailed)
        {
            globalResult.WithErrors(result.Errors);
        }

        return result;
    }

    public static Result<TValue> OnFailed<TValue>(this Result<TValue> result, Result<TValue> globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        if (result.IsFailed)
        {
            globalResult.WithErrors(result.Errors);
        }

        return result;
    }
    public static Result<TValue> OnFailed<TValue>(this Result<TValue> result, Action<IReadOnlyCollection<IError>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsFailed)
        {
            action(result.Errors);
        }
        return result;
    }
    public static Result<TValue> OnFailedAsync<TValue>(this Result<TValue> result, Func<IReadOnlyCollection<IError>, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsFailed)
        {
            func(result.Errors).Wait();
        }
        return result;
    }

    public static async Task<Result> OnFailed(this Task<Result> result, Result globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(r.Errors);
        }

        return r;
    }
    public static async Task<Result> OnFailed<TGlobal>(this Task<Result> result, Result<TGlobal> globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(r.Errors);
        }

        return r;
    }
    public static async Task<Result> OnFailed(this Task<Result> result, Action<IReadOnlyCollection<IError>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            action(r.Errors);
        }

        return r;
    }
    public static async Task<Result> OnFailedAsync(this Task<Result> result, Func<IReadOnlyCollection<IError>, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            await func(r.Errors).ConfigureAwait(false);
        }

        return r;
    }

    public static async Task<Result<TValue>> OnFailed<TValue>(this Task<Result<TValue>> result, Result globalResult)
    {
        ArgumentNullException.ThrowIfNull(result);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(result.Result.Errors);
        }

        return r;
    }
    public static async Task<Result<TValue>> OnFailed<TValue>(this Task<Result<TValue>> result, Result<TValue> globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(result.Result.Errors);
        }

        return r;
    }
    public static async Task<Result<TValue>> OnFailed<TValue>(this Task<Result<TValue>> result, Action<IReadOnlyCollection<IError>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            action(r.Errors);
        }
        return r;
    }
    public static async Task<Result<TValue>> OnFailedAsync<TValue>(this Task<Result<TValue>> result, Func<IReadOnlyCollection<IError>, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);
        if (r.IsFailed)
        {
            await func(r.Errors).ConfigureAwait(false);
        }
        return r;
    }

    public static async ValueTask<Result> OnFailed(this ValueTask<Result> result, Result globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(r.Errors);
        }

        return r;
    }
    public static async ValueTask<Result> OnFailed(this ValueTask<Result> result, Action<IReadOnlyCollection<IError>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            action(r.Errors);
        }

        return r;
    }
    public static async ValueTask<Result> OnFailedAsync(this ValueTask<Result> result, Func<IReadOnlyCollection<IError>, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            await func(r.Errors).ConfigureAwait(false);
        }

        return r;
    }

    public static async ValueTask<Result<TValue>> OnFailed<TValue>(this ValueTask<Result<TValue>> result, Result globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(result.Result.Errors);
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnFailed<TValue>(this ValueTask<Result<TValue>> result, Result<TValue> globalResult)
    {
        ArgumentNullException.ThrowIfNull(globalResult);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            globalResult.WithErrors(result.Result.Errors);
        }

        return r;
    }
    public static async ValueTask<Result<TValue>> OnFailed<TValue>(this ValueTask<Result<TValue>> result, Action<IReadOnlyCollection<IError>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var r = await result.ConfigureAwait(false);

        if (r.IsFailed)
        {
            action(r.Errors);
        }
        return r;
    }
    public static async ValueTask<Result<TValue>> OnFailedAsync<TValue>(this ValueTask<Result<TValue>> result, Func<IReadOnlyCollection<IError>, Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var r = await result.ConfigureAwait(false);
        if (r.IsFailed)
        {
            await func(r.Errors).ConfigureAwait(false);
        }
        return r;
    }
    #endregion

    #region LogIfFailed

    public static async Task<Result> LogIfFailed(this Task<Result> result, LogLevel logLevel = LogLevel.Error)
    {
        var r = await result.ConfigureAwait(false);

        r.LogIfFailed(logLevel);

        return r;
    }
    public static async Task<Result<TValue>> LogIfFailed<TValue>(this Task<Result<TValue>> result, LogLevel logLevel = LogLevel.Error)
    {
        var r = await result.ConfigureAwait(false);

        r.LogIfFailed(logLevel);

        return r;
    }

    public static async ValueTask<Result> LogIfFailed(this ValueTask<Result> result, LogLevel logLevel = LogLevel.Error)
    {
        var r = await result.ConfigureAwait(false);

        r.LogIfFailed(logLevel);

        return r;
    }
    public static async ValueTask<Result<TValue>> LogIfFailed<TValue>(this ValueTask<Result<TValue>> result, LogLevel logLevel = LogLevel.Error)
    {
        var r = await result.ConfigureAwait(false);

        r.LogIfFailed(logLevel);

        return r;
    }
    #endregion
}
