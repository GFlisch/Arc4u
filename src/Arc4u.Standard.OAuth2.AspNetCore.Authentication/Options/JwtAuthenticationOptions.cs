using Arc4u.OAuth2.Events;
using System;

namespace Arc4u.OAuth2.Options;

public class JwtAuthenticationOptions
{
    public IKeyValueSettings OAuth2Settings { get; set; }

    public string MetadataAddress { get; set; }

    public bool ValidateAuthority { get; set; }

    public Type JwtBearerEventsType { get; set; } = typeof(StandardBearerEvents);
}
