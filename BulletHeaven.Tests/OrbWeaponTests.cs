using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Entities;
using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for the allocation-free <see cref="OrbWeapon"/> rework:
/// fixed 16-slot position buffer (ActiveOrbCount / GetOrbPosition), MaxOrbs
/// clamp, orbit geometry, contact damage with the per-enemy
/// <see cref="Enemy.OrbHitCooldown"/>, kill counting, and Reset.
/// A large-radius enemy parked on the player keeps the orbiting orb in
/// permanent contact, making cooldown timing observable at any dt.
/// </summary>
public class OrbWeaponTests
{
    private const double Tolerance = 1e-9;

    private static (OrbWeapon weapon, Player player, EnemyPool enemies) Setup() =>
        (new OrbWeapon(), new Player(), new EnemyPool());

    /// <summary>Enemy whose body always contains the orbit ring (radius ≫ OrbOrbitRadius).</summary>
    private static Enemy AddEnemyCoveringOrbit(EnemyPool pool, Player player, int maxHealth)
    {
        Assert.True(pool.TryRent(out var e), "enemy pool unexpectedly full");
        e.Activate(player.X, player.Y, EnemyType.Tank, radius: 200, speed: 0,
                   maxHealth: maxHealth, killValue: 1, xpValue: 1, scoreValue: 10);
        return e;
    }

    // ── ActiveOrbCount / buffer bookkeeping ───────────────────────────────────

    // Happy path: the buffer tracks stats.OrbCount.
    [Fact]
    public void Update_ActiveOrbCount_TracksStatsOrbCount()
    {
        var (w, player, enemies) = Setup();

        w.Update(0.016, player, enemies, new WeaponStats { OrbCount = 3 });

        Assert.Equal(3, w.ActiveOrbCount);
    }

    // Boundary: OrbCount above the fixed buffer is clamped to MaxOrbs (16).
    [Fact]
    public void Update_OrbCountAboveMax_ClampsToMaxOrbs()
    {
        var (w, player, enemies) = Setup();

        w.Update(0.016, player, enemies, new WeaponStats { OrbCount = 99 });

        Assert.Equal(16, OrbWeapon.MaxOrbs);
        Assert.Equal(OrbWeapon.MaxOrbs, w.ActiveOrbCount);
    }

    // Boundary: zero orbs — no positions, no kills.
    [Fact]
    public void Update_ZeroOrbs_ReportsZeroActiveAndNoKills()
    {
        var (w, player, enemies) = Setup();
        AddEnemyCoveringOrbit(enemies, player, maxHealth: 1);

        var kills = w.Update(0.016, player, enemies, new WeaponStats { OrbCount = 0 });

        Assert.Equal(0, w.ActiveOrbCount);
        Assert.Equal(0, kills);
        Assert.Equal(1, enemies.Slots[0].CurrentHealth); // untouched
    }

    // ── Orbit geometry ────────────────────────────────────────────────────────

    // Happy path: a single orb sits on the orbit ring at angle dt·π.
    // dt = 0.5 → angle π/2 → directly below the player (+y).
    [Fact]
    public void Update_SingleOrb_OrbitsAtConfiguredRadius()
    {
        var (w, player, enemies) = Setup();

        w.Update(0.5, player, enemies, new WeaponStats { OrbCount = 1, OrbOrbitRadius = 80 });

        var (x, y) = w.GetOrbPosition(0);
        Assert.Equal(player.X, x, Tolerance);
        Assert.Equal(player.Y + 80, y, Tolerance);
    }

    // Happy path: multiple orbs are spaced evenly around the ring.
    [Fact]
    public void Update_FourOrbs_AreEvenlySpacedAroundThePlayer()
    {
        var (w, player, enemies) = Setup();
        const double dt = 1e-12; // angle advance is negligible

        w.Update(dt, player, enemies, new WeaponStats { OrbCount = 4, OrbOrbitRadius = 80 });

        var expected = new (double X, double Y)[]
        {
            (player.X + 80, player.Y),
            (player.X, player.Y + 80),
            (player.X - 80, player.Y),
            (player.X, player.Y - 80),
        };
        for (var i = 0; i < 4; i++)
        {
            var (x, y) = w.GetOrbPosition(i);
            Assert.Equal(expected[i].X, x, 1e-6);
            Assert.Equal(expected[i].Y, y, 1e-6);
        }
    }

    // ── Contact damage + per-enemy cooldown ───────────────────────────────────

    // Happy path: an orb touching an enemy deals 1 damage and arms the
    // enemy's personal cooldown.
    [Fact]
    public void Update_OrbTouchingEnemy_DealsOneDamage_AndArmsCooldown()
    {
        var (w, player, enemies) = Setup();
        var e = AddEnemyCoveringOrbit(enemies, player, maxHealth: 5);

        var kills = w.Update(0.001, player, enemies, new WeaponStats { OrbCount = 1 });

        Assert.Equal(0, kills);
        Assert.Equal(4, e.CurrentHealth);
        Assert.True(e.OrbHitCooldown > 0, "hit must arm the per-enemy cooldown");
    }

    // While the per-enemy cooldown is running, continued contact deals no damage.
    [Fact]
    public void Update_DuringCooldown_DoesNotHitAgain()
    {
        var (w, player, enemies) = Setup();
        var e = AddEnemyCoveringOrbit(enemies, player, maxHealth: 5);
        var stats = new WeaponStats { OrbCount = 1 };
        w.Update(0.001, player, enemies, stats); // first hit → cooldown 0.4

        w.Update(0.1, player, enemies, stats); // 0.3 remaining — still blocked

        Assert.Equal(4, e.CurrentHealth);
    }

    // The cooldown is decremented by dt in Update; once it expires the enemy
    // can be hit again.
    [Fact]
    public void Update_AfterCooldownExpires_HitsAgain()
    {
        var (w, player, enemies) = Setup();
        var e = AddEnemyCoveringOrbit(enemies, player, maxHealth: 5);
        var stats = new WeaponStats { OrbCount = 1 };
        w.Update(0.001, player, enemies, stats); // hp 4, cooldown 0.4
        w.Update(0.1, player, enemies, stats);   // hp 4, cooldown 0.3

        w.Update(0.5, player, enemies, stats);   // cooldown expired → second hit

        Assert.Equal(3, e.CurrentHealth);
    }

    // The cooldown is per enemy: several orbs touching the same enemy in one
    // frame land only a single hit.
    [Fact]
    public void Update_MultipleOrbsOnSameEnemy_HitOncePerCooldownWindow()
    {
        var (w, player, enemies) = Setup();
        var e = AddEnemyCoveringOrbit(enemies, player, maxHealth: 10);

        w.Update(0.001, player, enemies, new WeaponStats { OrbCount = 4 });

        Assert.Equal(9, e.CurrentHealth); // exactly one hit, not four
    }

    // Independent enemies each take their own hit in the same frame.
    [Fact]
    public void Update_TwoOverlappingEnemies_EachTakeTheirOwnHit()
    {
        var (w, player, enemies) = Setup();
        var a = AddEnemyCoveringOrbit(enemies, player, maxHealth: 5);
        var b = AddEnemyCoveringOrbit(enemies, player, maxHealth: 5);

        w.Update(0.001, player, enemies, new WeaponStats { OrbCount = 1 });

        Assert.Equal(4, a.CurrentHealth);
        Assert.Equal(4, b.CurrentHealth);
    }

    // ── Kill counting ─────────────────────────────────────────────────────────

    // A lethal orb hit is reported in the return value and kills the slot.
    [Fact]
    public void Update_LethalHit_CountsKill_AndDeactivatesEnemy()
    {
        var (w, player, enemies) = Setup();
        var e = AddEnemyCoveringOrbit(enemies, player, maxHealth: 1);

        var kills = w.Update(0.001, player, enemies, new WeaponStats { OrbCount = 1 });

        Assert.Equal(1, kills);
        Assert.False(e.IsAlive);
    }

    // Expected error path: dead pool slots (stale positions) are ignored.
    [Fact]
    public void Update_DeadEnemySlot_IsNeverHit()
    {
        var (w, player, enemies) = Setup();
        var e = AddEnemyCoveringOrbit(enemies, player, maxHealth: 5);
        e.IsAlive = false; // killed elsewhere; slot keeps stale position

        var kills = w.Update(0.001, player, enemies, new WeaponStats { OrbCount = 1 });

        Assert.Equal(0, kills);
        Assert.Equal(5, e.CurrentHealth);
        Assert.Equal(0, e.OrbHitCooldown);
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    // Restart path: Reset clears the buffer count and rewinds the orbit angle.
    [Fact]
    public void Reset_ClearsActiveOrbCount_AndRewindsAngle()
    {
        var (w, player, enemies) = Setup();
        w.Update(0.7, player, enemies, new WeaponStats { OrbCount = 3 });

        w.Reset();

        Assert.Equal(0, w.ActiveOrbCount);

        // Angle restarts from 0: a tiny dt puts orb 0 due east of the player again.
        w.Update(1e-12, player, enemies, new WeaponStats { OrbCount = 1, OrbOrbitRadius = 80 });
        var (x, y) = w.GetOrbPosition(0);
        Assert.Equal(player.X + 80, x, 1e-6);
        Assert.Equal(player.Y, y, 1e-6);
    }
}
