using System;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    public interface IDynamicExceptionMap
    {
        bool TryGetValue(Type exceptionType, out (Type ExceptionType, Delegate Handler) value);
    }
}
