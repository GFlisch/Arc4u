using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    public static class DynamicExceptionHandlerExtensions
    {
        /// <summary>
        /// Add the dynamic exception handler
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDynamicExceptionHandler(this IServiceCollection services)
        {
            var exceptionMap = new DynamicExceptionMap();
            services.AddSingleton<IDynamicExceptionHandlerBuilder>(exceptionMap);
            services.AddSingleton<IDynamicExceptionMap>(exceptionMap);
            return services.AddSingleton<DynamicExceptionMap>();
        }

        /// <summary>
        /// Add and configure the dynamic exception handler
        /// </summary>
        /// <param name="app"></param>
        /// <param name="config">a configurator for the exception handler to specify what to do with exceptions</param>
        /// <returns></returns>
        public static IApplicationBuilder UseDynamicExceptionHandler(this IApplicationBuilder app, Action<IDynamicExceptionHandlerBuilder> config)
        {
            var builder = app.ApplicationServices.GetService<IDynamicExceptionHandlerBuilder>();
            if (builder == null)
                throw new ArgumentException($"You forgot to call {nameof(AddDynamicExceptionHandler)}", nameof(app));
            config(builder);
            app.UseMiddleware<DynamicExceptionHandler>();
            return app;
        }


        /// <summary>
        /// Add and configure the dynamic exception handler
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDynamicExceptionHandler(this IApplicationBuilder app) => app.UseDynamicExceptionHandler(builder => { });
    }
}
