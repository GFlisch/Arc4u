using Arc4u.Diagnostics.Logging;
using Microsoft.AspNetCore.Builder;

namespace Arc4u.Diagnostics;
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseArc4uLogging(this IApplicationBuilder app)
    {
        LoggerWrapperContext.Initialize(app.ApplicationServices);
        return app;
    }
}
