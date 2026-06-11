using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="Entity.IsOffScreen"/> — the check that backs
/// projectile culling. Boundary semantics are strict: an entity exactly at
/// ±Radius outside the edge is still considered on-screen.
/// Assumes the default 800×600 <see cref="GameBounds"/> (never mutated).
/// </summary>
public class EntityTests
{
    private static Projectile At(double x, double y) => new() { X = x, Y = y, Radius = 6 };

    [Fact]
    public void IsOffScreen_CenterOfField_ReturnsFalse()
    {
        Assert.False(At(GameBounds.CenterX, GameBounds.CenterY).IsOffScreen());
    }

    // Boundary: exactly at -Radius (strict <) is still on-screen on every edge.
    [Fact]
    public void IsOffScreen_ExactlyAtEdgeThreshold_ReturnsFalse()
    {
        Assert.False(At(-6, 300).IsOffScreen());
        Assert.False(At(GameBounds.Width + 6, 300).IsOffScreen());
        Assert.False(At(400, -6).IsOffScreen());
        Assert.False(At(400, GameBounds.Height + 6).IsOffScreen());
    }

    [Fact]
    public void IsOffScreen_JustPastEachEdge_ReturnsTrue()
    {
        Assert.True(At(-6.01, 300).IsOffScreen());
        Assert.True(At(GameBounds.Width + 6.01, 300).IsOffScreen());
        Assert.True(At(400, -6.01).IsOffScreen());
        Assert.True(At(400, GameBounds.Height + 6.01).IsOffScreen());
    }

    [Fact]
    public void IsOffScreen_FarAway_ReturnsTrue()
    {
        Assert.True(At(-1000, -1000).IsOffScreen());
    }

    // A bigger radius extends how far the entity may travel before being culled.
    [Fact]
    public void IsOffScreen_AccountsForRadius()
    {
        var big = new Projectile { X = -10, Y = 300, Radius = 12 };
        var small = new Projectile { X = -10, Y = 300, Radius = 6 };

        Assert.False(big.IsOffScreen());
        Assert.True(small.IsOffScreen());
    }
}
