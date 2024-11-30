using Arc4u.Threading;
using Xunit;

namespace Arc4u.UnitTest.Threading;

public class Scope2Tests
{
    [Fact]
    public void TestScopeInitialization()
    {
        var instance = new TestInstance();
        using var scope = new Scope<string, TestInstance>(instance);
        Assert.Equal(instance, Scope<string, TestInstance>.Current);
    }

    [Fact]
    public void TestScopeDisposal()
    {
        var instance = new TestInstance();
        using (var scope = new Scope<string, TestInstance>(instance))
        {
            Assert.Equal(instance, Scope<string, TestInstance>.Current);
        }
        Assert.Null(Scope<string, TestInstance>.Current);
    }

    [Fact]
    public void TestNestedScopes()
    {
        var instance1 = new TestInstance();
        var instance2 = new TestInstance();

        using (var scope1 = new Scope<string, TestInstance>(instance1))
        {
            Assert.Equal(instance1, Scope<string, TestInstance>.Current);

            using (var scope2 = new Scope<string, TestInstance>(instance2))
            {
                Assert.Equal(instance2, Scope<string, TestInstance>.Current);
            }

            Assert.Equal(instance1, Scope<string, TestInstance>.Current);
        }
        Assert.Null(Scope<string, TestInstance>.Current);
    }

    [Fact]
    public void TestScopeToString()
    {
        var instance = new TestInstance();
        using var scope = new Scope<string, TestInstance>(instance);
        Assert.Equal($"Scoping: {instance}", scope.ToString());
    }

    private sealed class TestInstance
    {
        public override string ToString()
        {
            return "TestInstance";
        }
    }
}
