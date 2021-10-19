using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;


namespace Arc4u.Standard.AspNetCore.Middleware
{
    public class GrpcAuthenticationStatusCodeMiddleware
    {
        public GrpcAuthenticationStatusCodeMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        private readonly RequestDelegate _next;

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);

            if (null != context.Request.ContentType &&
                null != context.Response &&
                context.Request.ContentType.Contains("grpc") &&
                context.Response.StatusCode == 302)
            {
                context.Response.Headers.Clear();
                context.Response.StatusCode = 401;
            }
        }
    }
}
