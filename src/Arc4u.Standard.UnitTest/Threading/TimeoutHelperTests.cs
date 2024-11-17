using Xunit;

namespace Arc4u.Standard.UnitTest.Threading;
public class TimeoutHelperTests
{
    [Trait("Category", "CI")]
    [Fact]
    public void WaitOne_WithTimeout_ShouldReturnFalse()
    {
        using (var waitHandle = new ManualResetEvent(false))
        {
            var result = WaitHandleHelper.WaitOne(waitHandle, TimeSpan.FromMilliseconds(100), false);
            Assert.False(result);
        }
    }

    [Trait("Category", "CI")]
    [Fact]
    public void WaitOne_WithSignaledHandle_ShouldReturnTrue()
    {
        using (var waitHandle = new ManualResetEvent(true))
        {
            var result = WaitHandleHelper.WaitOne(waitHandle, TimeSpan.FromMilliseconds(100), false);
            Assert.True(result);
        }
    }

    [Trait("Category", "CI")]
    [Fact]
    public void WaitOne_WithNegativeTimeout_ShouldThrowArgumentOutOfRangeException()
    {
        using (var waitHandle = new ManualResetEvent(false))
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => WaitHandleHelper.WaitOne(waitHandle, TimeSpan.FromMilliseconds(-1), false));
        }
    }
}
