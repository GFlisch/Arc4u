using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.Serialization.Json;
using System.Text;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arc4u.UnitTest;

public class MessageTests
{
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var message = new Message
        {
            Code = "Code1",
            Subject = "Subject1",
            Text = "Text1"
        };

        // Act
        var result = message.ToString();

        // Assert
        Assert.Equal("Code1 Subject1 Text1", result);
    }

    [Fact]
    public void ToString_ShouldReturnBaseString_WhenPropertiesAreNull()
    {
        // Arrange
        var message = new Message();

        // Act
        var result = message.ToString();

        // Assert
        Assert.Equal(message.GetType().ToString(), result);
    }

    [Fact]
    public void LocalizeMessage_ShouldReturnLocalizedMessage()
    {
        // Arrange
        var localizedMessage = new LocalizedMessage
        {
            Type = "Arc4u.ServiceModel.Tests, Arc4u.ServiceModel.Tests",
            Message = "TestMessage",
            Parameters = new object[] { "param1" }
        };

        var serializer = new DataContractJsonSerializer(typeof(LocalizedMessage));
        var sb = new StringBuilder();
        using (var stream = new MemoryStream())
        {
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true))
            {
                serializer.WriteObject(writer, localizedMessage);
            }
            sb.Append(Encoding.UTF8.GetString(stream.ToArray()));
        }

        var message = new Message
        {
            Text = sb.ToString()
        };

        // Act
        var result = message.LocalizeMessage();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Localized TestMessage param1", result.Text);
    }

    [Fact]
    public void Clone_ShouldReturnClonedMessage()
    {
        // Arrange
        var message = new Message
        {
            Code = "Code1",
            Subject = "Subject1",
            Text = "Text1"
        };

        // Act
        var clone = (Message)message.Clone();

        // Assert
        Assert.Equal(message.Code, clone.Code);
        Assert.Equal(message.Subject, clone.Subject);
        Assert.Equal(message.Text, clone.Text);
    }

    [Fact]
    public void LogAndThrowIfNecessary_ShouldLogMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MessageTests>>();
        Expression<Func<string>> messageExpression = () => "Test message";

        // Act
        Message.LogAndThrowIfNecessary(loggerMock.Object, MessageType.Error, messageExpression);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test message")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void GetResourceFromLocalizableMessage_ShouldReturnResourceString()
    {
        // Arrange
        var localizedMessage = new LocalizedMessage
        {
            Type = "Arc4u.ServiceModel.Tests, Arc4u.ServiceModel.Tests",
            Message = "TestMessage"
        };

        // Act
        var methodInfo = typeof(Message).GetMethod("GetResourceFromLocalizableMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = methodInfo?.Invoke(null, new object[] { localizedMessage, CultureInfo.InvariantCulture }) as string;

        // Assert
        Assert.Equal("Localized TestMessage", result);
    }

    [Fact]
    public void ConvertToCode_ShouldReturnSerializedString()
    {
        // Arrange
        Expression<Func<string>> messageExpression = () => "TestMessage";
        var methodInfo = typeof(Message).GetMethod("ConvertToCode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        //var result = methodInfo?.Invoke(null, [messageExpression, Array.Empty<object>()]) as string;
        var result = methodInfo?.Invoke(null, [messageExpression, Array.Empty<object>()]) as string;
        // Assert
        Assert.Contains("\"Message\":\"TestMessage\"", result);
    }

    [Fact]
    public void GetTypeFromstring_ShouldReturnType()
    {
        // Arrange
        var typeString = "Arc4u.ServiceModel.Tests, Arc4u.ServiceModel.Tests";

        // Act
        var methodInfo = typeof(Message).GetMethod("GetTypeFromstring", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = methodInfo?.Invoke(null, [typeString]) as Type;

        // Assert
        Assert.Equal(typeof(MessageTests), result);
    }

}
