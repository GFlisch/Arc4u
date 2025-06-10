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

    [Fact]
    public void Int_NonNullable_IsSingleton()
    {
        var interval = new Interval<int>(BoundDirection.Closed, 5, 5, BoundDirection.Closed);
        interval.IsSingleton.Should().BeTrue();
        interval.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Int_NonNullable_IsEmpty()
    {
        var interval = new Interval<int>(BoundDirection.Opened, 5, 5, BoundDirection.Opened);
        interval.IsEmpty.Should().BeTrue();
        interval.IsSingleton.Should().BeFalse();
    }

    [Fact]
    public void Int_Nullable_IsSingleton()
    {
        var interval = new Interval<int?>(BoundDirection.Closed, 5, 5, BoundDirection.Closed);
        interval.IsSingleton.Should().BeTrue();
        interval.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Int_Nullable_IsEmpty()
    {
        var interval = new Interval<int?>(BoundDirection.Opened, 5, 5, BoundDirection.Opened);
        interval.IsEmpty.Should().BeTrue();
        interval.IsSingleton.Should().BeFalse();
    }

    [Fact]
    public void Int_Nullable_WithNullValue_IsEmpty()
    {
        var interval = new Interval<int?>(BoundDirection.Opened, null, null, BoundDirection.Opened);
        interval.IsEmpty.Should().BeFalse();
        interval.IsSingleton.Should().BeFalse();
    }

    [Fact]
    public void Int_Nullable_WithNullValue_IsSingleton()
    {
        var exception = Record.Exception(() => new Interval<int?>(BoundDirection.Closed, null, null, BoundDirection.Closed));

        exception.Should().BeOfType<ArgumentException>("An infinity bound must define an opened direction.");
    }

    [Fact]
    public void Double_Nullable_Contains()
    {
        var interval = new Interval<double?>(BoundDirection.Closed, 1.5, 2.5, BoundDirection.Closed);
        interval.Contains(2.0).Should().BeTrue();
        interval.Contains(null).Should().BeFalse();
    }

    [Fact]
    public void DateTime_Nullable_Contains()
    {
        var now = DateTime.UtcNow;
        var interval = new Interval<DateTime?>(BoundDirection.Closed, now, now.AddDays(1), BoundDirection.Closed);
        interval.Contains(now.AddHours(12)).Should().BeTrue();
        interval.Contains(null).Should().BeFalse();
    }

    //[Fact]
    //public void ReferenceType_String_IsEmpty()
    //{
    //    var interval = new Interval<string>(BoundDirection.Opened, null, null, BoundDirection.Opened);
    //    interval.IsEmpty.Should().BeTrue();
    //}

    [Fact]
    public void ReferenceType_String_IsSingleton()
    {
        var interval = new Interval<string>(BoundDirection.Closed, "a", "a", BoundDirection.Closed);
        interval.IsSingleton.Should().BeTrue();
    }

    [Fact]
    public void Equality_And_Comparison()
    {
        var a = new Interval<int>(BoundDirection.Closed, 1, 2, BoundDirection.Closed);
        var b = new Interval<int>(BoundDirection.Closed, 1, 2, BoundDirection.Closed);
        var c = new Interval<int>(BoundDirection.Closed, 2, 3, BoundDirection.Closed);

        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        a.CompareTo(b).Should().Be(0);
        a.CompareTo(c).Should().BeLessThan(0);
        c.CompareTo(a).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToString_And_GetHashCode()
    {
        var interval = new Interval<int>(BoundDirection.Closed, 1, 2, BoundDirection.Closed);
        interval.ToString().Should().NotBeNullOrEmpty();
        interval.GetHashCode().Should().NotBe(0);
    }
}
