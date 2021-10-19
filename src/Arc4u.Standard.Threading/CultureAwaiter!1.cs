using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Arc4u.Threading
{
    public struct CultureAwaiter<T> : INotifyCompletion
    {
        private ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter _awaiter;
        private readonly CultureInfo _uiculture;
        private readonly CultureInfo _culture;
        private readonly Task<T> _task;

        public CultureAwaiter(Task<T> task, bool continueOnCapturedContext)
        {
            _uiculture = CultureInfo.CurrentUICulture;
            _culture = CultureInfo.CurrentCulture;
            var cta = task.ConfigureAwait(continueOnCapturedContext);
            _awaiter = cta.GetAwaiter();
            _task = task;
        }

        public CultureAwaiter<T> GetAwaiter() { return this; }

        public bool IsCompleted
        {
            get
            {
                return _awaiter.IsCompleted;
            }
        }
        public void OnCompleted(Action continuation)
        {
            _awaiter.OnCompleted(continuation);
        }

        public T GetResult()
        {
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiculture;

            _awaiter.GetResult();
            return _task.Result;
        }
    }
}
