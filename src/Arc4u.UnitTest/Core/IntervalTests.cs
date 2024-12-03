using FluentAssertions;
using Xunit;

namespace Arc4u.UnitTest.Core;

public class IntervalTests
{
    [Fact]
    public void Test_IsSingleton()
    {
        var interval = new Interval<int>(BoundDirection.Closed, 5, 5, BoundDirection.Closed);
        interval.IsSingleton.Should().BeTrue();
    }

    [Fact]
    public void Test_IsNotSingleton()
    {
        var interval = new Interval<int>(BoundDirection.Closed, 5, 6, BoundDirection.Closed);
        interval.IsSingleton.Should().BeFalse();
    }

    [Fact]
    public void Test_IsEmpty()
    {
        var interval = new Interval<int>(BoundDirection.Opened, 5, 5, BoundDirection.Opened);
        interval.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Test_IsNotEmpty()
    {
        var interval = new Interval<int>(BoundDirection.Closed, 5, 6, BoundDirection.Closed);
        interval.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Test_IsUniverse()
    {
        var interval = Interval.Universe<int>();
        interval.IsUniverse.Should().BeTrue();
    }

    [Fact]
    public void Test_Contains()
    {
        var interval = new Interval<int>(BoundDirection.Closed, 5, 10, BoundDirection.Closed);
        interval.Contains(7).Should().BeTrue();
    }

    [Fact]
    public void Test_DoesNotContain()
    {
        var interval = new Interval<int>(BoundDirection.Closed, 5, 10, BoundDirection.Closed);
        interval.Contains(11).Should().BeFalse();
    }

    [Fact]
    public void Test_IntersectsWith()
    {
        var interval1 = new Interval<int>(BoundDirection.Closed, 5, 10, BoundDirection.Closed);
        var interval2 = new Interval<int>(BoundDirection.Closed, 8, 12, BoundDirection.Closed);
        interval1.IntersectsWith(interval2).Should().BeTrue();
    }

    [Fact]
    public void Test_DoesNotIntersectWith()
    {
        var interval1 = new Interval<int>(BoundDirection.Closed, 5, 10, BoundDirection.Closed);
        var interval2 = new Interval<int>(BoundDirection.Closed, 11, 15, BoundDirection.Closed);
        interval1.IntersectsWith(interval2).Should().BeFalse();
    }

    [Fact]
    public void Test_UnionWith()
    {
        var interval1 = new Interval<int>(BoundDirection.Closed, 5, 10, BoundDirection.Closed);
        var interval2 = new Interval<int>(BoundDirection.Closed, 8, 12, BoundDirection.Closed);
        var union = interval1.UnionWith(interval2);
        union.Should().Contain(i => i.Contains(5) && i.Contains(12));
    }

    [Fact]
    public void Test_DifferenceWith()
    {
        var interval1 = new Interval<int>(BoundDirection.Closed, 5, 10, BoundDirection.Closed);
        var interval2 = new Interval<int>(BoundDirection.Closed, 8, 12, BoundDirection.Closed);
        var difference = interval1.DifferenceWith(interval2);
        difference.Should().Contain(i => i.Contains(5) && !i.Contains(8));
    }
}
