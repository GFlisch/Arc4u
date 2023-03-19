using System.Collections.Generic;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentAssertions;
using System.Linq;
using Arc4u.Configuration;

namespace Arc4u.Standard.UnitTest;

[Trait("Category", "CI")]
public class SettingsOptionTests
{
    public SettingsOptionTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void SimpleKeyValueShould()
    {
        var config = new ConfigurationBuilder()
              .AddInMemoryCollection(
                  new Dictionary<string, string?>
                  {
                      ["OAuth2.Settings:ProviderId"] = "Oidc",
                      ["OAuth2.Settings:AuthenticationType"] = "OAuth2Bearer",
                      ["OAuth2.Settings:Object"] = "True",
                  }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        var dic = configuration.GetSection("OAuth2.Settings").Get<Dictionary<string, string>>();

        void options(SimpleKeyValueSettings settings)
        {
            foreach (var kv in dic)
            {
                settings.Add(kv.Key, kv.Value);
            }
        }

        IServiceCollection services = new ServiceCollection();

        services.Configure<SimpleKeyValueSettings>("OAuth2", options);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptionsMonitor<SimpleKeyValueSettings>>()!.Get("OAuth2");

        sut.Values.Count.Should().Be(3);
        sut.Values["ProviderId"].Should().Be("Oidc");
        sut.Values["AuthenticationType"].Should().Be("OAuth2Bearer");
        sut.Values["Object"].Should().Be("True");

    }
}
