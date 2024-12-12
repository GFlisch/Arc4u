using System.Diagnostics;
using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace Arc4u.UnitTest.Diagnostics;

[Trait("Category", "Diagnostics")]
public class TracingHandlerTests
{
    [Fact]
    public async Task When_Sending_A_HttpCall_It_Should_Add_CorrelationId()
    {
        var activityListener = new ActivityListener
        {
            ShouldListenTo = s => true,
            SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
            Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
        };


        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "http://localhost/api/test")
            .Respond("application/json", "{'name':'test'}");

        var nullLogger = new NullLogger<TracingHandler>();
        var defaultActivitySourceFactoryMock = new Mock<IActivitySourceFactory>();
        defaultActivitySourceFactoryMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new ActivitySource("Arc4u", "1.0.0"));


        var activitySource = defaultActivitySourceFactoryMock.Object.Get("Arc4u");
        ActivitySource.AddActivityListener(activityListener);
        Activity.Current = activitySource.StartActivity(nameof(When_Sending_A_HttpCall_It_Should_Add_CorrelationId));

        var tracingHandler = new TracingHandler(defaultActivitySourceFactoryMock.Object, nullLogger)
        {
            InnerHandler = mockHttp
        };
        var client = new HttpClient(tracingHandler);

        //Act
        var response = await client.GetAsync("http://localhost/api/test");

        //Assert
        response.Should().NotBeNull();
        response.Headers.Should().ContainKey(CorrelationIdHttpHeaders.CorrelationId);
    }

    [Fact]
    public async Task When_Sending_A_HttpCall_An_Existing_CorrelationId_Should_Be_Propagated()
    {
        var correlationId = Guid.NewGuid().ToString("D");
        var activityListener = new ActivityListener
        {
            ShouldListenTo = s => true,
            SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
            Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
        };
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "http://localhost/api/test")
            .Respond("application/json", "{'name':'test'}");
        var nullLogger = new NullLogger<TracingHandler>();
        var defaultActivitySourceFactoryMock = new Mock<IActivitySourceFactory>();
        defaultActivitySourceFactoryMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new ActivitySource("Arc4u", "1.0.0"));
        var activitySource = defaultActivitySourceFactoryMock.Object.Get("Arc4u");

        ActivitySource.AddActivityListener(activityListener);
        Activity.Current = activitySource.StartActivity(nameof(When_Sending_A_HttpCall_An_Existing_CorrelationId_Should_Be_Propagated))
            ?.AddTag(CorrelationIdHttpHeaders.CorrelationId, correlationId);

        var tracingHandler = new TracingHandler(defaultActivitySourceFactoryMock.Object, nullLogger)
        {
            InnerHandler = mockHttp
        };
        var client = new HttpClient(tracingHandler);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
        request.Headers.Add(CorrelationIdHttpHeaders.CorrelationId, correlationId);
        //Act
        var response = await client.SendAsync(request);

        //Assert
        response.Should().NotBeNull();
        response.Headers.Should().ContainKey(CorrelationIdHttpHeaders.CorrelationId);
        response.Headers.GetValues(CorrelationIdHttpHeaders.CorrelationId).First().Should().Be(correlationId);
    }
}

