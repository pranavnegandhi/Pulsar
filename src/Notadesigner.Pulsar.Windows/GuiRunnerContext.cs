using Notadesigner.Pulsar.Windows.Properties;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Notadesigner.Pulsar.Windows;

public class GuiRunnerContext : ApplicationContext
{
    private readonly ContextMenuStrip _contextMenu = new();

    private readonly Pulsar _pulsar = new();

    private int _direction = 1;

    public GuiRunnerContext()
    {
        ThreadExit += GuiRunnerContextThreadExitHandler;
        _pulsar.Pulse += PulsarPulseHandler;

        var icon = new NotifyIcon()
        {
            ContextMenuStrip = _contextMenu,
            Icon = Default.MainIcon,
            Visible = true
        };

        icon.Click += IconClickHandler;

        var startMenuItem = new ToolStripMenuItem("S&tart");
        startMenuItem.Click += (_, _) => _pulsar.Start();
        _contextMenu.Items.Add(startMenuItem);

        var interruptMenuItem = new ToolStripMenuItem("I&nterrupt");
        interruptMenuItem.Click += async (_, _) => await _pulsar.StopAsync();
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
            await _pulsar.StopAsync();
        }
        else
        {
            _pulsar.Start();
        }
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
        var input = new INPUT()
        {
            type = INPUT_TYPE.INPUT_MOUSE,
            Anonymous = new INPUT._Anonymous_e__Union()
            {
                mi = new MOUSEINPUT()
                {
                    dx = deltaX,
                    dy = deltaY,
                    mouseData = 0,
                    dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE,
                    time = 0,
                    dwExtraInfo = 0
                }
            }
        };

        var result = PInvoke.SendInput(new ReadOnlySpan<INPUT>(in input), Marshal.SizeOf<INPUT>());

        if (result == 1)
        {
            return;
        }

        var error = Marshal.GetLastWin32Error();
    }
}