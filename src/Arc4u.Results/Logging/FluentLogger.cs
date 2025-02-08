using Arc4u.Diagnostics;
using Arc4u.Results.Validation;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Arc4u.Results.Logging;

public class FluentLogger : IResultLogger
{
    public FluentLogger(ILogger<FluentLogger> logger)
    {
        _logger = logger;
    }

    delegate void logDelegate(string? message, params object?[] args);

    private readonly ILogger<FluentLogger> _logger;
    public void Log(string context, string content, ResultBase result, LogLevel logLevel)
    {
        if (!string.IsNullOrEmpty(context) && !string.IsNullOrEmpty(content))
        {
            var logger = _logger.Business()
                                .AddIf(context is not null, "Context", () => context!);

            GetBusinessLogger(logger, logLevel)(content);
        }

        if (result is not null && result.IsFailed && result.Errors is not null)
        {
            foreach (var error in result.Errors)
            {
                if (error is ValidationError validationError)
                {
                    var logger = _logger.Business()
                                        .AddIf(validationError.Code is not null, "Code", () => validationError.Code!);

                    GetBusinessLogger(logger, validationError.Severity)(validationError.Message); ;

                    LogReasons(result.Reasons);
                    continue;
                }
                if (error is IExceptionalError exceptionalError)
                {
                    _logger.LogException(exceptionalError.Exception);
                    continue;
                }

                _logger.Business().LogError(error.Message);
            }
        }

        if (result is not null && result.IsSuccess && result.Reasons.Any())
        {
            LogReasons(result.Reasons);
        }
    }

    public void Log<TContext>(string content, ResultBase result, LogLevel logLevel)
    {
        var logger = _logger.Business()
                            .Add("Context", typeof(TContext).FullName!);

        GetBusinessLogger(logger, logLevel)(content);

        if (result is not null && result.IsFailed && result.Errors is not null)
        {
            foreach (var error in result.Errors)
            {
                if (error is ValidationError validationError)
                {
                    logger = _logger.Business()
                                    .AddIf(validationError.Code is not null, "Code", () => validationError.Code!);

                    GetBusinessLogger(logger, validationError.Severity)(validationError.Message);

                    LogReasons(result.Reasons);
                    continue;
                }
                if (error is IExceptionalError exceptionalError)
                {
                    _logger.LogException(exceptionalError.Exception);
                    continue;
                }

                _logger.Business().LogError(error.Message);
            }
        }

        if (result is not null && result.IsSuccess && result.Reasons.Any())
        {
            LogReasons(result.Reasons);
        }
    }

    private void LogReasons(IList<IReason> reasons)
    {
        foreach (var reason in reasons)
        {
            _logger.Business().LogInformation(reason.Message);
        }
    }

    private static logDelegate GetBusinessLogger(LoggerWrapper<FluentLogger> logger, Severity severity) => severity switch
    {
        Severity.Error => logger.LogError,
        Severity.Warning => logger.LogWarning,
        Severity.Info => logger.LogInformation,
        _ => logger.LogDebug,
    };

    private static logDelegate GetBusinessLogger(LoggerWrapper<FluentLogger> logger, LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => logger.LogTrace,
        LogLevel.Debug => logger.LogDebug,
        LogLevel.Information => logger.LogInformation,
        LogLevel.Warning => logger.LogWarning,
        LogLevel.Error => logger.LogError,
        LogLevel.Critical => logger.LogCritical,
        LogLevel.None => logger.LogTrace,
        _ => logger.LogDebug,
    };
}
