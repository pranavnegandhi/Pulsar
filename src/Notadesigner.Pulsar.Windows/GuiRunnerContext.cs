using Notadesigner.Pulsar.Windows.Movement;
using Notadesigner.Pulsar.Windows.Properties;

namespace Notadesigner.Pulsar.Windows;

public class GuiRunnerContext : ApplicationContext
{
    private readonly ContextMenuStrip _contextMenu = new();

    private readonly Pulsar _pulsar = new();

    private readonly IconAnimator _iconAnimator;

    private readonly IPathGenerator _pathGenerator = new PhantomPathGenerator();

    private readonly MouseMover _mouseMover = new();

    public GuiRunnerContext()
    {
        ThreadExit += GuiRunnerContextThreadExitHandler;
        _pulsar.Pulse += PulsarPulseHandler;

        var icon = new NotifyIcon()
        {
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        _iconAnimator = new IconAnimator(icon);

        icon.Click += IconClickHandler;

        var startMenuItem = new ToolStripMenuItem("S&tart");
        startMenuItem.Click += async (_, _) => await StartJigglerAsync();
        _contextMenu.Items.Add(startMenuItem);

        var interruptMenuItem = new ToolStripMenuItem("I&nterrupt");
        interruptMenuItem.Click += async (_, _) => await StopJigglerAsync();
        _contextMenu.Items.Add(interruptMenuItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitMenuItem = new ToolStripMenuItem("E&xit");
        exitMenuItem.Click += (_, _) => Application.Exit();
        _contextMenu.Items.Add(exitMenuItem);
    }

    private async void IconClickHandler(object? sender, EventArgs e)
    {
        if (e is not MouseEventArgs mouseArgs || mouseArgs.Button != MouseButtons.Left)
        {
            return;
        }

        if (_pulsar.IsRunning)
        {
            await StopJigglerAsync();
        }
        else
        {
            await StartJigglerAsync();
        }
    }

    private async Task StartJigglerAsync()
    {
        _iconAnimator.Cancel();
        await _iconAnimator.AnimateStartAsync();
        _pulsar.Start();
    }

    private async Task StopJigglerAsync()
    {
        _iconAnimator.Cancel();
        await _iconAnimator.AnimateStopAsync();
        await _pulsar.StopAsync();
    }

    private async void PulsarPulseHandler(object? sender, EventArgs e)
    {
        try
        {
            var screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            var path = _pathGenerator.GeneratePath(screenBounds.Width, screenBounds.Height);
            await _mouseMover.ExecutePathAsync(path);
        }
        catch (OperationCanceledException)
        {
            // Animation cancelled, that's fine
        }
    }

    private async void GuiRunnerContextThreadExitHandler(object? sender, EventArgs e)
    {
        await _pulsar.DisposeAsync();
    }
}