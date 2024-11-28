using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Token;

[Export(NullTokenProvider.ProviderName, typeof(ITokenProvider)), Shared]
public class NullTokenProvider : ITokenProvider
{
    public NullTokenProvider(ILogger<NullTokenProvider> logger)
    {
        _logger = logger;
    }

    public const string ProviderName = "null";

    private readonly ILogger<NullTokenProvider> _logger;

    public Task<TokenInfo?> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        _logger.Technical().System("Null token provide is invoked.").Log();
        return Task.FromResult<TokenInfo?>(null);
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        _logger.Technical().System("Null token provider doesn't do anything.").Log();
#if NET8_0_OR_GREATER
        return ValueTask.CompletedTask;
#else
        return default;
#endif
    }
}

