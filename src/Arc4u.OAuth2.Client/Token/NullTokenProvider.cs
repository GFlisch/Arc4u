using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Client;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
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

    public Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        _logger.Technical().LogCallNullTokenProvider();
        return Task.FromResult<Result<TokenInfo>>(new());
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        _logger.Technical().LogCallSignOutNullTokenProvider();

        return ValueTask.CompletedTask;
    }
}

