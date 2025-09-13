using Arc4u.OAuth2;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class ClaimsFillerOptionsTests
{
    private readonly Fixture _fixture;

    public ClaimsFillerOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void Basic_ClaimsFillerOptions_Should()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>()).Build();

        IConfiguration configuration = new ConfigurationRoot([.. config.Providers]);

        IServiceCollection services = new ServiceCollection();

        services.AddClaimsFiller(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<ClaimsFillerOptions>>().CurrentValue;

        Assert.NotNull(settings);
        settings.LoadClaimsFromClaimsFillerProvider.Should().BeTrue();
        settings.SettingsKeys.Should().OnlyContain(settings => settings == Constants.OpenIdOptionsName);
    }

    [Fact]
    public void Settings_ClaimsFillerOptions_Should()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClaimsMiddleWare:ClaimsFiller:SettingsKeys:0"] = "OAuth2"
                }).Build();

        IConfiguration configuration = new ConfigurationRoot([.. config.Providers]);

        IServiceCollection services = new ServiceCollection();

        services.AddClaimsFiller(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<ClaimsFillerOptions>>().CurrentValue;

        Assert.NotNull(settings);
        settings.LoadClaimsFromClaimsFillerProvider.Should().BeTrue();
        settings.SettingsKeys.Should().HaveCount(1);
        settings.SettingsKeys.Should().OnlyContain(s => s == "OAuth2");
        settings.ClaimsToExclude.Should().NotBeEmpty();
    }

    [Fact]
    public void Do_No_Load_Claims_ClaimsFillerOptions_Should()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClaimsMiddleWare:ClaimsFiller:LoadClaimsFromClaimsFillerProvider"] = "false"
                }).Build();

        IConfiguration configuration = new ConfigurationRoot([.. config.Providers]);

        IServiceCollection services = new ServiceCollection();

        services.AddClaimsFiller(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<ClaimsFillerOptions>>().CurrentValue;

        Assert.NotNull(settings);
        settings.LoadClaimsFromClaimsFillerProvider.Should().BeFalse();
        settings.SettingsKeys.Should().HaveCount(1);
        settings.SettingsKeys.Should().OnlyContain(settings => settings == "OpenId");
    }
}
