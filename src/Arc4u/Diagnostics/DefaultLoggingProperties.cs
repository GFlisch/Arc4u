using Arc4u.Dependency.Attribute;
using Arc4u.Security.Principal;

namespace Arc4u.Diagnostics;

[Export("Scoped", typeof(IAddPropertiesToLog)), Scoped]
public class DefaultLoggingProperties : IAddPropertiesToLog
{
    private readonly IApplicationContext applicationContext;

    public DefaultLoggingProperties(IApplicationContext applicationContext)
    {
        this.applicationContext = applicationContext;
    }

    public IDictionary<string, object> GetProperties()
    {
        if (null != applicationContext)
        {
            if (null != applicationContext.Principal)
            {
                return new Dictionary<string, object>
                    {
                        { LoggingConstants.ActivityId, applicationContext.ActivityID },
                        { LoggingConstants.Identity, (null != applicationContext.Principal?.Profile)
                                                                                        ? applicationContext.Principal.Profile.Name ?? string.Empty
                                                                                        : null != applicationContext.Principal?.Identity ? applicationContext.Principal.Identity.Name ?? string.Empty: string.Empty }
                    };
            }
        }

        return new Dictionary<string, object>();

    }
}
