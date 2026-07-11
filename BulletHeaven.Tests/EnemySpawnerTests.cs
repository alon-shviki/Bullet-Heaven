using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Entities;
using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="EnemySpawner"/>: spawn cadence, wave size,
/// per-type stat integrity, boss spawning, and reset.
/// Since POOL-002 the spawner activates slots in an <see cref="EnemyPool"/>
/// instead of allocating; spawned enemies are read back from live slots.
/// Spawn positions/types are random, so tests assert invariants (counts,
/// outside-the-field placement, type→stat consistency) rather than exact values.
/// </summary>
public class EnemySpawnerTests
{
    private static List<Enemy> Alive(EnemyPool pool) =>
        pool.Slots.Where(e => e.IsAlive).ToList();

    // ── Cadence ───────────────────────────────────────────────────────────────

    [Fact]
    public void Update_BeforeIntervalElapses_SpawnsNothing()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();

        spawner.Update(0.5, enemies, spawnInterval: 1.5, enemySpeed: 100, count: 2);

        Assert.Equal(0, enemies.CountAlive());
    }

    [Fact]
    public void Update_WhenIntervalElapses_SpawnsRequestedCount()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();

        spawner.Update(1.5, enemies, spawnInterval: 1.5, enemySpeed: 100, count: 3);

        Assert.Equal(3, enemies.CountAlive());
    }

    [Fact]
    public void Update_AccumulatesTimeAcrossCalls()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();

        spawner.Update(0.8, enemies, 1.5, 100, 2);
        spawner.Update(0.8, enemies, 1.5, 100, 2); // total 1.6 ≥ 1.5

        Assert.Equal(2, enemies.CountAlive());
    }

    // The timer keeps the overshoot, so waves stay on cadence rather than drifting.
    [Fact]
    public void Update_CarriesRemainderIntoNextInterval()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();

        spawner.Update(2.0, enemies, 1.5, 100, 1);  // spawns, 0.5 carried over
        spawner.Update(1.0, enemies, 1.5, 100, 1);  // 0.5 + 1.0 = 1.5 → spawns again

        Assert.Equal(2, enemies.CountAlive());
    }

    [Fact]
    public void Reset_ClearsAccumulatedTime()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();
        spawner.Update(1.4, enemies, 1.5, 100, 1);

        spawner.Reset();
        spawner.Update(1.4, enemies, 1.5, 100, 1); // would spawn without the reset

        Assert.Equal(0, enemies.CountAlive());
    }

    // ── Spawned enemy integrity ───────────────────────────────────────────────

    [Fact]
    public void SpawnedEnemies_AppearOutsideThePlayField()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();

        spawner.Update(1.5, enemies, 1.5, 100, 50);

        Assert.All(Alive(enemies), e => Assert.True(
            e.X < 0 || e.X > GameBounds.Width || e.Y < 0 || e.Y > GameBounds.Height,
            $"enemy spawned inside the field at ({e.X:F1}, {e.Y:F1})"));
    }

    [Fact]
    public void SpawnedEnemies_HaveStatsConsistentWithTheirType()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();
        const double baseSpeed = 100;

        spawner.Update(1.5, enemies, 1.5, baseSpeed, 200); // large sample to hit all types

        Assert.All(Alive(enemies), e =>
        {
            var (radius, speedMult, hp) = e.Type switch
            {
                EnemyType.Runner => (8.0, 1.5, 1),
                EnemyType.Tank => (20.0, 0.5, 3),
                EnemyType.Elite => (16.0, 1.2, 5),
                _ => (12.0, 1.0, 1),
            };
            Assert.Equal(radius, e.Radius);
            Assert.Equal(baseSpeed * speedMult, e.Speed);
            Assert.Equal(hp, e.MaxHealth);
            Assert.Equal(e.MaxHealth, e.CurrentHealth);
            Assert.True(e.IsAlive);
        });
    }

    // ── Pool exhaustion ───────────────────────────────────────────────────────

    [Fact]
    public void Update_WhenPoolIsFull_DropsTheWaveInsteadOfGrowing()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();
        while (enemies.TryRent(out _)) { } // saturate all 1000 slots

        spawner.Update(1.5, enemies, 1.5, 100, 5);

        Assert.Equal(enemies.Capacity, enemies.CountAlive());
    }

    // ── Boss ──────────────────────────────────────────────────────────────────

    [Fact]
    public void SpawnBoss_HasBossStats_AndSpawnsOutsideTheField()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();

        Assert.True(spawner.SpawnBoss(enemies, currentEnemySpeed: 200));

        var boss = Assert.Single(Alive(enemies));
        Assert.Equal(EnemyType.Boss, boss.Type);
        Assert.Equal(35, boss.Radius);
        Assert.Equal(40, boss.MaxHealth);
        Assert.Equal(40, boss.CurrentHealth);
        Assert.Equal(120, boss.Speed); // 0.6 × current enemy speed
        Assert.Equal(500, boss.ScoreValue);
        Assert.True(
            boss.X < 0 || boss.X > GameBounds.Width || boss.Y < 0 || boss.Y > GameBounds.Height,
            $"boss spawned inside the field at ({boss.X:F1}, {boss.Y:F1})");
    }

    [Fact]
    public void SpawnBoss_WhenPoolIsFull_ReturnsFalse()
    {
        var spawner = new EnemySpawner();
        var enemies = new EnemyPool();
        while (enemies.TryRent(out _)) { }

        Assert.False(spawner.SpawnBoss(enemies, currentEnemySpeed: 200));
    }
}
