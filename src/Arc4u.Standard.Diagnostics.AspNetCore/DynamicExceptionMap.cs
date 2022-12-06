using System;
using System.Collections.Generic;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    /// <summary>
    /// This is a singleton instance holding the mapping between an exception and how to handle it.
    /// It is used when building the configuration at startup, and consulted in the <see cref="DynamicExceptionHandler"/> at exception time.
    /// </summary>
    class DynamicExceptionMap : IDynamicExceptionHandlerBuilder, IDynamicExceptionMap
    {
        private readonly List<(Type ExceptionType, Delegate Handler, Delegate HandlerAsync)> _map;

        public DynamicExceptionMap()
        {
            _map = new List<(Type ExceptionType, Delegate Handler, Delegate HandlerAsync)>();
            // default behavior is to handle AppException and block everything else
            Handle<AppException>(DefaultExceptionHandler.AppExceptionHandlerAsync);
        }

        public bool TryGetValue(Type exceptionType, out (Type ExceptionType, Delegate Handler, Delegate HandlerAsync) value)
        {
            foreach (var item in _map)
                if (item.ExceptionType.IsAssignableFrom(exceptionType))
                {
                    value = item;
                    return true;
                }
            value = default;
            return false;
        }


        public IDynamicExceptionHandlerBuilder Handle<TException>(IDynamicExceptionHandlerBuilder.ExceptionHandlerAsyncDelegate<TException> handlerAsync) where TException : Exception
        {
            if (handlerAsync is null)
                throw new ArgumentNullException(nameof(handlerAsync));
            // make sure we don't add the same type twice, or a more specific type after a more general type, since this will never be caught
            // it's not an error to add a more general type after a more specific type, since the order of processing is linear and any more specific type will be handled first
            foreach (var item in _map)
                if (item.ExceptionType == typeof(TException))
                    throw new ArgumentException($"There is already a handler for {typeof(TException).Name}", nameof(TException));
                else if (item.ExceptionType.IsAssignableFrom(typeof(TException)))
                    throw new ArgumentException($"The handler for {typeof(TException).Name} will never be reached since there is already a handler for the base type {item.ExceptionType.Name}", nameof(TException));
            _map.Add((typeof(TException), null, handlerAsync));
            return this;
        }

        public IDynamicExceptionHandlerBuilder Handle<TException>(IDynamicExceptionHandlerBuilder.ExceptionHandlerDelegate<TException> handler) where TException : Exception
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));
            // make sure we don't add the same type twice, or a more specific type after a more general type, since this will never be caught
            // it's not an error to add a more general type after a more specific type, since the order of processing is linear and any more specific type will be handled first
            foreach (var item in _map)
                if (item.ExceptionType == typeof(TException))
                    throw new ArgumentException($"There is already a handler for {typeof(TException).Name}", nameof(TException));
                else if (item.ExceptionType.IsAssignableFrom(typeof(TException)))
                    throw new ArgumentException($"The handler for {typeof(TException).Name} will never be reached since there is already a handler for the base type {item.ExceptionType.Name}", nameof(TException));
            _map.Add((typeof(TException), handler, null));
            return this;
        }
    }
}
