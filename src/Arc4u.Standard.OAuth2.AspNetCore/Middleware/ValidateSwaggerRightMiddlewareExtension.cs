using Microsoft.AspNetCore.Builder;

namespace Arc4u.OAuth2.Middleware;

public static class ValidateSwaggerRightMiddlewareExtension
{
    public static IApplicationBuilder AddValidateSwaggerRightFor(this IApplicationBuilder app, ValidateSwaggerRightMiddlewareOption option)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(option);

        return app.UseMiddleware<ValidateSwaggerRightMiddleware>(option);
    }
}
