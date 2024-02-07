using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Arc4u.Results;
using System;

namespace Arc4u.OAuth2.AspNetCore.Extensions;
public static class MessageDetailExtension
{
    public static async ValueTask<ActionResult<T>> ToActionResultAsync<TResult, T>(this ValueTask<Result<TResult>> result, Func<TResult, T> mapper = null)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult<T> objectResult = new BadRequestResult();
        res
            .OnSuccess(value => objectResult = new OkObjectResult(mapper is null ? res.Value : mapper(res.Value)))
            .OnFailed(errors => objectResult = new BadRequestObjectResult(res.ToMessageDetails()));

        return objectResult;
    }


    public static async Task<ActionResult> ToActionResultAsync(this Task<Result> result)
    {
        var res = await result.ConfigureAwait(false);

        ActionResult objectResult = res.IsSuccess ? new OkObjectResult(res) : new BadRequestObjectResult(res.ToMessageDetails());

        return objectResult;
    }

    public static Task<ActionResult> ToActionResultAsync<TResult>(this Result<TResult> result)
    {
        ActionResult objectResult = result.IsSuccess ? new OkObjectResult(result.Value) : new BadRequestObjectResult(result.ToMessageDetails());

        return Task.FromResult(objectResult);
    }

    public static Task<ActionResult> ToActionResultAsync(this Result result)
    {
        ActionResult objectResult = result.IsSuccess ? new OkResult() : new BadRequestObjectResult(result.ToMessageDetails());

        return Task.FromResult(objectResult);
    }
}
