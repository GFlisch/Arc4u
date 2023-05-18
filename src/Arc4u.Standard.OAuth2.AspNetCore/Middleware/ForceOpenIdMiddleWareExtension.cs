using System;
using Arc4u.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Standard.OAuth2.Middleware;
public static class ForceOpenIdMiddleWareOptionsExtension
{
    public static IApplicationBuilder UseForceOfOpenId(this IApplicationBuilder app, Action<ForceOpenIdMiddleWareOptions> options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        var _options = new ForceOpenIdMiddleWareOptions();
        options(_options);

        return app.UseMiddleware<ForceOpenIdMiddleWare>(_options);
    }

    public static IApplicationBuilder UseForceOfOpenId(this IApplicationBuilder app, string sectionName = "Authentication:ClaimsMiddleWare:ForceOpenId")
    {
        ArgumentNullException.ThrowIfNull(app);
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }

        var section = app.ApplicationServices.GetRequiredService<IConfiguration>().GetSection(sectionName);

        if (section is null && !section.Exists())
        {
            throw new ConfigurationException($"Section {sectionName} does not exist!");
        }

        return app.UseMiddleware<ForceOpenIdMiddleWare>(section.Get<ForceOpenIdMiddleWareOptions>());
    }
}
