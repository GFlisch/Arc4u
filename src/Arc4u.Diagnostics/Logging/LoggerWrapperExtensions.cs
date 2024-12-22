using Arc4u.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection;

public static class LoggerWrapperExtensions
{
    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static LoggerWrapper<T> Add<T>(this LoggerWrapper<T> logger, string key, object? value)
    {
        logger.ThrowIfDisposed();
        logger.AdditionalFields[ValidateKey(key)] = value;
        return logger;
    }

    /// <summary>
    /// Adds the specified key when condition is true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="condition">if set to <c>true</c> [condition].</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static LoggerWrapper<T> AddIf<T>(this LoggerWrapper<T> logger, bool condition, string key, object? value)
    {
        logger.ThrowIfDisposed();
        if (condition)
        {
            logger.AdditionalFields[ValidateKey(key)] = value;
        }
        return logger;
    }

    /// <summary>
    /// Adds the specified key if doesn't not exist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static LoggerWrapper<T> AddIfNotExist<T>(this LoggerWrapper<T> logger, string key, object? value)
    {
        logger.ThrowIfDisposed();
        var validKey = ValidateKey(key);

        if (value == null || logger.AdditionalFields.ContainsKey(validKey))
        {
            return logger;
        }

        logger.AdditionalFields[validKey] = value;
        return logger;
    }

    /// <summary>
    /// Adds the specified key or replaces it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static LoggerWrapper<T> AddOrReplace<T>(this LoggerWrapper<T> logger, string key, object? value)
    {
        logger.ThrowIfDisposed();
        logger.AdditionalFields[ValidateKey(key)] = value;
        return logger;
    }

    /// <summary>
    /// Adds the specified key or replaces it when the condition is true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="condition">if set to <c>true</c> [condition].</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static LoggerWrapper<T> AddOrReplaceIf<T>(this LoggerWrapper<T> logger, bool condition, string key, object? value)
    {
        logger.ThrowIfDisposed();

        if (condition)
        {
            logger.AdditionalFields[ValidateKey(key)] = value;
        }
        return logger;
    }

    /// <summary>
    /// Adds the stack trace.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger">The logger.</param>
    /// <returns></returns>
    public static LoggerWrapper<T> AddStackTrace<T>(this LoggerWrapper<T> logger)
    {
        logger.ThrowIfDisposed();
        logger.IncludeStackTrace = true;
        return logger;
    }

    /// <summary>
    /// Validates the key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">key</exception>
    /// <exception cref="ReservedLoggingKeyException"></exception>
    private static string ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        return key switch
        {
            LoggingConstants.ActivityId or
            LoggingConstants.Application or
            LoggingConstants.Category or
            LoggingConstants.Class or
            LoggingConstants.Identity or
            LoggingConstants.MethodName or
            LoggingConstants.ProcessId or
            LoggingConstants.Stacktrace or
            LoggingConstants.UnwrappedException or
            LoggingConstants.ThreadId =>
            throw new ReservedLoggingKeyException(key),
            _ => key,
        };
    }
}
