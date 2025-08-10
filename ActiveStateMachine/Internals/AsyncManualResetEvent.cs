namespace ActiveStateMachine.Internals;

/// <summary>
/// An async-compatible ManualResetEvent that avoids blocking threads.
/// </summary>
internal sealed class AsyncManualResetEvent
{
    private volatile TaskCompletionSource _tcs = CreateCompletedTcs();

    public AsyncManualResetEvent(bool isSet = true)
    {
        if (!isSet) _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        var task = _tcs.Task;
        return cancellationToken.CanBeCanceled
            ? task.WaitAsync(cancellationToken)
            : task;
    }

    public void Set()
    {
        var tcs = _tcs;
        if (!tcs.Task.IsCompleted)
            tcs.TrySetResult();
    }

    public void Reset()
    {
        while (true)
        {
            var tcs = _tcs;
            if (!tcs.Task.IsCompleted) return;
            var newTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            if (Interlocked.CompareExchange(ref _tcs, newTcs, tcs) == tcs) return;
        }
    }

    private static TaskCompletionSource CreateCompletedTcs()
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        tcs.TrySetResult();
        return tcs;
    }
}