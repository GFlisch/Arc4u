using System.Diagnostics;
using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Xunit;

namespace Arc4u.UnitTest.Diagnostics;

[Trait("Category", "Diagnostics")]
public class TracingHandlerTests
{
    [Fact]
    public async Task When_Sending_A_HttpCall_It_Should_Add_CorrelationId()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "http://localhost/api/test")
            .Respond("application/json", "{'name':'test'}");

        var nullLogger = new NullLogger<TracingHandler>();
        var defaultActivitySourceFactory = new DefaultActivitySourceFactory();

        Activity.Current = defaultActivitySourceFactory.GetArc4u()
            .StartActivity(nameof(When_Sending_A_HttpCall_It_Should_Add_CorrelationId));

        var tracingHandler = new TracingHandler(defaultActivitySourceFactory, nullLogger)
        {
            InnerHandler = mockHttp
        };
        var client = new HttpClient(tracingHandler);

        //Act
        var response = await client.GetAsync("http://localhost/api/test");

        //Assert
        response.Should().NotBeNull();
        response.Headers.Should().ContainKey(CorrelationIdActityExtensions.CorrelationIdKey);
    }
}
