namespace Notadesigner.Pulsar.Windows.Movement;

/// <summary>
/// Generates a zero-movement path that triggers system activity without visible cursor motion.
/// The system detects input activity (preventing screensavers/idle), but the cursor stays still.
/// Note: May not work with applications that implement their own idle detection.
/// </summary>
public class PhantomPathGenerator : IPathGenerator
{
    public IEnumerable<Point2D> GeneratePath(int maxExtentX, int maxExtentY)
    {
        // Single zero-delta point - triggers input event without moving cursor
        yield return new Point2D(0, 0);
    }
}
