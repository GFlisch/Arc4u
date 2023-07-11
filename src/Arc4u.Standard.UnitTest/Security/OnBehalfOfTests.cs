using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Extensions;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class OnBehalfOfTests
{
    public OnBehalfOfTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void OnBehalfOfAuthenticationOptionsShould()
    {
        var settings = _fixture.Create<OnBehalfOfSettingsOptions>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
       new Dictionary<string, string?>
       {
           ["Authentication:OnBehalfOf:Obo1:ClientId"] = settings.ClientId,
           ["Authentication:OnBehalfOf:Obo1:ClientSecret"] = settings.ClientSecret,
           ["Authentication:OnBehalfOf:Obo1:Scope"] = settings.Scope,
       }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddOnBehalfOf(configuration);

        var app = services.BuildServiceProvider();

        var oboSettings = app.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>().Get("Obo1");

        oboSettings.Should().NotBeNull();
        oboSettings.Values[TokenKeys.ClientIdKey].Should().Be(settings.ClientId);
        oboSettings.Values[TokenKeys.ClientSecret].Should().Be(settings.ClientSecret);
        oboSettings.Values[TokenKeys.Scope].Should().Be(settings.Scope);

    }
}
