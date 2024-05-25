#if NET8_0_OR_GREATER

using FluentResults;
using System.Diagnostics.CodeAnalysis;
using IResult = Microsoft.AspNetCore.Http.IResult;
using HttpResults = Microsoft.AspNetCore.Http.Results;
using Arc4u.Results;

namespace Arc4u.AspNetCore.Results;
public static class FromResultToHttpResultExtension
{
    #region ActionResult

    #region ValueTask<Result<T>>

    public static async ValueTask<IResult>
    ToHttpOkResultAsync<TResult>(this ValueTask<Result<TResult>> result)
    {
        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Ok(value))
            .OnSuccessNull(() => objectResult = HttpResults.Ok())
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<IResult>
    ToHttpOkResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Ok(mapper(value)))
            .OnSuccessNull(() => objectResult = HttpResults.Ok())
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<IResult>
    ToHttpOkResultAsync(this ValueTask<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccess(() => objectResult = HttpResults.NoContent())
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<IResult>
    ToHttpCreatedResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Created(location, mapper(value)))
            .OnSuccessNull(() => objectResult = HttpResults.Created((Uri?)null, default(T)))
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<IResult>
    ToHttpCreatedResultAsync<TResult>(this ValueTask<Result<TResult>> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Created(location, value))
            .OnSuccessNull(() => objectResult = HttpResults.Created((Uri?)null, default(TResult)))
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #region Task<Result<T>>

    public static async Task<IResult>
    ToHttpOkResultAsync<TResult>(this Task<Result<TResult>> result)
    {
        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Ok(value))
            .OnSuccessNull(() => objectResult = HttpResults.Ok())
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<IResult>
    ToHttpOkResultAsync<TResult, T>(this Task<Result<TResult>> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Ok(mapper(value)))
            .OnSuccessNull(() => objectResult = HttpResults.Ok())
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<IResult>
    ToHttpOkResultAsync(this Task<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccess(() => objectResult = HttpResults.NoContent())
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<IResult>
    ToHttpOkResultAsync(this Result result)
    {
        IResult objectResult = HttpResults.BadRequest();

        result
            .OnSuccess(() => objectResult = HttpResults.NoContent())
            .OnFailed(_ => objectResult = HttpResults.Problem(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static async Task<IResult>
    ToHttpCreatedResultAsync<TResult, T>(this Task<Result<TResult>> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Created(location, mapper(value)))
            .OnSuccessNull(() => objectResult = HttpResults.Created((Uri?)null, default(T)))
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<IResult>
    ToHttpCreatedResultAsync<TResult>(this Task<Result<TResult>> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        IResult objectResult = HttpResults.BadRequest();
        res
            .OnSuccessNotNull(value => objectResult = HttpResults.Created(location, value))
            .OnSuccessNull(() => objectResult = HttpResults.Created((Uri?)null, default(TResult)))
            .OnFailed(_ => objectResult = HttpResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #region Result<T>

    public static IResult
    ToHttpOkResult<TResult>(this Result<TResult> result)
    {
        IResult objectResult = HttpResults.BadRequest();
        result
            .OnSuccessNotNull(value => objectResult = HttpResults.Ok(value))
            .OnSuccessNull(() => objectResult = HttpResults.Ok())
            .OnFailed(_ => objectResult = HttpResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static IResult
    ToHttpOkResult<TResult, T>(this Result<TResult> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        IResult objectResult = HttpResults.BadRequest();
        result
            .OnSuccessNotNull(value => objectResult = HttpResults.Ok(mapper(value)))
            .OnSuccessNull(() => objectResult = HttpResults.Ok())
            .OnFailed(_ => objectResult = HttpResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static IResult
    ToHttpOkResult(this Result result)
    {
        IResult objectResult = HttpResults.BadRequest();

        result
            .OnSuccess(() => objectResult = HttpResults.NoContent())
            .OnFailed(_ => objectResult = HttpResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static IResult
    ToHttpCreatedResult<TResult, T>(this Result<TResult> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        IResult objectResult = HttpResults.BadRequest();
        result
            .OnSuccessNotNull(value => objectResult = HttpResults.Created(location, mapper(value)))
            .OnSuccessNull(() => objectResult = HttpResults.Created((Uri?)null, default(T)))
            .OnFailed(_ => objectResult = HttpResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static IResult
    ToHttpCreatedResult<TResult>(this Result<TResult> result, Uri? location)
    {
        IResult objectResult = HttpResults.BadRequest();
        result
            .OnSuccessNotNull(value => objectResult = HttpResults.Created(location, value))
            .OnSuccessNull(() => objectResult = HttpResults.Created((Uri?)null, default(TResult)))
            .OnFailed(_ => objectResult = HttpResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #endregion

}

#endif
