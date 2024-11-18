using Arc4u.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Arc4u.Standard.UnitTest.Collections.Generic;

public class MemberEqualityComparerTests
{
    private class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Fact]
    public void Equals_SameObject_ReturnsTrue()
    {
        var comparer = MemberEqualityComparer<TestClass>.Default(x => x.Id);
        var obj = new TestClass { Id = 1, Name = "Test" };

        comparer.Equals(obj, obj).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentObjectsWithSameId_ReturnsTrue()
    {
        var comparer = MemberEqualityComparer<TestClass>.Default(x => x.Id);
        var obj1 = new TestClass { Id = 1, Name = "Test1" };
        var obj2 = new TestClass { Id = 1, Name = "Test2" };

        comparer.Equals(obj1, obj2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentObjectsWithDifferentId_ReturnsFalse()
    {
        var comparer = MemberEqualityComparer<TestClass>.Default(x => x.Id);
        var obj1 = new TestClass { Id = 1, Name = "Test1" };
        var obj2 = new TestClass { Id = 2, Name = "Test2" };

        comparer.Equals(obj1, obj2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameObject_ReturnsSameHashCode()
    {
        var comparer = MemberEqualityComparer<TestClass>.Default(x => x.Id);
        var obj = new TestClass { Id = 1, Name = "Test" };

        comparer.GetHashCode(obj).Should().Be(comparer.GetHashCode(obj));
    }

    [Fact]
    public void GetHashCode_DifferentObjectsWithSameId_ReturnsSameHashCode()
    {
        var comparer = MemberEqualityComparer<TestClass>.Default(x => x.Id);
        var obj1 = new TestClass { Id = 1, Name = "Test1" };
        var obj2 = new TestClass { Id = 1, Name = "Test2" };

        comparer.GetHashCode(obj1).Should().Be(comparer.GetHashCode(obj2));
    }

    [Fact]
    public void GetHashCode_DifferentObjectsWithDifferentId_ReturnsDifferentHashCode()
    {
        var comparer = MemberEqualityComparer<TestClass>.Default(x => x.Id);
        var obj1 = new TestClass { Id = 1, Name = "Test1" };
        var obj2 = new TestClass { Id = 2, Name = "Test2" };

        comparer.GetHashCode(obj1).Should().NotBe(comparer.GetHashCode(obj2));
    }
}
