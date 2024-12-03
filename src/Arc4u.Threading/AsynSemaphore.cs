namespace Arc4u.Threading;

public class AsyncSemaphore
{
    public AsyncSemaphore(int initialCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCount);

        m_currentCount = initialCount;
    }

    public Task WaitAsync()
    {
        lock (m_waiters)
        {
            if (m_currentCount > 0)
            {
                --m_currentCount;
                return s_completed;
            }
            else
            {
                var waiter = new TaskCompletionSource<bool>();
                m_waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }
    }
    public void Release()
    {
        TaskCompletionSource<bool>? toRelease = null;
        lock (m_waiters)
        {
            if (m_waiters.Count > 0)
            {
                toRelease = m_waiters.Dequeue();
            }
            else
            {
                ++m_currentCount;
            }
        }
        toRelease?.SetResult(true);
    }

    private static readonly Task s_completed = Task.FromResult(true);
    private readonly Queue<TaskCompletionSource<bool>> m_waiters = new Queue<TaskCompletionSource<bool>>();
    private int m_currentCount;
}
