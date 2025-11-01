using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Client.Authentication;

public static partial class LoggerMessages
{
     [LoggerMessage(EventId = 9180, Level = LogLevel.Error,
         Message = "No settings for {resolvingName} is found.")]
     public static partial void LogNoHttpHandlerSettingsFound(this ILogger logger, string resolvingName);

     [LoggerMessage(EventId = 9181, Level = LogLevel.Error,
         Message = "No ApplicationContext is registered in the DI container.")]
     public static partial void LogNoApplicationContextFound(this ILogger logger);

     [LoggerMessage(EventId = 9182, Level = LogLevel.Trace,
         Message = "{handlerName} HttpHandler is called.")]
     public static partial void LogHttpHandlerIsCalled(this ILogger logger, string handlerName);

     [LoggerMessage(EventId = 9183, Level = LogLevel.Debug,
         Message = "An authorization header already exist for handler {handlerName}, Check next Delegate Handler")]
     public static partial void LogHasAlreadyAnAuthorizationHeader(this ILogger logger, string handlerName);

     [LoggerMessage(EventId = 9184, Level = LogLevel.Warning,
         Message = "No token provider is defined in the settings, Check next Delegate Handler")]
     public static partial void LogNoTokenProviderIsDefinedInSettings(this ILogger logger);

     [LoggerMessage(EventId = 9185, Level = LogLevel.Debug,
         Message = "Resolving instance of token provider {providerName}.")]
     public static partial void LogTokenProviderIsResolved(this ILogger logger, string providerName);

     [LoggerMessage(EventId = 9186, Level = LogLevel.Debug,
         Message = "Requesting an authentication token.")]
     public static partial void LogRequestingAToken(this ILogger logger);

     [LoggerMessage(EventId = 9187, Level = LogLevel.Error,
         Message = "Authentication token is not available with token provider {providerName}, Check next Delegate Handler.")]
     public static partial void LogNoAuthenticationTokenCanBeRetrieve(this ILogger logger, string providerName);

     [LoggerMessage(EventId = 9188, Level = LogLevel.Error,
         Message = "Token is expired! Next Hanlder will be called.")]
     public static partial void LogTokenIsExpired(this ILogger logger);

     [LoggerMessage(EventId = 9189, Level = LogLevel.Debug,
         Message = "Add the {scheme} token to provide authentication evidence.")]
     public static partial void LogSchemeInfo(this ILogger logger, string scheme);

     [LoggerMessage(EventId = 9190, Level = LogLevel.Debug,
         Message = "Add the activity id to the request for tracing purpose: {activityId}.")]
     public static partial void LogPrincipalActivityId(this ILogger logger, string activityId);

     [LoggerMessage(EventId = 9191, Level = LogLevel.Debug,
         Message = "Add the current culture to the request: {culture}.")]
     public static partial void LogCultureRequested(this ILogger logger, string culture);
}
