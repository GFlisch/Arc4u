using Arc4u.ServiceModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    public static class DefaultExceptionHandler
    {
        public const string ExceptionUidKey = "ExceptionUid";

        /// <summary>
        /// The default handler for <see cref="AppException"/> to be compatible with the prvevious behavior
        /// </summary>
        /// <param name="path"></param>
        /// <param name="appException"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static Task<(int StatusCode, object Value)> AppExceptionHandlerAsync(string path, AppException appException, Guid uid)
        {
            // it's too bad we need to include Arc4u.Standard just for Messages... Banana -> Gorilla
            var messages = Messages.FromEnum(appException.Messages);
            messages.LocalizeAll();
            return Task.FromResult((StatusCodes.Status400BadRequest, (object)messages));
        }

        /// <summary>
        /// A default generic handler that returns problem details from exception information as outlined in https://tools.ietf.org/html/rfc7807.
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="path"></param>
        /// <param name="exception"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static Task<(int StatusCode, object Value)> GenericExceptionHandlerAsync<TException>(string path, TException exception, Guid uid) where TException : Exception
        {
            var problemDetails = CreateProblemDetails(path, exception, uid);
            return Task.FromResult((problemDetails.Status.Value, (object)problemDetails));
        }

        /// <summary>
        /// A default generic handler that returns problem details from exception information as outlined in https://tools.ietf.org/html/rfc7807.
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="path"></param>
        /// <param name="exception"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static Task<(int StatusCode, object Value)> GenericExceptionHandlerWithDetailsAsync<TException>(string path, TException exception, Guid uid) where TException : Exception
        {
            var problemDetails = CreateProblemDetails(path, exception, uid);
            var serializableValue = ExceptionPropertyValues.GetSerializable(exception);
            if (serializableValue is not null)
                problemDetails.Extensions["ExceptionInstance"] = serializableValue;
            return Task.FromResult((problemDetails.Status.Value, (object)problemDetails));
        }


        /// <summary>
        /// Fill in the problem details with exception information
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="path"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static ProblemDetails CreateProblemDetails<TException>(string path, TException exception, Guid uid) where TException : Exception
        {
            var problemDetails = new ProblemDetails
            {
                Title = exception.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = path,
            };
            problemDetails.Extensions[ExceptionUidKey] = uid;
            return problemDetails;
        }
    }
}
