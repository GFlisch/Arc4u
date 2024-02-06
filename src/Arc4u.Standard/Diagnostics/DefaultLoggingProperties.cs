using System;
using System.Collections.Generic;
using Arc4u.Dependency.Attribute;
using Arc4u.Security.Principal;

namespace Arc4u.Diagnostics
{
    [Export(typeof(IAddPropertiesToLog)), Scoped]
    public class DefaultLoggingProperties : IAddPropertiesToLog
    {
        public DefaultLoggingProperties(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        private readonly IApplicationContext _applicationContext;

        public IDictionary<string, object> GetProperties()
        {
            if (null != _applicationContext)
            {
                if (null != _applicationContext.Principal)
                {
                    return new Dictionary<string, object>
                        {
                            { LoggingConstants.ActivityId, _applicationContext.ActivityID },
                            { LoggingConstants.Identity, (null != _applicationContext.Principal?.Profile)
                                                                                            ? _applicationContext.Principal.Profile.Name
                                                                                            : null != _applicationContext.Principal?.Identity ? _applicationContext.Principal.Identity.Name : String.Empty }
                        };
                }
            }

            return new Dictionary<string, object>();

        }
    }
}
