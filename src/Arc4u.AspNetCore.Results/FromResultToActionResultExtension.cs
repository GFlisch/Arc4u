using System.Diagnostics.CodeAnalysis;
using Arc4u.Results;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arc4u.AspNetCore.Results;
public static class FromResultToActionResultExtension
{
    #region ActionResult

    #region ValueTask<Result<T>>

    public static async ValueTask<ActionResult<TResult>>
    ToActionOkResultAsync<TResult>(this ValueTask<Result<TResult>> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new OkObjectResult(value))
            .OnSuccessNull(() => objectResult = new OkObjectResult(default(TResult)))
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<T>>
    ToActionOkResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new OkObjectResult(mapper(value)))
            .OnSuccessNull(() => objectResult = new OkObjectResult(default(TResult)))
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult>
    ToActionOkResultAsync(this ValueTask<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccess(() => objectResult = new NoContentResult())
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<T>>
    ToActionCreatedResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, mapper(value)))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(T))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult>>
    ToActionCreatedResultAsync<TResult>(this ValueTask<Result<TResult>> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, value))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(TResult))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #region Task<Result<T>>

    public static async Task<ActionResult<TResult>>
    ToActionOkResultAsync<TResult>(this Task<Result<TResult>> result)
    {

        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new OkObjectResult(value))
            .OnSuccessNull(() => objectResult = new OkObjectResult(default(TResult)))
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<ActionResult<T>>
    ToActionOkResultAsync<TResult, T>(this Task<Result<TResult>> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new OkObjectResult(mapper(value)))
            .OnSuccessNull(() => objectResult = new OkObjectResult(default(TResult)))
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<ActionResult>
    ToActionOkResultAsync(this Task<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccess(() => objectResult = new NoContentResult())
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<ActionResult>
    ToActionOkResultAsync(this Result result)
    {
        ActionResult objectResult = new BadRequestResult();

        result
            .OnSuccess(() => objectResult = new NoContentResult())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static async Task<ActionResult<T>>
    ToActionCreatedResultAsync<TResult, T>(this Task<Result<TResult>> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, mapper(value)))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(T))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async Task<ActionResult<TResult>>
    ToActionCreatedResultAsync<TResult>(this Task<Result<TResult>> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<TResult> objectResult = new BadRequestResult();
        res
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, value))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(TResult))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<ActionResult>
    ToActionCreatedResultAsync<TResult, T>(this Result<TResult> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        ActionResult objectResult = new BadRequestResult();
        result
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, mapper(value)))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(T))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static Task<ActionResult>
    ToActionCreatedResultAsync<TResult>(this Result<TResult> result, Uri? location)
    {
        ActionResult objectResult = new BadRequestResult();
        result
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, value))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(TResult))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static async Task<ActionResult>
    ToActionCreatedResultAsync<TResult>(this Task<Result> result, Uri? location)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = new BadRequestResult();
        res
            .OnSuccess(() => objectResult = new CreatedResult(location, null))
            .OnFailed(_ => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static Task<ActionResult>
    ToActionCreatedResultAsync<TResult>(this Result result, Uri? location)
    {
        ActionResult objectResult = new BadRequestResult();
        result
            .OnSuccess(() => objectResult = new CreatedResult(location, null))
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    #endregion

    #region Result<T>

    public static ActionResult<TResult>
    ToActionOkResult<TResult>(this Result<TResult> result)
    {
        ActionResult<TResult> objectResult = new BadRequestResult();
        result
            .OnSuccessNotNull(value => objectResult = new OkObjectResult(value))
            .OnSuccessNull(() => objectResult = new OkObjectResult(default(TResult)))
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return objectResult;
    }

    public static ActionResult<T>
    ToActionOkResult<TResult, T>(this Result<TResult> result, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        ActionResult<T> objectResult = new BadRequestResult();
        result
            .OnSuccessNotNull(value => objectResult = new OkObjectResult(mapper(value)))
            .OnSuccessNull(() => objectResult = new OkObjectResult(default(TResult)))
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return objectResult;
    }

    public static ActionResult<T>
    ToActionCreatedResult<TResult, T>(this Result<TResult> result, Uri? location, [DisallowNull] Func<TResult, T> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        ActionResult<T> objectResult = new BadRequestResult();
        result
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, mapper(value)))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(T))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return objectResult;
    }

    public static ActionResult<TResult>
    ToActionCreatedResult<TResult>(this Result<TResult> result, Uri? location)
    {
        ActionResult<TResult> objectResult = new BadRequestResult();
        result
            .OnSuccessNotNull(value => objectResult = new CreatedResult(location, value))
            .OnSuccessNull(() => objectResult = new ObjectResult(default(TResult))
            {
                StatusCode = StatusCodes.Status201Created
            })
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #region Result

    public static ActionResult
    ToActionOkResult(this Result result)
    {
        ActionResult objectResult = new BadRequestResult();

        result
            .OnSuccess(() => objectResult = new NoContentResult())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return objectResult;
    }

    public static ActionResult
    ToActionCreatedResult(this Result result, Uri? location)
    {
        ActionResult objectResult = new BadRequestResult();
        result
            .OnSuccess(() => objectResult = new CreatedResult(location, null))
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return objectResult;
    }

    #endregion

    #endregion
}
