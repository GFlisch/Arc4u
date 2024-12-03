using FluentAssertions;
using Xunit;

namespace Arc4u.Standard.UnitTest.Threading;
public class TimeoutHelperTests
{
    [Trait("Category", "CI")]
    [Fact]
    public void WaitOne_WithTimeout_ShouldReturnFalse()
    {
        using var waitHandle = new ManualResetEvent(false);
        var result = WaitHandleHelper.WaitOne(waitHandle, TimeSpan.FromMilliseconds(100), false);
        Assert.False(result);
    }

    [Trait("Category", "CI")]
    [Fact]
    public void WaitOne_WithSignaledHandle_ShouldReturnTrue()
    {
        using var waitHandle = new ManualResetEvent(true);
        var result = WaitHandleHelper.WaitOne(waitHandle, TimeSpan.FromMilliseconds(100), false);
        Assert.True(result);
    }

    [Trait("Category", "CI")]
    [Fact]
    public void WaitOne_WithNegativeTimeout_ShouldThrowArgumentOutOfRangeException()
    {
        using var waitHandle = new ManualResetEvent(false);
        Assert.Throws<ArgumentOutOfRangeException>(() => WaitHandleHelper.WaitOne(waitHandle, TimeSpan.FromMilliseconds(-1), false));
    }

    [Trait("Category", "CI")]
    [Fact]
    public void TimeoutHelper_Constructor_ShouldInitializeCorrectly()
    {
        var timeoutHelper = new TimeoutHelper(TimeSpan.FromMilliseconds(100));
        Assert.Equal(TimeSpan.FromMilliseconds(100), timeoutHelper.OriginalTimeout);
    }

    [Trait("Category", "CI")]
    [Fact]
    public void TimeoutHelper_RemainingTime_ShouldReturnCorrectValue()
    {
        var timeoutHelper = new TimeoutHelper(TimeSpan.FromMilliseconds(100));
        Thread.Sleep(50);
        Assert.True(timeoutHelper.RemainingTime() <= TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public void Constructor_ShouldSetOriginalTimeout()
    {
        var timeout = TimeSpan.FromSeconds(10);
        var helper = new TimeoutHelper(timeout);

        Assert.Equal(timeout, helper.OriginalTimeout);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_ForNegativeTimeout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimeoutHelper(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void RemainingTime_ShouldReturnCorrectTime()
    {
        var timeout = TimeSpan.FromSeconds(2);
        var helper = new TimeoutHelper(timeout);

        Thread.Sleep(1000);

        var remaining = helper.RemainingTime();

        Assert.True(remaining > TimeSpan.Zero && remaining < timeout);
    }

    [Fact]
    public void RemainingTime_ShouldReturnZero_WhenTimeoutHasElapsed()
    {
        var timeout = TimeSpan.FromSeconds(1);
        var helper = new TimeoutHelper(timeout);

        Thread.Sleep(2000);

        var remaining = helper.RemainingTime();

        Assert.Equal(TimeSpan.Zero, remaining);
    }

    [Fact]
    public void SetTimer_ShouldInvokeCallback_AfterTimeout()
    {
        var timeout = TimeSpan.FromMilliseconds(500);
        var helper = new TimeoutHelper(timeout);
        var callbackInvoked = false;

        void Callback(object state)
        {
            callbackInvoked = true;
        }

        helper.SetTimer(Callback!, null);

        Thread.Sleep(1000);

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void FromMilliseconds_ShouldReturnMaxValue_ForNegativeOne()
    {
        var result = TimeoutHelper.FromMilliseconds(-1);

        Assert.Equal(TimeSpan.MaxValue, result);
    }

    [Fact]
    public void FromMilliseconds_ShouldReturnCorrectTimeSpan()
    {
        var milliseconds = 1000;
        var result = TimeoutHelper.FromMilliseconds(milliseconds);

        Assert.Equal(TimeSpan.FromMilliseconds(milliseconds), result);
    }

    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        var timeout1 = TimeSpan.FromSeconds(1);
        var timeout2 = TimeSpan.FromSeconds(2);

        var result = TimeoutHelper.Add(timeout1, timeout2);

        Assert.Equal(TimeSpan.FromSeconds(3), result);
    }

    [Fact]
    public void Subtract_ShouldReturnCorrectDifference()
    {
        var time = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(1);

        var result = TimeoutHelper.Subtract(time, timeout);

        Assert.Equal(time - timeout, result);
    }

    [Fact]
    public void Divide_ShouldReturnCorrectQuotient()
    {
        var timeout = TimeSpan.FromSeconds(10);
        var factor = 2;

        var result = TimeoutHelper.Divide(timeout, factor);

        Assert.Equal(TimeSpan.FromSeconds(5), result);
    }

    [Fact]
    public void Set_Timer_CallBack()
    {
        var timeout = TimeSpan.FromMilliseconds(50);
        var helper = new TimeoutHelper(timeout);
        var callbackInvoked = false;
        void Callback(object? state)
        {
            callbackInvoked = true;
        }
        helper.SetTimer(Callback, null);
        Thread.Sleep(100);

        callbackInvoked.Should().BeTrue();
    }
}
