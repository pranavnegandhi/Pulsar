using Notadesigner.Pulsar.Windows.Properties;

namespace Notadesigner.Pulsar.Windows;

public class IconAnimator
{
    private readonly NotifyIcon _icon;
    private readonly Icon _idleIcon;
    private readonly Icon _activeIcon;
    private readonly Icon[] _startFrames;
    private readonly Icon[] _stopFrames;
    private CancellationTokenSource? _animationCts;

    public IconAnimator(NotifyIcon icon)
    {
        _icon = icon;

        // Load icons from resources
        _idleIcon = IconFromBitmap(Default.Idle);
        _activeIcon = IconFromBitmap(Default.Active);

        var active_25 = IconFromBitmap(Default.Active_25);
        var active_50 = IconFromBitmap(Default.Active_50);
        var active_75 = IconFromBitmap(Default.Active_75);
        var idle_25 = IconFromBitmap(Default.Idle_25);
        var idle_50 = IconFromBitmap(Default.Idle_50);
        var idle_75 = IconFromBitmap(Default.Idle_75);

        _startFrames = [_idleIcon, active_25, active_50, active_75, _activeIcon];
        _stopFrames = [_activeIcon, idle_25, idle_50, idle_75, _idleIcon];

        // Set initial icon
        _icon.Icon = _idleIcon;
    }

    public async Task AnimateStartAsync()
    {
        await AnimateAsync(_startFrames);
    }

    public async Task AnimateStopAsync()
    {
        await AnimateAsync(_stopFrames);
    }

    public void Cancel()
    {
        _animationCts?.Cancel();
    }

    private async Task AnimateAsync(Icon[] frames)
    {
        _animationCts?.Cancel();
        _animationCts = new CancellationTokenSource();

        try
        {
            foreach (var frame in frames)
            {
                if (_animationCts.Token.IsCancellationRequested)
                {
                    break;
                }

                _icon.Icon = frame;
                await Task.Delay(150, _animationCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Animation cancelled, that's fine
        }
    }

    private static Icon IconFromBitmap(Bitmap bitmap)
    {
        var handle = bitmap.GetHicon();
        return Icon.FromHandle(handle);
    }
}