namespace Notadesigner.Pulsar.Windows.Movement;

public interface IPathGenerator
{
    IEnumerable<Point2D> GeneratePath(int maxExtentX, int maxExtentY);
}
