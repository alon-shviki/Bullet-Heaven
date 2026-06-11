using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="XpGem"/> magnet behaviour: idle outside range,
/// attracted inside range, and stationary once it reaches the player.
/// </summary>
public class XpGemTests
{
    [Fact]
    public void Update_OutsideMagnetRange_DoesNotMove()
    {
        var gem = new XpGem { X = 0, Y = 0 };

        gem.Update(1.0, px: 500, py: 0, magnetRange: 80);

        Assert.Equal(0, gem.X);
        Assert.Equal(0, gem.Y);
    }

    [Fact]
    public void Update_InsideMagnetRange_MovesTowardPlayer()
    {
        var gem = new XpGem { X = 0, Y = 0 };

        gem.Update(0.1, px: 50, py: 0, magnetRange: 80);

        Assert.True(gem.X > 0, "gem must be pulled toward the player");
        Assert.Equal(0, gem.Y, 1e-9);
    }

    // Closer gems are pulled faster (speed scales up as distance shrinks).
    [Fact]
    public void Update_CloserGem_MovesFasterThanFartherGem()
    {
        var close = new XpGem { X = 60, Y = 0 }; // 20px from player
        var far   = new XpGem { X = 10, Y = 0 }; // 70px from player

        close.Update(0.01, px: 80, py: 0, magnetRange: 80);
        far.Update(0.01, px: 80, py: 0, magnetRange: 80);

        var closeMoved = close.X - 60;
        var farMoved   = far.X - 10;
        Assert.True(closeMoved > farMoved, "attraction speed must increase as the gem gets closer");
    }

    // Edge case: gem essentially on top of the player (dist < 0.1) stays put — no jitter.
    [Fact]
    public void Update_AtPlayerPosition_DoesNotJitter()
    {
        var gem = new XpGem { X = 100, Y = 100 };

        gem.Update(1.0, px: 100.05, py: 100, magnetRange: 80);

        Assert.Equal(100, gem.X);
        Assert.Equal(100, gem.Y);
    }

    // Boundary: minimum pull speed is 120 px/s even at the very edge of the range.
    [Fact]
    public void Update_NearRangeEdge_StillPullsAtMinimumSpeed()
    {
        var gem = new XpGem { X = 0, Y = 0 };

        gem.Update(0.1, px: 79, py: 0, magnetRange: 80); // dist 79, just inside

        Assert.True(gem.X >= 12 - 1e-6, $"expected ≥12px (120px/s × 0.1s), got {gem.X}");
    }

    [Fact]
    public void Value_DefaultsToOne()
    {
        Assert.Equal(1, new XpGem().Value);
    }

    // ── Activate (pooled re-initialization, POOL-001/002 follow-up) ───────────

    // Happy path: Activate places the gem at the kill location with its value.
    [Fact]
    public void Activate_SetsPositionValue_AndRevives()
    {
        var gem = new XpGem { IsAlive = false };

        gem.Activate(120, 340, value: 8);

        Assert.Equal(120, gem.X);
        Assert.Equal(340, gem.Y);
        Assert.Equal(8, gem.Value);
        Assert.True(gem.IsAlive);
    }

    // Boundary: the value parameter defaults to 1.
    [Fact]
    public void Activate_DefaultValue_IsOne()
    {
        var gem = new XpGem { Value = 30 }; // stale boss-drop value

        gem.Activate(0, 0);

        Assert.Equal(1, gem.Value);
    }

    // Pooled slot reuse: a collected gem's old position and value must not
    // leak into its next life.
    [Fact]
    public void Activate_AfterPreviousLife_ClearsAllStaleState()
    {
        var gem = new XpGem();
        gem.Activate(500, 500, value: 30); // boss drop
        gem.IsAlive = false;               // collected

        gem.Activate(10, 20, value: 1);

        Assert.True(gem.IsAlive);
        Assert.Equal(10, gem.X);
        Assert.Equal(20, gem.Y);
        Assert.Equal(1, gem.Value);
    }
}
