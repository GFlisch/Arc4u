using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.OAuth2.Middleware
{
    public static class ValidateSwaggerRightMiddlewareExtension
    {
        public static IApplicationBuilder AddValidateSwaggerRightFor(this IApplicationBuilder app, ValidateSwaggerRightMiddlewareOption option)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<ValidateSwaggerRightMiddleware>(option);
        }
    }
}
