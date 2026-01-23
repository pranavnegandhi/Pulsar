namespace Notadesigner.Pulsar.Windows.Movement;

public class JigglePathGenerator : IPathGenerator
{
    public IEnumerable<Point2D> GeneratePath(int maxExtentX, int maxExtentY)
    {
        // Original behavior: small random movement (1-4px) in one direction
        var deltaX = Random.Shared.Next(1, 5);
        var deltaY = Random.Shared.Next(1, 5);

        // Move out
        yield return new Point2D(deltaX, deltaY);

        // Move back (return to origin)
        yield return new Point2D(0, 0);
    }
}
