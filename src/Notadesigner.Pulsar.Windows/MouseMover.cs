using System.Runtime.InteropServices;
using Notadesigner.Pulsar.Windows.Movement;

namespace Notadesigner.Pulsar.Windows;

public class MouseMover
{
    private const int StepDelayMs = 16;  // ~60fps

    public async Task ExecutePathAsync(IEnumerable<Point2D> path, CancellationToken cancellationToken = default)
    {
        var previous = new Point2D(0, 0);
        double accumX = 0, accumY = 0;

        foreach (var point in path)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            accumX += point.X - previous.X;
            accumY += point.Y - previous.Y;

            int moveX = (int)accumX;
            int moveY = (int)accumY;

            accumX -= moveX;
            accumY -= moveY;

            if (moveX != 0 || moveY != 0)
            {
                SendMouseMove(moveX, moveY);
            }

            previous = point;
            await Task.Delay(StepDelayMs, cancellationToken);
        }

        // Final correction for accumulated remainder
        int finalX = (int)Math.Round(accumX);
        int finalY = (int)Math.Round(accumY);
        if (finalX != 0 || finalY != 0)
        {
            SendMouseMove(finalX, finalY);
        }
    }

    private static void SendMouseMove(int deltaX, int deltaY)
    {
        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_MOUSE,
            mi = new NativeMethods.MOUSEINPUT
            {
                dx = deltaX,
                dy = deltaY,
                dwFlags = NativeMethods.MOUSEEVENTF_MOVE
            }
        };

        NativeMethods.SendInput(1, [input], Marshal.SizeOf<NativeMethods.INPUT>());
    }
}
