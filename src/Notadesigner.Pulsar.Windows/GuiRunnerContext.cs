using Notadesigner.Pulsar.Windows.Properties;
using System.Runtime.InteropServices;

namespace Notadesigner.Pulsar.Windows;

public class GuiRunnerContext : ApplicationContext
{
    private readonly ContextMenuStrip _contextMenu = new();

    private readonly Pulsar _pulsar = new();

    private readonly IconAnimator _iconAnimator;

    private int _direction = 1;

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

    private void PulsarPulseHandler(object? sender, EventArgs e)
    {
        var deltaX = Random.Shared.Next(1, 5);
        var deltaY = Random.Shared.Next(1, 5);
        Jiggle(deltaX * _direction, deltaY * _direction);

        _direction *= -1;
    }

    private async void GuiRunnerContextThreadExitHandler(object? sender, EventArgs e)
    {
        await _pulsar.DisposeAsync();
    }

    private static void Jiggle(int deltaX, int deltaY)
    {
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_MOUSE,
            mi = new NativeMethods.MOUSEINPUT
            {
                dx = deltaX,
                dy = deltaY,
                mouseData = 0,
                dwFlags = NativeMethods.MOUSEEVENTF_MOVE,
                time = 0,
                dwExtraInfo = 0
            }
        };

        var inputs = new[] { input };
        var result = NativeMethods.SendInput(1, inputs, Marshal.SizeOf<NativeMethods.INPUT>());

        if (result != 1)
        {
            var error = Marshal.GetLastWin32Error();
        }
    }
}