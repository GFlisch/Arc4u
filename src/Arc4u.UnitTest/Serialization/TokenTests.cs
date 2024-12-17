using System.IdentityModel.Tokens.Jwt;
using Arc4u.Caching;
using Arc4u.Caching.Memory;
using Arc4u.Configuration.Memory;
using Arc4u.OAuth2.Token;
using Arc4u.Serializer;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Arc4u.UnitTest.Serialization;

[Trait("Category", "CI")]
public class TokenTests
{
    public TokenTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void AccessTokenValidityShould()
    {
        // act
        var sut = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow, expires: DateTime.UtcNow.AddHours(1));

        // assert
        (sut.ValidTo > DateTime.UtcNow.AddMinutes(-5)).Should().BeTrue();
    }

    [Fact]
    public void AccessTokenValidityShouldNot()
    {
        // act
        var sut = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

        // assert
        (sut.ValidTo > DateTime.UtcNow.AddMinutes(-5)).Should().BeFalse();
    }

    [Fact]
    public void AccessTokenBlazorSerializationShouldNot()
    {
        // Arrange
        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

        var tokenInfo = new TokenInfo("Bearer", jwt.EncodedPayload, DateTime.UtcNow);

        var sut = new Arc4u.Caching.SecureCache();
        sut.Initialize("store name");

        sut.Put("key", tokenInfo);

        // act
        var cachedToken = sut.Get<TokenInfo>("key");

        // assert
        cachedToken.Should().NotBeNull();
        cachedToken.Token.Should().Be(jwt.EncodedPayload);

    }

    [Fact]
    public void AccessTokenSerializationJsonShouldNot()
    {
        // Arrange
        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

        var tokenInfo = new TokenInfo("Bearer", jwt.EncodedPayload, DateTime.UtcNow);
        var storeName = "store name";

        IServiceCollection services = new ServiceCollection();

        services.AddTransient<ICache, MemoryCache>();
        services.AddMemoryCache(storeName, options => options.SizeLimitInMB = 10);
        services.AddTransient<IObjectSerialization, JsonSerialization>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var serviceProvider = services.BuildServiceProvider();

        ICache sut = serviceProvider.GetRequiredService<ICache>();
        sut.Initialize(storeName);

        sut.Put("key", tokenInfo);

        // act
        var cachedToken = sut.Get<TokenInfo>("key");

        // assert
        cachedToken.Should().NotBeNull();
        cachedToken!.Token.Should().Be(jwt.EncodedPayload);
        cachedToken.TokenType.Should().Be(tokenInfo.TokenType);
        cachedToken.ExpiresOnUtc.Should().Be(tokenInfo.ExpiresOnUtc);
    }

    [Fact]
    public void AccessTokenSerializationProtobufShouldNot()
    {
        // Arrange
        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

        var tokenInfo = new TokenInfo("Bearer", jwt.EncodedPayload, DateTime.UtcNow);

        var storeName = "store name";
        IServiceCollection services = new ServiceCollection();

        services.AddTransient<ICache, MemoryCache>();
        services.AddMemoryCache(storeName, options => options.SizeLimitInMB = 10);
        services.AddTransient<IObjectSerialization, JsonSerialization>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var serviceProvider = services.BuildServiceProvider();

        ICache sut = serviceProvider.GetRequiredService<ICache>();
        sut.Initialize(storeName);

        sut.Put("key", tokenInfo);

        // act
        var cachedToken = sut.Get<TokenInfo>("key");

        // assert
        cachedToken.Should().NotBeNull();
        cachedToken!.Token.Should().Be(jwt.EncodedPayload);
        cachedToken.TokenType.Should().Be(tokenInfo.TokenType);
        cachedToken.ExpiresOnUtc.Should().Be(tokenInfo.ExpiresOnUtc);
    }
}
