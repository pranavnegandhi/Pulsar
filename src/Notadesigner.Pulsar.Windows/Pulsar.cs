namespace Notadesigner.Pulsar.Windows;

public class Pulsar : IAsyncDisposable
{
    public event EventHandler<EventArgs>? Pulse;

    public bool IsRunning => _cycleCancellation is not null;

    private CancellationTokenSource? _cycleCancellation;

    private bool _disposed = false;

    private Task? _cycleTask;

    public void Start()
    {
        if (_cycleCancellation is not null)
        {
            /// Prevent double starts
            return;
        }

        _cycleCancellation = new CancellationTokenSource();
        _cycleTask = CycleAsync(_cycleCancellation.Token);
    }

    public async Task StopAsync()
    {
        /// This method implementation is similar to <see cref="DisposeAsync"/>
        /// except for the change in the <c>_disposed</c> flag. It is
        /// maintained separately to emphasise semantic differences between
        /// stopping and disposing.
        if (_cycleCancellation is null)
        {
            /// Prevent double stops
            return;
        }

        await _cycleCancellation.CancelAsync();

        if (_cycleTask is not null)
        {
            try
            {
                await _cycleTask;
            }
            catch (OperationCanceledException)
            {
                // Expected; do nothing
            }
        }

        _cycleCancellation.Dispose();
        _cycleCancellation = null;
        _cycleTask = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed || _cycleCancellation is null)
        {
            return;
        }

        _disposed = true;
        await _cycleCancellation.CancelAsync();

        if (_cycleTask is not null)
        {
            try
            {
                await _cycleTask;
            }
            catch (OperationCanceledException)
            {
                // Expected; do nothing
            }
        }

        _cycleCancellation.Dispose();
        _cycleCancellation = null;
        _cycleTask = null;

        GC.SuppressFinalize(this);
    }

    private async Task CycleAsync(CancellationToken token)
    {
        while (true)
        {
            var delaySeconds = Random.Shared.Next(3, 11); // 3-10 seconds
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            OnPulse();
        }
    }

    private void OnPulse()
    {
        Pulse?.Invoke(this, EventArgs.Empty);
    }
}