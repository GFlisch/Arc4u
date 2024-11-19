using System.Globalization;
using Arc4u.Threading;
using Xunit;

namespace Arc4u.UnitTest.Threading;

public class Context
{
    private static readonly Context InnerContext;

    static Context()
    {
        InnerContext = new Context();
    }

    public static Context Current
    {
        get
        {
            return Scope<Context>.Current ?? InnerContext;
        }
    }

    public String Value { get; set; }
}

public class ScopeTest
{
    [Trait("Category", "CI")]
    [Fact]
    public void Fact1()
    {
        Assert.Null(Scope<String>.Current);
    }

    [Trait("Category", "CI")]
    [Fact]
    public void Fact2()
    {
        using (new Scope<String>("Hello"))
        {
            using (new Scope<String>("Gilles"))
            {
                Assert.Equal("Gilles", Scope<String>.Current);
            }
            Assert.Equal("Hello", Scope<String>.Current);
        }

        Assert.Null(Scope<String>.Current);

    }

    [Trait("Category", "CI")]
    [Fact]
    public void Fact3()
    {
        Assert.NotNull(Context.Current);
        Context.Current.Value = "Global";

        using (new Scope<Context>(new Context()))
        {
            Context.Current.Value = "Local";
            Assert.Equal("Local", Scope<Context>.Current.Value);
        }

        Assert.Equal("Global", Context.Current.Value);
    }

    [Trait("Category", "All")]
    [Fact]
    public async void TestCultureContinueOnCurrentThread()
    {
        var frFR = new CultureInfo("fr-FR");
        var deDE = new CultureInfo("de-DE");
        var culture = Thread.CurrentThread.CurrentCulture;

        int threadId = Environment.CurrentManagedThreadId;

        Assert.NotEqual(culture, frFR);
        Assert.NotEqual(culture, deDE);

        Thread.CurrentThread.CurrentCulture = frFR;
        Thread.CurrentThread.CurrentUICulture = deDE;

        await ScopeTest.ChangeCultureAsync(culture, threadId).ConfigureAwait(true);

        Assert.NotEqual(threadId, Environment.CurrentManagedThreadId);

        Assert.Equal("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
        Assert.Equal("de-DE", Thread.CurrentThread.CurrentUICulture.Name);
    }

    private static async Task ChangeCultureAsync(CultureInfo culture, int threadId)
    {
        Assert.Equal(threadId, Environment.CurrentManagedThreadId);
        await Task.Delay(100).ConfigureAwait(true);

        Assert.NotEqual(threadId, Environment.CurrentManagedThreadId);

        Assert.Equal("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
        Assert.Equal("de-DE", Thread.CurrentThread.CurrentUICulture.Name);

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        Assert.NotEqual("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
        Assert.NotEqual("de-DE", Thread.CurrentThread.CurrentUICulture.Name);
    }

    [Fact]
    public async Task TestAsync1()
    {
        using (new Scope<string>("Hello"))
        {
            await GetManageThreadIdAsync();

            Assert.Equal("Hello", Scope<string>.Current);
        }

        Assert.Null(Scope<string>.Current);
    }

    [Fact]
    public void TestAsync2()
    {
        using (new Scope<String>("Hello"))
        {
            var testThread = Environment.CurrentManagedThreadId;
            var t1 = GetManageThreadIdAsync();
            var t2 = GetManageThreadIdAsync2();
            Task.WaitAll(t1, t2);

            Assert.False(testThread == t1.Result);
            Assert.False(testThread == t2.Result);
            Assert.Equal("Hello", Scope<String>.Current);
        }

        Assert.Null(Scope<String>.Current);
    }

    private async Task<int> GetManageThreadIdAsync()
    {
        int threadId;

        using (new Scope<String>("Gilles"))
        {
            await Task.Delay(100).ConfigureAwait(false);
            Assert.Equal("Gilles", Scope<String>.Current);
            threadId = Environment.CurrentManagedThreadId;

        }

        Assert.Equal("Hello", Scope<String>.Current);

        return threadId;
    }

    private async Task<int> GetManageThreadIdAsync2()
    {
        int threadId;

        using (new Scope<String>("Gaëtan"))
        {
            await Task.Delay(100).ConfigureAwait(false);
            Assert.Equal("Gaëtan", Scope<String>.Current);
            threadId = Environment.CurrentManagedThreadId;

        }

        Assert.Equal("Hello", Scope<String>.Current);

        return threadId;
    }

}
