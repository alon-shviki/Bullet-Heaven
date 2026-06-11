using BulletHeaven.Client.Game;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="GameMath"/> (Distance, Normalize, CirclesOverlap, FindNearest).
/// <see cref="GameMath.Clamp01"/> is covered separately in <see cref="GameMathClamp01Tests"/>.
/// </summary>
public class GameMathTests
{
    private const double Tolerance = 1e-9;

    // Minimal ICollidable stub for the overload-based APIs.
    private sealed class Circle(double x, double y, double radius) : ICollidable
    {
        public double X { get; } = x;
        public double Y { get; } = y;
        public double Radius { get; } = radius;
    }

    // ── Distance ──────────────────────────────────────────────────────────────

    [Fact]
    public void Distance_SamePoint_ReturnsZero()
    {
        Assert.Equal(0, GameMath.Distance(5, 5, 5, 5));
    }

    [Theory]
    [InlineData(0, 0, 3, 4, 5)]      // classic 3-4-5 triangle
    [InlineData(0, 0, -3, -4, 5)]    // negative coordinates
    [InlineData(1, 1, 1, 6, 5)]      // vertical only
    [InlineData(2, 0, 7, 0, 5)]      // horizontal only
    public void Distance_KnownPoints_ReturnsExpected(double x1, double y1, double x2, double y2, double expected)
    {
        Assert.Equal(expected, GameMath.Distance(x1, y1, x2, y2), Tolerance);
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        Assert.Equal(
            GameMath.Distance(1, 2, 8, -3),
            GameMath.Distance(8, -3, 1, 2),
            Tolerance);
    }

    [Fact]
    public void Distance_CollidableOverload_MatchesCoordinateOverload()
    {
        var a = new Circle(0, 0, 1);
        var b = new Circle(3, 4, 1);
        Assert.Equal(GameMath.Distance(0, 0, 3, 4), GameMath.Distance(a, b), Tolerance);
    }

    // ── Normalize ─────────────────────────────────────────────────────────────

    [Fact]
    public void Normalize_ZeroVector_ReturnsZero()
    {
        var (nx, ny) = GameMath.Normalize(0, 0);
        Assert.Equal(0, nx);
        Assert.Equal(0, ny);
    }

    [Theory]
    [InlineData(10, 0, 1, 0)]    // pure +x
    [InlineData(0, -5, 0, -1)]   // pure -y
    [InlineData(3, 4, 0.6, 0.8)] // diagonal
    public void Normalize_NonZeroVector_ReturnsUnitDirection(double dx, double dy, double ex, double ey)
    {
        var (nx, ny) = GameMath.Normalize(dx, dy);
        Assert.Equal(ex, nx, Tolerance);
        Assert.Equal(ey, ny, Tolerance);
    }

    [Fact]
    public void Normalize_ResultHasUnitMagnitude()
    {
        var (nx, ny) = GameMath.Normalize(-7.3, 12.9);
        Assert.Equal(1, Math.Sqrt(nx * nx + ny * ny), Tolerance);
    }

    // ── CirclesOverlap ────────────────────────────────────────────────────────

    [Fact]
    public void CirclesOverlap_Overlapping_ReturnsTrue()
    {
        var a = new Circle(0, 0, 5);
        var b = new Circle(6, 0, 5); // distance 6 < radii sum 10
        Assert.True(GameMath.CirclesOverlap(a, b));
    }

    [Fact]
    public void CirclesOverlap_Separated_ReturnsFalse()
    {
        var a = new Circle(0, 0, 2);
        var b = new Circle(10, 0, 2); // distance 10 > radii sum 4
        Assert.False(GameMath.CirclesOverlap(a, b));
    }

    // Boundary: exactly touching circles (distance == radii sum) do NOT overlap (strict <).
    [Fact]
    public void CirclesOverlap_ExactlyTouching_ReturnsFalse()
    {
        var a = new Circle(0, 0, 3);
        var b = new Circle(6, 0, 3);
        Assert.False(GameMath.CirclesOverlap(a, b));
    }

    // ── FindNearest ───────────────────────────────────────────────────────────

    [Fact]
    public void FindNearest_EmptyCandidates_ReturnsDefault()
    {
        var result = GameMath.FindNearest(0, 0, Array.Empty<Circle>());
        Assert.Null(result);
    }

    [Fact]
    public void FindNearest_ReturnsClosestCandidate()
    {
        var near = new Circle(1, 1, 1);
        var far  = new Circle(50, 50, 1);
        var mid  = new Circle(10, 0, 1);

        var result = GameMath.FindNearest(0, 0, new[] { far, near, mid });

        Assert.Same(near, result);
    }

    [Fact]
    public void FindNearest_SingleCandidate_ReturnsIt()
    {
        var only = new Circle(100, 100, 1);
        Assert.Same(only, GameMath.FindNearest(0, 0, new[] { only }));
    }
}
