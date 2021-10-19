using Arc4u.Dependency;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public static class ClientSecretAuthenticationMiddlewareExtension
    {

        public static IApplicationBuilder UseClientSecretAuthentication(this IApplicationBuilder app, IContainerResolve container, IKeyValueSettings settings, string settingsSectionName)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            if (null == container)
                throw new ArgumentNullException(nameof(container));

            if (null == settings)
                throw new ArgumentNullException(nameof(settings));

            var option = new ClientSecretAuthenticationOption(settings);
            container.Resolve<IConfiguration>().Bind(settingsSectionName, option);

            return app.UseMiddleware<ClientSecretAuthenticationMiddleware>(container, option);
        }


        public static IApplicationBuilder UseClientSecretAuthentication(this IApplicationBuilder app, IContainerResolve container, ClientSecretAuthenticationOption option)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            if (null == container)
                throw new ArgumentNullException(nameof(container));

            if (null == option)
                throw new ArgumentNullException(nameof(option));


            return app.UseMiddleware<ClientSecretAuthenticationMiddleware>(container, option);
        }


    }
}
