using System.Collections.Generic;
using Arc4u.OAuth2.AspNetCore.Options;

namespace Arc4u.OAuth2.Options;
public class ValidateResourcesRightMiddlewareOptions
{
    public string DefaultContent { get; set; } = "{\"swagger\": \"2.0\",  \"info\": { \"title\": \"You are not authorized!\", \"version\": \"1.0.0\" }, \"consumes\": [ \"application/json\"  ],  \"produces\": [ \"application/json\" ]}";

    /// <summary>
    /// The key is not used but I use this for the configuration, this is more clear, the key can be used as a description.
    /// </summary>
    public Dictionary<string, ValidateResourceRightMiddlewareOptions> ResourcesPolicies { get; set; }
}
