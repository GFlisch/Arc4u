using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Middleware
{

    public class ValidateSwaggerRightMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ValidateSwaggerRightMiddlewareOption _option;

        public ValidateSwaggerRightMiddleware(RequestDelegate next, ValidateSwaggerRightMiddlewareOption option)
        {
            if (null == option)
                throw new ArgumentNullException(nameof(option));

            _next = next ?? throw new ArgumentNullException(nameof(next));

            _option = option;
        }

        public async Task Invoke(HttpContext context)
        {
            var applicationContext = ((IApplicationContext)context.RequestServices.GetService(typeof(IApplicationContext)));

            if (context.Request.Path.HasValue && string.Equals(context.Request.Path.Value.Trim('/'), _option.Path.Trim('/'), StringComparison.OrdinalIgnoreCase))
            {
                if (applicationContext.Principal.IsAuthorized(_option.Access))
                {
                    await _next.Invoke(context);
                }
                else
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(_option.ContentToDisplay);
                }
            }
            else
                await _next.Invoke(context);

        }


    }
}
