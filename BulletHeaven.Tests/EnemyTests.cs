using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="Enemy"/>: damage/kill logic and chase movement.
/// </summary>
public class EnemyTests
{
    // ── TakeDamage ────────────────────────────────────────────────────────────

    [Fact]
    public void TakeDamage_NonLethal_ReturnsFalse_AndStaysAlive()
    {
        var e = new Enemy { MaxHealth = 3, CurrentHealth = 3 };

        var killed = e.TakeDamage(1);

        Assert.False(killed);
        Assert.True(e.IsAlive);
        Assert.Equal(2, e.CurrentHealth);
    }

    [Fact]
    public void TakeDamage_Lethal_ReturnsTrue_AndKills()
    {
        var e = new Enemy(); // 1 HP default

        var killed = e.TakeDamage(1);

        Assert.True(killed);
        Assert.False(e.IsAlive);
    }

    [Fact]
    public void TakeDamage_Overkill_StillKillsExactlyOnce()
    {
        var e = new Enemy { MaxHealth = 2, CurrentHealth = 2 };

        Assert.True(e.TakeDamage(100));
        Assert.False(e.IsAlive);
    }

    // Dead enemies must not report a second kill (would double-count score/XP).
    [Fact]
    public void TakeDamage_WhenAlreadyDead_ReturnsFalse()
    {
        var e = new Enemy();
        e.TakeDamage(1);

        Assert.False(e.TakeDamage(1));
    }

    [Fact]
    public void TakeDamage_DefaultAmount_IsOne()
    {
        var e = new Enemy { MaxHealth = 2, CurrentHealth = 2 };

        e.TakeDamage();

        Assert.Equal(1, e.CurrentHealth);
    }

    // ── Update (chase movement) ───────────────────────────────────────────────

    [Fact]
    public void Update_MovesTowardTarget()
    {
        var e = new Enemy { X = 0, Y = 0, Speed = 80 };

        e.Update(1.0, 100, 0); // target due east

        Assert.Equal(80, e.X, 1e-9);
        Assert.Equal(0, e.Y, 1e-9);
    }

    [Fact]
    public void Update_DiagonalChase_ScalesBySpeedAndDt()
    {
        var e = new Enemy { X = 0, Y = 0, Speed = 100 };

        e.Update(0.5, 30, 40); // unit direction (0.6, 0.8), 100px/s × 0.5s

        Assert.Equal(30, e.X, 1e-9);
        Assert.Equal(40, e.Y, 1e-9);
    }

    // Edge case: enemy exactly on the target — Normalize returns (0,0), no NaN drift.
    [Fact]
    public void Update_AtTargetPosition_DoesNotMoveOrProduceNaN()
    {
        var e = new Enemy { X = 50, Y = 50 };

        e.Update(1.0, 50, 50);

        Assert.Equal(50, e.X);
        Assert.Equal(50, e.Y);
    }

    // ── Activate (pooled re-initialization, POOL-002) ─────────────────────────

    // Happy path: Activate applies every spawn parameter and revives the slot.
    [Fact]
    public void Activate_SetsAllSpawnParameters_AndRevives()
    {
        var e = new Enemy { IsAlive = false };

        e.Activate(10, 20, EnemyType.Elite, radius: 16, speed: 120,
                   maxHealth: 5, killValue: 5, xpValue: 8, scoreValue: 80);

        Assert.Equal(10, e.X);
        Assert.Equal(20, e.Y);
        Assert.Equal(EnemyType.Elite, e.Type);
        Assert.Equal(16, e.Radius);
        Assert.Equal(120, e.Speed);
        Assert.Equal(5, e.MaxHealth);
        Assert.Equal(5, e.CurrentHealth); // full health, not stale
        Assert.Equal(5, e.KillValue);
        Assert.Equal(8, e.XpValue);
        Assert.Equal(80, e.ScoreValue);
        Assert.True(e.IsAlive);
    }

    // Pooled slot reuse: no state from a previous life may leak into the next —
    // damage, hit flash, and orb cooldown must all be wiped.
    [Fact]
    public void Activate_AfterPreviousLife_ClearsAllStaleState()
    {
        var e = new Enemy();
        e.Activate(0, 0, EnemyType.Tank, radius: 20, speed: 50,
                   maxHealth: 3, killValue: 2, xpValue: 3, scoreValue: 25);
        e.TakeDamage(3);          // dies: IsAlive false, CurrentHealth 0
        e.HitFlash = 0.15;        // mid-flash when it died
        e.OrbHitCooldown = 0.4;   // orb had just hit it

        e.Activate(99, 88, EnemyType.Runner, radius: 8, speed: 150,
                   maxHealth: 1, killValue: 1, xpValue: 1, scoreValue: 15);

        Assert.True(e.IsAlive);
        Assert.Equal(1, e.CurrentHealth);   // no leftover damage
        Assert.Equal(0, e.HitFlash);        // no leftover flash
        Assert.Equal(0, e.OrbHitCooldown);  // no leftover orb cooldown
        Assert.Equal(EnemyType.Runner, e.Type);
        Assert.Equal(99, e.X);
        Assert.Equal(88, e.Y);
    }

    // Reactivated slot behaves like a brand-new enemy: lethal damage works again.
    [Fact]
    public void Activate_ReusedSlot_CanBeKilledAgain()
    {
        var e = new Enemy();
        e.TakeDamage(1); // first life over

        e.Activate(0, 0, EnemyType.Standard, radius: 12, speed: 80,
                   maxHealth: 2, killValue: 1, xpValue: 1, scoreValue: 10);

        Assert.False(e.TakeDamage(1)); // 2 → 1, survives
        Assert.True(e.TakeDamage(1));  // 1 → 0, second kill reported
        Assert.False(e.IsAlive);
    }
}
