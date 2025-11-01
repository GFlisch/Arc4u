using Arc4u.Configuration;

namespace Arc4u.OAuth2.Options;

public class ApiExtraContextAuthenticationOption
{
    public IKeyValueSettings AuthorizationParameters { get; set; } = new SimpleKeyValueSettings();

    public IKeyValueSettings TokenParameters { get; set; } = new SimpleKeyValueSettings();
}
