using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Arc4u.Threading
{
    public struct CultureAwaiter : INotifyCompletion
    {
        private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter _awaiter;
        private readonly CultureInfo _uiculture;
        private readonly CultureInfo _culture;

        public CultureAwaiter(Task task, bool continueOnCapturedContext)
        {
            _uiculture = CultureInfo.CurrentUICulture;
            _culture = CultureInfo.CurrentCulture;
            var cta = task.ConfigureAwait(continueOnCapturedContext);
            _awaiter = cta.GetAwaiter();
        }

        public CultureAwaiter GetAwaiter() { return this; }

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

        public void GetResult()
        {
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiculture;

            _awaiter.GetResult();
        }
    }
}
