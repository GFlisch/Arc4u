using System;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    public interface IDynamicExceptionHandlerBuilder
    {
        /// <summary>
        /// A handler delegate that maps an exception to a status code and something to stream back in the response.
        /// </summary>
        /// <param name="path">The path of the handler that triggered the exception</param>
        /// <param name="error">The actual exception</param>
        /// <param name="uid">A unique identifier identifying the exception. Used as a correlation between the respone and the log</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A tuple with the status code and the value to be written in the response body as Json. If the value is null, nothing is written to the response body</returns>
        delegate Task<(int StatusCode, object Value)> ExceptionHandlerAsyncDelegate<TException>(string path, TException error, Guid uid, CancellationToken cancellationToken) where TException : Exception;

        /// <summary>
        /// A handler delegate that maps an exception to a status code and something to stream back in the response.
        /// </summary>
        /// <param name="path">The path of the handler that triggered the exception</param>
        /// <param name="error">The actual exception</param>
        /// <param name="uid">A unique identifier identifying the exception. Used as a correlation between the respone and the log</param>
        /// <returns>A tuple with the status code and the value to be written in the response body as Json. If the value is null, nothing is written to the response body</returns>
        delegate (int StatusCode, object Value) ExceptionHandlerDelegate<TException>(string path, TException error, Guid uid) where TException : Exception;

        /// <summary>
        /// Handle an exception by executing its handler.
        /// If more than one call is made to this method, the order is important since exceptions whose type derives from <paramref name="exceptionType"/> will also be matched, not only those of exact type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception that will trigger the handler. Derived exception will also trigger the handler, unless their handler has been defined first</typeparam>
        /// <param name="handlerAsync">The handler to execute</param>
        /// <returns></returns>
        IDynamicExceptionHandlerBuilder Handle<TException>(ExceptionHandlerAsyncDelegate<TException> handlerAsync) where TException : Exception;

        /// <summary>
        /// Handle an exception by executing its handler.
        /// If more than one call is made to this method, the order is important since exceptions whose type derives from <paramref name="exceptionType"/> will also be matched, not only those of exact type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception that will trigger the handler. Derived exception will also trigger the handler, unless their handler has been defined first</typeparam>
        /// <param name="handler">The handler to execute</param>
        /// <returns></returns>
        IDynamicExceptionHandlerBuilder Handle<TException>(ExceptionHandlerDelegate<TException> handler) where TException : Exception;
    }
}
