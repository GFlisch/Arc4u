using System.Diagnostics;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Arc4u.Results;

namespace Arc4u.OAuth2.AspNetCore.Extensions;
public static class MessageDetailExtension
{
    public static Task<IActionResult> ToActionResultAsync<TResult>(this Result<TResult> result)
    {
        return ToActionResultAsync(result, Activity.Current?.Id);
    }

    public static Task<IActionResult> ToActionResultAsync<TResult>(this Result<TResult> result, string? activityId)
    {
        IActionResult objectResult = result.IsSuccess ? new OkObjectResult(result.Value) : new BadRequestObjectResult(result.ToMessageDetails());

        return Task.FromResult(objectResult);
    }

    public static Task<IActionResult> ToActionResultAsync(this Result result)
    {
        return result.ToActionResultAsync(Activity.Current?.Id);
    }

    public static Task<IActionResult> ToActionResultAsync(this Result result, string? activityId)
    {
        IActionResult objectResult = result.IsSuccess ? new OkResult() : new BadRequestObjectResult(result.ToMessageDetails());

        return Task.FromResult(objectResult);
    }
}
