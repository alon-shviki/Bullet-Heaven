using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="Projectile"/>: velocity integration and homing steer.
/// </summary>
public class ProjectileTests
{
    private const double Tolerance = 1e-9;

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_MovesByVelocityTimesSpeedTimesDt()
    {
        var p = new Projectile { X = 0, Y = 0, Vx = 1, Vy = 0, Speed = 400 };

        p.Update(0.25);

        Assert.Equal(100, p.X, Tolerance);
        Assert.Equal(0, p.Y, Tolerance);
    }

    [Fact]
    public void Update_ZeroDt_DoesNotMove()
    {
        var p = new Projectile { X = 10, Y = 20, Vx = 1, Vy = 1 };

        p.Update(0);

        Assert.Equal(10, p.X);
        Assert.Equal(20, p.Y);
    }

    [Fact]
    public void Update_NegativeVelocity_MovesBackward()
    {
        var p = new Projectile { X = 0, Y = 0, Vx = 0, Vy = -1, Speed = 200 };

        p.Update(0.5);

        Assert.Equal(0, p.X, Tolerance);
        Assert.Equal(-100, p.Y, Tolerance);
    }

    // ── ApplyHoming ───────────────────────────────────────────────────────────

    [Fact]
    public void ApplyHoming_SteersVelocityTowardTarget()
    {
        // Travelling east, target directly below (+y is down) — Vy must bend positive.
        var p = new Projectile { X = 0, Y = 0, Vx = 1, Vy = 0 };

        p.ApplyHoming(0.1, 0, 100);

        Assert.True(p.Vy > 0, "homing must bend velocity toward the target");
        Assert.True(p.Vx < 1, "the original axis must give up some share");
    }

    [Fact]
    public void ApplyHoming_ResultIsAlwaysUnitVector()
    {
        var p = new Projectile { X = 0, Y = 0, Vx = 1, Vy = 0 };

        p.ApplyHoming(0.08, 70, -30);

        var magnitude = Math.Sqrt(p.Vx * p.Vx + p.Vy * p.Vy);
        Assert.Equal(1, magnitude, Tolerance);
    }

    [Fact]
    public void ApplyHoming_FullStrength_PointsDirectlyAtTarget()
    {
        var p = new Projectile { X = 0, Y = 0, Vx = 1, Vy = 0 };

        p.ApplyHoming(1.0, 0, 50); // strength 1 fully replaces direction

        Assert.Equal(0, p.Vx, Tolerance);
        Assert.Equal(1, p.Vy, Tolerance);
    }

    [Fact]
    public void ApplyHoming_RepeatedCalls_ConvergeOnTarget()
    {
        var p = new Projectile { X = 0, Y = 0, Vx = 0, Vy = 1 }; // heading perpendicular

        for (var i = 0; i < 200; i++)
            p.ApplyHoming(0.08, 100, 0);

        Assert.Equal(1, p.Vx, 1e-3);
        Assert.Equal(0, p.Vy, 1e-3);
    }

    // ── Activate (pooled re-initialization, POOL-001) ─────────────────────────

    // Happy path: Activate applies every firing parameter and revives the slot.
    [Fact]
    public void Activate_SetsAllFiringParameters_AndRevives()
    {
        var p = new Projectile { IsAlive = false };

        p.Activate(10, 20, vx: 0.6, vy: 0.8, speed: 500, radius: 9,
                   piercing: 2, ricochet: 3);

        Assert.Equal(10, p.X);
        Assert.Equal(20, p.Y);
        Assert.Equal(0.6, p.Vx);
        Assert.Equal(0.8, p.Vy);
        Assert.Equal(500, p.Speed);
        Assert.Equal(9, p.Radius);
        Assert.Equal(2, p.PiercingLeft);
        Assert.Equal(3, p.RicochetLeft);
        Assert.True(p.IsAlive);
    }

    // Pooled slot reuse: a previous shot's spent piercing/ricochet budget and
    // velocity must not leak into the next shot.
    [Fact]
    public void Activate_AfterPreviousShot_ClearsAllStaleState()
    {
        var p = new Projectile();
        p.Activate(0, 0, vx: 1, vy: 0, speed: 400, radius: 6,
                   piercing: 5, ricochet: 4);
        p.PiercingLeft = 1;   // mostly spent in flight
        p.RicochetLeft = 0;   // fully spent
        p.Update(1.0);        // drifted away from spawn
        p.IsAlive = false;    // culled

        p.Activate(50, 60, vx: 0, vy: -1, speed: 300, radius: 4,
                   piercing: 0, ricochet: 0);

        Assert.True(p.IsAlive);
        Assert.Equal(50, p.X);              // no leftover drift
        Assert.Equal(60, p.Y);
        Assert.Equal(0, p.Vx);              // no leftover velocity
        Assert.Equal(-1, p.Vy);
        Assert.Equal(0, p.PiercingLeft);    // budget reset to the new stats
        Assert.Equal(0, p.RicochetLeft);
        Assert.Equal(300, p.Speed);
        Assert.Equal(4, p.Radius);
    }
}
