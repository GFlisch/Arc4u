using System;
using System.Collections.Generic;

namespace Arc4u.OAuth2.Configuration
{
    public class UserConfig
    {
        /// <summary>
        /// The name of the cache used to retrieve the instance of the cache => CacheContext.
        /// </summary>
        public String Identifier { get; set; } = "UniqueId";

        /// <summary>
        /// The claim type used to retrieve the STS identifier for the user.
        /// </summary>
        public List<String> Claims { get; set; } = new List<string>();
    }
}
