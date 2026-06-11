namespace BulletHeaven.Client.Game;

public static class GameMath
{
    public static double Distance(ICollidable a, ICollidable b) =>
        Distance(a.X, a.Y, b.X, b.Y);

    public static double Distance(double x1, double y1, double x2, double y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static (double nx, double ny) Normalize(double dx, double dy)
    {
        var mag = Math.Sqrt(dx * dx + dy * dy);
        return mag == 0 ? (0, 0) : (dx / mag, dy / mag);
    }

    public static bool CirclesOverlap(ICollidable a, ICollidable b) =>
        Distance(a, b) < a.Radius + b.Radius;

    public static T? FindNearest<T>(double fromX, double fromY, IEnumerable<T> candidates)
        where T : ICollidable
    {
        T? nearest = default;
        var minDist = double.MaxValue;
        foreach (var c in candidates)
        {
            var d = Distance(fromX, fromY, c.X, c.Y);
            if (d < minDist) { minDist = d; nearest = c; }
        }
        return nearest;
    }

    public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);
}
