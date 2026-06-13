using Arc4u.Configuration;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest;

[Trait("Category", "CI")]
public class ClientTokensOptionsTests
{
    private readonly Fixture _fixture;

    public ClientTokensOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void UserPassword_Scenario_Should()
    {
        var clientId = _fixture.Create<string>();
        var user = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var authUrl = _fixture.Create<Uri>().ToString();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Client1:Scenario"] = UserPasswordScenario.Name,
                    ["Authentication:ClientTokens:Client1:Authority:Url"] = authUrl,
                    ["Authentication:ClientTokens:Client1:Settings:ClientId"] = clientId,
                    ["Authentication:ClientTokens:Client1:Settings:User"] = user,
                    ["Authentication:ClientTokens:Client1:Settings:Password"] = password
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();
        services.AddClientTokens(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Client1");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(CredentialSecretTokenProvider.ProviderName);
        sut.Values[TokenKeys.ClientIdKey].Should().Be(clientId);
        sut.Values[TokenKeys.AuthorityKey].Should().Be("Client1");
        sut.Values[TokenKeys.Scope].Should().Be("openid");
        sut.Values["User"].Should().Be(user);
        sut.Values["Password"].Should().Be(password);
        // BasicProviderId defaults when not provided.
        sut.Values["BasicProviderId"].Should().Be(CredentialTokenCacheTokenProvider.ProviderName);
        sut.Values.ContainsKey("Credential").Should().BeFalse();
        sut.Values.ContainsKey(TokenKeys.ExtraParameters).Should().BeFalse();

        var sutAuthority = serviceProvider.GetService<IOptionsMonitor<AuthorityOptions>>()!.Get("Client1");
        sutAuthority.Should().NotBeNull();
        sutAuthority.Url.Should().Be(authUrl);
    }

    [Fact]
    public void ClientCredentials_Scenario_With_ExtraParameters_Should()
    {
        var clientId = _fixture.Create<string>();
        var clientSecret = _fixture.Create<string>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Logto:Scenario"] = ClientCredentialsScenario.Name,
                    ["Authentication:ClientTokens:Logto:Settings:ClientId"] = clientId,
                    ["Authentication:ClientTokens:Logto:Settings:ClientSecret"] = clientSecret,
                    ["Authentication:ClientTokens:Logto:Settings:resource"] = "https://api.example.com/path",
                    ["Authentication:ClientTokens:Logto:Settings:audience"] = "logto"
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();
        services.AddClientTokens(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Logto");

        sut.Should().NotBeNull();
        sut.Values[TokenKeys.ProviderIdKey].Should().Be(ClientCredentialsScenario.ProviderName);
        sut.Values[TokenKeys.ClientIdKey].Should().Be(clientId);
        sut.Values[TokenKeys.ClientSecret].Should().Be(clientSecret);
        sut.Values.ContainsKey(TokenKeys.AuthorityKey).Should().BeFalse();

        // Unclaimed Settings keys flow through as extra parameters.
        var extra = ExtraParametersEncoder.Decode(sut.Values[TokenKeys.ExtraParameters]).ToDictionary(p => p.Key, p => p.Value);
        extra.Should().HaveCount(2);
        extra["resource"].Should().Be("https://api.example.com/path");
        extra["audience"].Should().Be("logto");
    }

    [Fact]
    public void Missing_Scenario_Discriminator_Should_Throw()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Client1:Settings:ClientId"] = _fixture.Create<string>(),
                    ["Authentication:ClientTokens:Client1:Settings:User"] = _fixture.Create<string>(),
                    ["Authentication:ClientTokens:Client1:Settings:Password"] = _fixture.Create<string>()
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddClientTokens(configuration));

        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void Unknown_Scenario_Discriminator_Should_Throw()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Client1:Scenario"] = "DoesNotExist",
                    ["Authentication:ClientTokens:Client1:Settings:ClientId"] = _fixture.Create<string>()
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddClientTokens(configuration));

        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void ClientCredentials_Missing_ClientSecret_Should_Throw()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Logto:Scenario"] = ClientCredentialsScenario.Name,
                    ["Authentication:ClientTokens:Logto:Settings:ClientId"] = _fixture.Create<string>()
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddClientTokens(configuration));

        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void UserPassword_Password_And_Credential_Should_Throw()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Client1:Scenario"] = UserPasswordScenario.Name,
                    ["Authentication:ClientTokens:Client1:Settings:ClientId"] = _fixture.Create<string>(),
                    ["Authentication:ClientTokens:Client1:Settings:User"] = _fixture.Create<string>(),
                    ["Authentication:ClientTokens:Client1:Settings:Password"] = _fixture.Create<string>(),
                    ["Authentication:ClientTokens:Client1:Settings:Credential"] = _fixture.Create<string>()
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddClientTokens(configuration));

        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void UserPassword_Password_Without_User_Should_Throw()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Client1:Scenario"] = UserPasswordScenario.Name,
                    ["Authentication:ClientTokens:Client1:Settings:ClientId"] = _fixture.Create<string>(),
                    ["Authentication:ClientTokens:Client1:Settings:Password"] = _fixture.Create<string>()
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddClientTokens(configuration));

        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void Custom_Scenario_Can_Be_Registered_Should()
    {
        var clientId = _fixture.Create<string>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClientTokens:Client1:Scenario"] = "Custom",
                    ["Authentication:ClientTokens:Client1:Settings:ClientId"] = clientId
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        try
        {
            ClientTokensExtension.SetScenarioResolver(discriminator => discriminator switch
            {
                "Custom" => new CustomScenario(),
                _ => ClientTokensExtension.DefaultScenario(discriminator)
            });

            services.AddClientTokens(configuration);

            var serviceProvider = services.BuildServiceProvider();
            var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("Client1");

            sut.Values[TokenKeys.ProviderIdKey].Should().Be("Custom");
            sut.Values[TokenKeys.ClientIdKey].Should().Be(clientId);
        }
        finally
        {
            // Restore the default resolver so the global static does not leak into other tests.
            ClientTokensExtension.SetScenarioResolver(ClientTokensExtension.DefaultScenario);
        }
    }

    [Fact]
    public void No_ClientTokens_Section_Should()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection([]).Build();
        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));
        IServiceCollection services = new ServiceCollection();

        services.AddClientTokens(configuration);

        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>().Should().BeNull();
    }

    private sealed class CustomScenario : IClientTokenScenario
    {
        public IReadOnlyCollection<string> KnownKeys { get; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ClientId" };

        public void Validate(string optionKey, ClientTokenSettingsOptions options)
        {
            if (!options.Settings.TryGetValue("ClientId", out var clientId) || string.IsNullOrWhiteSpace(clientId))
            {
                throw new ConfigurationException($"[{optionKey}] ClientId must be filled.");
            }
        }

        public void WriteTo(string optionKey, ClientTokenSettingsOptions options, SimpleKeyValueSettings settings)
        {
            settings.Add(TokenKeys.ProviderIdKey, "Custom");
            settings.Add(TokenKeys.ClientIdKey, options.Settings["ClientId"]);
        }
    }
}
