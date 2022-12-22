using System;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    interface IDynamicExceptionMap
    {
        bool TryGetValue(Type exceptionType, out (Type ExceptionType, Delegate Handler, Delegate HandlerAsync) value);
    }
}
