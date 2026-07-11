using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Entities;
using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="Weapon"/>: fire-rate cooldown, nearest-enemy
/// targeting, multi-bullet spread, and stat propagation to spawned projectiles.
/// Since POOL-001 the weapon activates slots in a <see cref="BulletPool"/>
/// instead of allocating; bullets are read back from the pool's live slots.
/// </summary>
public class WeaponTests
{
    private const double Tolerance = 1e-9;

    private static (Weapon weapon, Player player, EnemyPool enemies, BulletPool projectiles) Setup()
    {
        var player = new Player(); // centered at GameBounds center
        return (new Weapon(), player, new EnemyPool(), new BulletPool());
    }

    private static Enemy AddEnemy(EnemyPool pool, double x, double y)
    {
        Assert.True(pool.TryRent(out var e), "enemy pool unexpectedly full");
        e.Activate(x, y, EnemyType.Standard, radius: 12, speed: 80,
                   maxHealth: 1, killValue: 1, xpValue: 1, scoreValue: 10);
        return e;
    }

    /// <summary>Live bullets in slot order — the pool rents sequentially, so this is firing order.</summary>
    private static List<Projectile> AliveBullets(BulletPool pool) =>
        pool.Slots.Where(p => p.IsAlive).ToList();

    // ── Firing conditions ─────────────────────────────────────────────────────

    [Fact]
    public void Update_WithEnemyPresent_FiresImmediately()
    {
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, 0, 0);

        w.Update(0.016, player, enemies, projectiles, new WeaponStats());

        Assert.Equal(1, projectiles.CountAlive());
    }

    [Fact]
    public void Update_NoEnemies_DoesNotFire()
    {
        var (w, player, enemies, projectiles) = Setup();

        w.Update(0.016, player, enemies, projectiles, new WeaponStats());

        Assert.Equal(0, projectiles.CountAlive());
    }

    [Fact]
    public void Update_DeadEnemiesOnly_DoesNotFire()
    {
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, 0, 0).IsAlive = false;

        w.Update(0.016, player, enemies, projectiles, new WeaponStats());

        Assert.Equal(0, projectiles.CountAlive());
    }

    [Fact]
    public void Update_DuringCooldown_DoesNotFireAgain()
    {
        var stats = new WeaponStats { FireRate = 1.0 };
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, 0, 0);

        w.Update(0.016, player, enemies, projectiles, stats);
        w.Update(0.5, player, enemies, projectiles, stats); // cooldown still active

        Assert.Equal(1, projectiles.CountAlive());
    }

    [Fact]
    public void Update_AfterCooldownElapses_FiresAgain()
    {
        var stats = new WeaponStats { FireRate = 1.0 };
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, 0, 0);

        w.Update(0.016, player, enemies, projectiles, stats);
        w.Update(1.1, player, enemies, projectiles, stats);

        Assert.Equal(2, projectiles.CountAlive());
    }

    [Fact]
    public void Reset_ClearsCooldown_SoNextUpdateFires()
    {
        var stats = new WeaponStats { FireRate = 5.0 };
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, 0, 0);
        w.Update(0.016, player, enemies, projectiles, stats);

        w.Reset();
        w.Update(0.016, player, enemies, projectiles, stats);

        Assert.Equal(2, projectiles.CountAlive());
    }

    // ── Targeting ─────────────────────────────────────────────────────────────

    [Fact]
    public void Update_AimsAtNearestEnemy()
    {
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, player.X, player.Y - 300); // due north, far
        AddEnemy(enemies, player.X + 50, player.Y);  // due east, close

        w.Update(0.016, player, enemies, projectiles, new WeaponStats());

        var shot = Assert.Single(AliveBullets(projectiles));
        Assert.Equal(1, shot.Vx, Tolerance); // unit vector pointing east
        Assert.Equal(0, shot.Vy, Tolerance);
    }

    // ── Spread / multi-bullet ─────────────────────────────────────────────────

    [Fact]
    public void Update_TriShot_FiresThreeBulletsCenteredOnTarget()
    {
        var stats = new WeaponStats { BulletCount = 3, SpreadAngle = Math.PI / 12 };
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, player.X + 100, player.Y); // due east → angle 0

        w.Update(0.016, player, enemies, projectiles, stats);

        var shots = AliveBullets(projectiles);
        Assert.Equal(3, shots.Count);

        // Middle bullet flies straight at the target; outer two are ±SpreadAngle.
        Assert.Equal(Math.Cos(-Math.PI / 12), shots[0].Vx, Tolerance);
        Assert.Equal(Math.Sin(-Math.PI / 12), shots[0].Vy, Tolerance);
        Assert.Equal(1, shots[1].Vx, Tolerance);
        Assert.Equal(0, shots[1].Vy, Tolerance);
        Assert.Equal(Math.Cos(Math.PI / 12), shots[2].Vx, Tolerance);
        Assert.Equal(Math.Sin(Math.PI / 12), shots[2].Vy, Tolerance);
    }

    // ── Stat propagation ──────────────────────────────────────────────────────

    [Fact]
    public void Update_ProjectileInheritsWeaponStats()
    {
        var stats = new WeaponStats
        {
            BulletSpeed = 555,
            BulletRadius = 9,
            Piercing = 2,
            RicochetCount = 4,
        };
        var (w, player, enemies, projectiles) = Setup();
        AddEnemy(enemies, 0, 0);

        w.Update(0.016, player, enemies, projectiles, stats);

        var shot = Assert.Single(AliveBullets(projectiles));
        Assert.Equal(555, shot.Speed);
        Assert.Equal(9, shot.Radius);
        Assert.Equal(2, shot.PiercingLeft);
        Assert.Equal(4, shot.RicochetLeft);
        Assert.Equal(player.X, shot.X); // spawns at the player
        Assert.Equal(player.Y, shot.Y);
    }
}
