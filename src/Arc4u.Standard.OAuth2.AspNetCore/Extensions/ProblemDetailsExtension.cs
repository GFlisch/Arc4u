using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Arc4u.Results;
using System;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Http;

namespace Arc4u.OAuth2.AspNetCore.Extensions;
public static class ProblemDetailsExtension
{
#region ActionResult
    /// <summary>
    /// For an asynchronous method returning a Task<Result<T></T>>, we expect a ValueTask.
    /// </summary>
    /// <return>OkResult<<TResult> or  BadRequestObjectResult<ProblemDetails></return>
    public static async ValueTask<ActionResult<T?>> ToActionOkResultAsync<TResult, T>(this ValueTask<Result<TResult?>> result, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T?> objectResult = new BadRequestResult();
        res
            .OnSuccess(value => objectResult = new OkObjectResult(mapper is null ? res.Value : mapper(res.Value!)))
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult?>> ToActionOkResultAsync<TResult>(this ValueTask<Result<TResult?>> result, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<TResult?> objectResult = new BadRequestResult();
        res
            .OnSuccess(value => objectResult = new OkObjectResult(res.Value))
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<T?>> ToActionCreatedResultAsync<TResult, T>(this ValueTask<Result<TResult?>> result, Uri? location, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T?> objectResult = new BadRequestResult();
        res
#if NET8_0
            .OnSuccess(value => objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!)))
#else
            .OnSuccess(value =>
            {
                if (location is null)
                {
                    objectResult = new ObjectResult(mapper is null ? value : mapper(value!))
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!));
                }
            })
                                                                
#endif
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult?>> ToActionCreatedResultAsync<TResult>(this ValueTask<Result<TResult?>> result, Uri? location, Func<StatusCodeResult>? nullCode = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<TResult?> objectResult = new BadRequestResult();
        res
#if NET8_0
            .OnSuccess(value => objectResult = new CreatedResult(location, value))
#else
            .OnSuccess(value =>
            {
                if (location is null)
                {
                    objectResult = new ObjectResult(value)
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    objectResult = new CreatedResult(location, value);
                }
            })

#endif
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(errors => objectResult = new ObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    /// <summary>
    /// For an asynchronous method return a Task<Result>, we expect a Task and not a ValueTask.
    /// </summary>
    /// <return>OkResult or  BadRequestObjectResult<ProblemDetails></return>
    public static async ValueTask<ActionResult> ToActionOkResultAsync(this Task<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = res.IsSuccess ? new OkResult() : new ObjectResult(res.ToProblemDetails());

        return objectResult;
    }

    public static ValueTask<ActionResult<T?>> ToActionOkResultAsync<TResult, T>(this Result<TResult?> result, Func<TResult, T> mapper, Func<StatusCodeResult>? nullCode = null)
    {
        ActionResult<T?> objectResult = new BadRequestResult();

        result
            .OnSuccess((value) => objectResult = new OkObjectResult(mapper is null ? value : mapper(value)))
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return ValueTask.FromResult(objectResult);
    }

    public static ValueTask<ActionResult<TResult?>> ToActionOkResultAsync<TResult>(this Result<TResult?> result, Func<StatusCodeResult>? nullCode = null)
    {
        ActionResult<TResult?> objectResult = new BadRequestResult();

        result
            .OnSuccess((value) => objectResult = new OkObjectResult(value))
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return ValueTask.FromResult(objectResult);
    }

    public static ValueTask<ActionResult<T?>> ToActionCreatedResultAsync<TResult, T>(this Result<TResult?> result, Uri? location, Func<TResult, T>? mapper = null, Func<StatusCodeResult>? nullCode = null)
    {
        ActionResult<T?> objectResult = new BadRequestResult();

        result
#if NET8_0
            .OnSuccess(value => objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!)))
#else
            .OnSuccess(value =>
            {
                if (location is null)
                {
                    objectResult = new ObjectResult(mapper is null ? value : mapper(value!))
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    objectResult = new CreatedResult(location, mapper is null ? value : mapper(value!));
                }
            })

#endif
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return ValueTask.FromResult(objectResult);
    }

    public static ValueTask<ActionResult<TResult?>> ToActionCreatedResultAsync<TResult>(this Result<TResult?> result, Uri? location, Func<StatusCodeResult>? nullCode = null)
    {
        ActionResult<TResult?> objectResult = new BadRequestResult();

        result
#if NET8_0
            .OnSuccess(value => objectResult = new CreatedResult(location, value))
#else
            .OnSuccess(value =>
            {
                if (location is null)
                {
                    objectResult = new ObjectResult(value)
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    objectResult = new CreatedResult(location, value);
                }
            })

#endif
            .OnSuccessNull(() => objectResult = null == nullCode ? new OkObjectResult(null) : nullCode())
            .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return ValueTask.FromResult(objectResult);
    }

    public static ValueTask<ActionResult> ToActionOkResultAsync(this Result result)
    {
        ActionResult objectResult = new BadRequestResult();

        result.OnSuccess(() => objectResult = new OkResult())
              .OnFailed(_ => objectResult = new ObjectResult(result.ToProblemDetails()));

        return ValueTask.FromResult(objectResult);
    }
#endregion

#region TypedResult
//#if NET7_0_OR_GREATER
//    /// <summary>
//    /// For an asynchronous method returning a Task<Result<T></T>>, we expect a ValueTask.
//    /// </summary>
//    /// <return>OkResult<<TResult> or  BadRequestObjectResult<ProblemDetails></return>
//    public static async ValueTask<Results<Ok<T>, BadRequest>> ToTypedResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Func<TResult, T> mapper)
//    {
//        var res = await result.ConfigureAwait(false);

//        Results<Ok<T>, BadRequest> objectResult = TypedResults.BadRequest();
//        res
//            .OnSuccess(value => objectResult = TypedResults.Ok<T>(mapper is null ? res.Value : mapper(res.Value)))
//            .OnFailed(errors => objectResult = TypedResults.Problem(res.ToProblemDetails()));

//        return objectResult;
//    }

//    public static async ValueTask<ActionResult<TResult>> ToActionOkResultAsync<TResult>(this ValueTask<Result<TResult>> result)
//    {
//        var res = await result.ConfigureAwait(false);

//        ActionResult<TResult> objectResult = new BadRequestResult();
//        res
//            .OnSuccess(value => objectResult = new OkObjectResult(res.Value))
//            .OnFailed(errors => objectResult = new BadRequestObjectResult(res.ToProblemDetails()));

//        return objectResult;
//    }

//    /// <summary>
//    /// For an asynchronous method return a Task<Result>, we expect a Task and not a ValueTask.
//    /// </summary>
//    /// <return>OkResult or  BadRequestObjectResult<ProblemDetails></return>
//    public static async Task<ActionResult> ToActionOkResultAsync(this Task<Result> result)
//    {
//        var res = await result.ConfigureAwait(false);
//        ActionResult objectResult = res.IsSuccess ? new OkResult() : new BadRequestObjectResult(res.ToProblemDetails());

//        return objectResult;
//    }

//    public static Task<ActionResult<T>> ToActionOkResultAsync<TResult, T>(this Result<TResult> result, Func<TResult, T> mapper)
//    {
//        ActionResult<T> objectResult = new BadRequestResult();

//        result
//            .OnSuccess((value) => objectResult = new OkObjectResult(mapper is null ? value : mapper(value)))
//            .OnFailed(_ => objectResult = new BadRequestObjectResult(result.ToProblemDetails()));

//        return Task.FromResult(objectResult);
//    }

//    public static Task<ActionResult<TResult>> ToActionOkResultAsync<TResult>(this Result<TResult> result)
//    {
//        ActionResult<TResult> objectResult = new BadRequestResult();

//        result
//            .OnSuccess((value) => objectResult = new OkObjectResult(value))
//            .OnFailed(_ => objectResult = new BadRequestObjectResult(result.ToProblemDetails()));

//        return Task.FromResult(objectResult);
//    }

//    public static Task<ActionResult> ToActionOkResultAsync(this Result result)
//    {
//        ActionResult objectResult = new BadRequestResult();

//        result.OnSuccess(() => objectResult = new OkResult())
//              .OnFailed(_ => objectResult = new BadRequestObjectResult(result.ToProblemDetails()));

//        return Task.FromResult(objectResult);
//    }
//#endif
#endregion
}
