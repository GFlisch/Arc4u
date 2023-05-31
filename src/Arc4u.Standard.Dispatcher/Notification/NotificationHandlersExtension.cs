using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dispatcher.Notification;
public static class NotificationHandlersExtension
{
    /// <summary>
    /// Register notification handlers to used to publish notifications
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="lifetime"><see cref="ServiceLifetime"/>. Singleton is registered ad Scoped</param>
    public static void AddNotificationHandlersAsScoped(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient(typeof(INotificationHandlers<>), typeof(NotificationHandlers<>));
                services.AddTransient(typeof(INotificationHandlers<,>), typeof(NotificationHandlers<,>));
                services.AddTransient(typeof(INotificationHandlers<,,>), typeof(NotificationHandlers<,,>));
                services.AddTransient(typeof(INotificationHandlers<,,,>), typeof(NotificationHandlers<,,,>));
                services.AddTransient(typeof(INotificationHandlers<,,,,>), typeof(NotificationHandlers<,,,,>));
                break;
            default:
                services.AddScoped(typeof(INotificationHandlers<>), typeof(NotificationHandlers<>));
                services.AddScoped(typeof(INotificationHandlers<,>), typeof(NotificationHandlers<,>));
                services.AddScoped(typeof(INotificationHandlers<,,>), typeof(NotificationHandlers<,,>));
                services.AddScoped(typeof(INotificationHandlers<,,,>), typeof(NotificationHandlers<,,,>));
                services.AddScoped(typeof(INotificationHandlers<,,,,>), typeof(NotificationHandlers<,,,,>));
                break;
        }
    }
}
