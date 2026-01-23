namespace Notadesigner.Pulsar.Windows.Movement;

public class BezierPathGenerator : IPathGenerator
{
    private const int StepCount = 100;

    public IEnumerable<Point2D> GeneratePath(int maxExtentX, int maxExtentY)
    {
        var controlPoints = GenerateClosedLoopControlPoints(maxExtentX, maxExtentY);
        return SampleBezierPath(controlPoints);
    }

    private static Point2D CubicBezier(Point2D p0, Point2D p1, Point2D p2, Point2D p3, double t)
    {
        double u = 1 - t;
        double tt = t * t;
        double uu = u * u;
        double uuu = uu * u;
        double ttt = tt * t;

        return new Point2D(
            uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X,
            uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y
        );
    }

    private static (Point2D[] Outbound, Point2D[] Return) GenerateClosedLoopControlPoints(int maxX, int maxY)
    {
        var origin = new Point2D(0, 0);

        var destination = new Point2D(
            Random.Shared.Next(-maxX / 2, maxX / 2 + 1),
            Random.Shared.Next(-maxY / 2, maxY / 2 + 1)
        );

        Point2D RandomPoint() => new(
            Random.Shared.Next(-maxX / 2, maxX / 2 + 1),
            Random.Shared.Next(-maxY / 2, maxY / 2 + 1)
        );

        return (
            Outbound: [origin, RandomPoint(), RandomPoint(), destination],
            Return: [destination, RandomPoint(), RandomPoint(), origin]
        );
    }

    private static IEnumerable<Point2D> SampleBezierPath((Point2D[] Outbound, Point2D[] Return) controlPoints)
    {
        int stepsPerSegment = StepCount / 2;

        // Outbound curve
        for (int i = 0; i <= stepsPerSegment; i++)
        {
            double t = (double)i / stepsPerSegment;
            var cp = controlPoints.Outbound;
            yield return CubicBezier(cp[0], cp[1], cp[2], cp[3], t);
        }

        // Return curve (skip first point - same as last outbound)
        for (int i = 1; i <= stepsPerSegment; i++)
        {
            double t = (double)i / stepsPerSegment;
            var cp = controlPoints.Return;
            yield return CubicBezier(cp[0], cp[1], cp[2], cp[3], t);
        }
    }
}
