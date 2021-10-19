using Microsoft.AspNetCore.Mvc;
using System;

namespace Arc4u.OAuth2.Aspect
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]

    public class ServiceAspectAttribute : TypeFilterAttribute
    {
        public ServiceAspectAttribute() : this(string.Empty, new int[0])
        {
        }

        public ServiceAspectAttribute(params int[] operations) : this(string.Empty, operations)
        {
        }

        public ServiceAspectAttribute(string scope, params int[] operations) : base(typeof(OperationCheckAttribute))
        {
            IsReusable = true;
            Arguments = new object[] { scope ?? string.Empty, operations };
        }

    }
}
