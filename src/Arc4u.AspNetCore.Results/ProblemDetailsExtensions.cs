using System;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Arc4u.AspNetCore.Results;
public static class ProblemDetailsExtensions
{
    public static ProblemDetails WithStatusCode(this ProblemDetails problemDetails, int statusCode)
    {
        problemDetails.Status = statusCode;
        return problemDetails;
    }

    public static ProblemDetails WithTitle(this ProblemDetails problemDetails, string title)
    {
        problemDetails.Title = title;
        return problemDetails;
    }

    public static ProblemDetails WithType(this ProblemDetails problemDetails, Uri type)
    {
        problemDetails.Type = type.ToString();
        return problemDetails;
    }

    public static ProblemDetails WithDetail(this ProblemDetails problemDetails, string detail)
    {
        problemDetails.Detail = detail;
        return problemDetails;
    }

    public static ProblemDetails WithCode(this ProblemDetails problemDetails, string code)
    {
        problemDetails.Extensions.AddOrReplace("Code", code);
        return problemDetails;
    }

    public static ProblemDetails WithSeverity(this ProblemDetails problemDetails, string severity)
    {
        problemDetails.Extensions.AddOrReplace("Severity", severity);
        return problemDetails;
    }

    public static ProblemDetails WithMetadata(this ProblemDetails problemDetails, string key, object value)
    {
        problemDetails.Extensions.AddOrReplace(key, value);
        return problemDetails;
    }
}

