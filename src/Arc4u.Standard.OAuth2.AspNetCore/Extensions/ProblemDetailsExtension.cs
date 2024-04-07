using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Arc4u.Results;
using System;

namespace Arc4u.OAuth2.AspNetCore.Extensions;
public static class ProblemDetailsExtension
{
    /// <summary>
    /// For an asynchronous method returning a Task<Result<T></T>>, we expect a ValueTask.
    /// </summary>
    /// <return>OkResult<<TResult> or  BadRequestObjectResult<ProblemDetails></return>
    public static async ValueTask<ActionResult<T>> ToActionResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Func<TResult, T> mapper)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
            .OnSuccess(value => objectResult = new OkObjectResult(mapper is null ? res.Value : mapper(res.Value)))
            .OnFailed(errors => objectResult = new BadRequestObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    public static async ValueTask<ActionResult<TResult>> ToActionResultAsync<TResult>(this ValueTask<Result<TResult>> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<TResult> objectResult = new BadRequestResult();
        res
            .OnSuccess(value => objectResult = new OkObjectResult(res.Value))
            .OnFailed(errors => objectResult = new BadRequestObjectResult(res.ToProblemDetails()));

        return objectResult;
    }

    /// <summary>
    /// For an asynchronous method return a Task<Result>, we expect a Task and not a ValueTask.
    /// </summary>
    /// <return>OkResult or  BadRequestObjectResult<ProblemDetails></return>
    public static async Task<ActionResult> ToActionResultAsync(this Task<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = res.IsSuccess ? new OkResult() : new BadRequestObjectResult(res.ToProblemDetails());

        return objectResult;
    }

    public static Task<ActionResult<T>> ToActionResultAsync<TResult, T>(this Result<TResult> result, Func<TResult, T> mapper)
    {
        ActionResult<T> objectResult = new BadRequestResult();

        result
            .OnSuccess((value) => objectResult = new OkObjectResult(mapper is null ? value : mapper(value)))
            .OnFailed(_ => objectResult = new BadRequestObjectResult(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static Task<ActionResult<TResult>> ToActionResultAsync<TResult>(this Result<TResult> result)
    {
        ActionResult<TResult> objectResult = new BadRequestResult();

        result
            .OnSuccess((value) => objectResult = new OkObjectResult(value))
            .OnFailed(_ => objectResult = new BadRequestObjectResult(result.ToProblemDetails()));

        return Task.FromResult(objectResult);
    }

    public static Task<ActionResult> ToActionResultAsync(this Result result)
    {
        ActionResult objectResult = result.IsSuccess ? new OkResult() : new BadRequestObjectResult(result.ToProblemDetails());

        return Task.FromResult(objectResult);
    }
}
