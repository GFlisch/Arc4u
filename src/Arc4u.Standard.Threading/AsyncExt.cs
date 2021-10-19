using System.Threading.Tasks;

namespace Arc4u.Threading
{
    public static class AsyncExt
    {
        public static CultureAwaiter<T> ContinueOnCurrentThread<T>(this Task<T> t)
        {
            return new CultureAwaiter<T>(t, true);
        }
        public static CultureAwaiter<T> ContinueOnAnyThread<T>(this Task<T> t)
        {
            return new CultureAwaiter<T>(t, false);
        }

        public static CultureAwaiter ContinueOnCurrentThread(this Task t)
        {
            return new CultureAwaiter(t, true);
        }
        public static CultureAwaiter ContinueOnAnyThread(this Task t)
        {
            return new CultureAwaiter(t, false);
        }
    }
}
