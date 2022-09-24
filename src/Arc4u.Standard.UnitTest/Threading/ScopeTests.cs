using Arc4u.Globalization;
using Arc4u.Threading;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Arc4u.Standard.UnitTest.Threading
{
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
        [Fact]
        public void Fact1()
        {
            Assert.Null(Scope<String>.Current);
        }

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

        [Fact]
        public void TestCulture1()
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            using (new CultureScope(CultureInfo.InvariantCulture))
            {
                Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
            }
            Assert.Equal(culture, Thread.CurrentThread.CurrentCulture);
        }

        [Fact]
        public async void TestCultureContinueOnCurrentThread()
        {
            var frFR = new CultureInfo("fr-FR");
            var deDE = new CultureInfo("de-DE");
            var culture = Thread.CurrentThread.CurrentCulture;

            int threadId = Thread.CurrentThread.ManagedThreadId;

            Assert.NotEqual(culture, frFR);
            Assert.NotEqual(culture, deDE);

            Thread.CurrentThread.CurrentCulture = frFR;
            Thread.CurrentThread.CurrentUICulture = deDE;

            await ChangeCultureAsync(culture, threadId);

            Assert.NotEqual(threadId, Thread.CurrentThread.ManagedThreadId);

            Assert.Equal("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
            Assert.Equal("de-DE", Thread.CurrentThread.CurrentUICulture.Name);
        }

        private async Task ChangeCultureAsync(CultureInfo culture, int threadId)
        {
            Assert.Equal(threadId, Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(100);

            Assert.NotEqual(threadId, Thread.CurrentThread.ManagedThreadId);

            Assert.Equal("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
            Assert.Equal("de-DE", Thread.CurrentThread.CurrentUICulture.Name);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Assert.NotEqual("fr-FR", Thread.CurrentThread.CurrentCulture.Name);
            Assert.NotEqual("de-DE", Thread.CurrentThread.CurrentUICulture.Name);
        }


        [Fact]
        public void TestCulture2()
        {
            var frFR = new CultureInfo("fr-FR");
            var culture = Thread.CurrentThread.CurrentCulture;
            using (new CultureScope(CultureInfo.InvariantCulture))
            {
                Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
                using (new CultureScope(frFR))
                {
                    Assert.Equal(frFR, Thread.CurrentThread.CurrentCulture);
                }
                Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
            }
            Assert.Equal(culture, Thread.CurrentThread.CurrentCulture);
        }

        [Fact]
        public void TestUICulture()
        {
            var frFR = new CultureInfo("fr-FR");
            var culture = Thread.CurrentThread.CurrentUICulture;
            using (new UICultureScope(CultureInfo.InvariantCulture))
            {
                Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentUICulture);
                using (new UICultureScope(frFR))
                {
                    Assert.Equal(frFR, Thread.CurrentThread.CurrentUICulture);
                }
                Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentUICulture);
            }
            Assert.Equal(culture, Thread.CurrentThread.CurrentUICulture);
        }

        [Fact]
        public void TestMixCultures()
        {
            var frFR = new CultureInfo("fr-FR");
            var enGB = new CultureInfo("en-GB");
            var culture = Thread.CurrentThread.CurrentUICulture;
            using (new UICultureScope(CultureInfo.InvariantCulture))
            using (new CultureScope(enGB))
            {
                Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentUICulture);
                Assert.Equal(enGB, Thread.CurrentThread.CurrentCulture);
                using (new UICultureScope(frFR))
                using (new CultureScope(CultureInfo.InvariantCulture))
                {
                    Assert.Equal(frFR, Thread.CurrentThread.CurrentUICulture);
                    Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
                }

                Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentUICulture);
                Assert.Equal(enGB, Thread.CurrentThread.CurrentCulture);
            }
            Assert.Equal(culture, Thread.CurrentThread.CurrentUICulture);
        }

        [Fact]
        public async Task TestAsync1()
        {
            using (new Scope<String>("Hello"))
            {
                var testThread = Thread.CurrentThread.ManagedThreadId;
                var awaitThread = await GetManageThreadIdAsync().ConfigureAwait(false);

                Assert.False(testThread == awaitThread);
                Assert.Equal("Hello", Scope<String>.Current);
            }

            Assert.Null(Scope<String>.Current);
        }

        [Fact]
        public void TestAsync2()
        {
            using (new Scope<String>("Hello"))
            {
                var testThread = Thread.CurrentThread.ManagedThreadId;
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
                threadId = Thread.CurrentThread.ManagedThreadId;

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
                threadId = Thread.CurrentThread.ManagedThreadId;

            }

            Assert.Equal("Hello", Scope<String>.Current);

            return threadId;
        }




    }
}
