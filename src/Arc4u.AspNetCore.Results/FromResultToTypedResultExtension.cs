#if NET8_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using Arc4u.Results;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Arc4u.AspNetCore.Results;

public static class FromResultToTypedResultExtension
{
    #region ActionResult

    #region ValueTask<Result<T>>

    public static async ValueTask<Results<Ok<TResult>, ProblemHttpResult, ValidationProblem>>
   ToTypedOkResultAsync<TResult>(this ValueTask<Result<TResult>> result)
    {

        var res = await result.ConfigureAwait(false);

        Results<Ok<TResult>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Ok(value))
            .OnSuccessNull(() => objectResult = TypedResults.Ok(default(TResult)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<Results<Ok<T>, ProblemHttpResult, ValidationProblem>>
    ToTypedOkResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        Results<Ok<T>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Ok(mapper(value)))
            .OnSuccessNull(() => objectResult = TypedResults.Ok(default(T)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<Results<NoContent, ProblemHttpResult, ValidationProblem>>
    ToTypedOkResultAsync(this ValueTask<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        Results<NoContent, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccess(() => objectResult = TypedResults.NoContent())
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<Results<Created<T>, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        Results<Created<T>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, mapper(value)))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(T)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<Results<Created<TResult>, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult>(this ValueTask<Result<TResult>> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        Results<Created<TResult>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, value))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(TResult)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<Results<Created, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult>(this ValueTask<Result> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        Results<Created, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccess(() => objectResult = TypedResults.Created(location))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #region Task<Result> Task<Result<T>>

    public static async Task<Results<Ok<TResult>, ProblemHttpResult, ValidationProblem>>
    ToTypedOkResultAsync<TResult>(this Task<Result<TResult>> result)
    {

        var res = await result.ConfigureAwait(false);

        Results<Ok<TResult>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Ok(value))
            .OnSuccessNull(() => objectResult = TypedResults.Ok(default(TResult)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<Results<Ok<T>, ProblemHttpResult, ValidationProblem>>
    ToTypedOkResultAsync<TResult, T>(this Task<Result<TResult>> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        Results<Ok<T>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Ok(mapper(value)))
            .OnSuccessNull(() => objectResult = TypedResults.Ok(default(T)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<Results<NoContent, ProblemHttpResult, ValidationProblem>>
    ToTypedOkResultAsync(this Task<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        Results<NoContent, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccess(() => objectResult = TypedResults.NoContent())
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<Results<NoContent, ProblemHttpResult, ValidationProblem>>
    ToTypedOkResultAsync(this Result result)
    {
        Results<NoContent, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();

        result
              .OnSuccess(() => objectResult = TypedResults.NoContent())
              .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static async Task<Results<Created<T>, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult, T>(this Task<Result<TResult>> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        Results<Created<T>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, mapper(value)))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(T)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<Results<Created<TResult>, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult>(this Task<Result<TResult>> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        Results<Created<TResult>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, value))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(TResult)))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<Results<Created<T>, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult, T>(this Result<TResult> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        Results<Created<T>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, mapper(value)))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(T)))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static Task<Results<Created<TResult>, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult>(this Result<TResult> result, Uri? location)
    {
        Results<Created<TResult>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, value))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(TResult)))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static async Task<Results<Created, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult>(this Task<Result> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        Results<Created, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        res
            .OnSuccess(() => objectResult = TypedResults.Created(location))
            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<Results<Created, ProblemHttpResult, ValidationProblem>>
    ToTypedCreatedResultAsync<TResult>(this Result result, Uri? location)
    {
        Results<Created, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccess(() => objectResult = TypedResults.Created(location))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    #endregion

    #region Result<T>

    public static Results<Ok<TResult>, ProblemHttpResult, ValidationProblem>
    ToTypedOkResult<TResult>(this Result<TResult> result)
    {

        Results<Ok<TResult>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccessNotNull(value => objectResult = TypedResults.Ok(value))
            .OnSuccessNull(() => objectResult = TypedResults.Ok(default(TResult)))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static Results<Ok<T>, ProblemHttpResult, ValidationProblem>
    ToTypedOkResult<TResult, T>(this Result<TResult> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        Results<Ok<T>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccessNotNull(value => objectResult = TypedResults.Ok(mapper(value)))
            .OnSuccessNull(() => objectResult = TypedResults.Ok(default(T)))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static Results<Created<T>, ProblemHttpResult, ValidationProblem>
    ToTypedCreatedResult<TResult, T>(this Result<TResult> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        Results<Created<T>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, mapper(value)))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(T)))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static Results<Created<TResult>, ProblemHttpResult, ValidationProblem>
    ToTypedCreatedResult<TResult>(this Result<TResult> result, Uri? location)
    {
        Results<Created<TResult>, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccessNotNull(value => objectResult = TypedResults.Created(location, value))
            .OnSuccessNull(() => objectResult = TypedResults.Created((Uri?)null, default(TResult)))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }
    #endregion

    #region Result
    public static Results<NoContent, ProblemHttpResult, ValidationProblem>
    ToTypedOkResult(this Result result)
    {
        Results<NoContent, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccess(() => objectResult = TypedResults.NoContent())
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    public static Results<Created, ProblemHttpResult, ValidationProblem>
    ToTypedCreatedResult(this Result result, Uri? location)
    {
        Results<Created, ProblemHttpResult, ValidationProblem> objectResult = TypedResults.Problem();
        result
            .OnSuccess(() => objectResult = TypedResults.Created(location))
            .OnFailed(errors => objectResult = TypedResults.Problem(result.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #endregion
}

#endif
