using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Client.Game.Entities;

public class EnemySpawner
{
    private double _timer;

    public void Update(double dt, EntityPool<Enemy> enemies, double spawnInterval, double enemySpeed, int count)
    {
        _timer += dt;
        if (_timer < spawnInterval) return;
        _timer -= spawnInterval;
        for (var i = 0; i < count; i++)
            if (!SpawnOne(enemies, enemySpeed)) return; // pool exhausted (POOL-002) — skip the rest of the wave
    }

    public void Reset() => _timer = 0;

    /// <summary>Activates a boss in the pool. Returns false when the pool is full.</summary>
    public bool SpawnBoss(EntityPool<Enemy> enemies, double currentEnemySpeed)
    {
        if (!enemies.TryRent(out var boss)) return false;
        var edge = Random.Shared.Next(4);
        var (x, y) = edge switch
        {
            0 => (Random.Shared.NextDouble() * GameBounds.Width, -40.0),
            1 => (GameBounds.Width + 40.0, Random.Shared.NextDouble() * GameBounds.Height),
            2 => (Random.Shared.NextDouble() * GameBounds.Width, GameBounds.Height + 40.0),
            _ => (-40.0, Random.Shared.NextDouble() * GameBounds.Height),
        };
        boss.Activate(x, y, EnemyType.Boss, radius: 35,
            speed: currentEnemySpeed * 0.6,
            maxHealth: 40, killValue: 20, xpValue: 30, scoreValue: 500);
        return true;
    }

    private static bool SpawnOne(EntityPool<Enemy> enemies, double baseSpeed)
    {
        if (!enemies.TryRent(out var enemy)) return false;

        var type = PickType();
        var (radius, speedMult, hp, killVal, xpVal, scoreVal) = type switch
        {
            EnemyType.Runner => (8.0, 1.5, 1, 1, 1, 15),
            EnemyType.Tank => (20.0, 0.5, 3, 2, 3, 25),
            EnemyType.Elite => (16.0, 1.2, 5, 5, 8, 80),
            _ => (12.0, 1.0, 1, 1, 1, 10),
        };

        var edge = Random.Shared.Next(4);
        var (x, y) = edge switch
        {
            0 => (Random.Shared.NextDouble() * GameBounds.Width, -20.0),
            1 => (GameBounds.Width + 20, Random.Shared.NextDouble() * GameBounds.Height),
            2 => (Random.Shared.NextDouble() * GameBounds.Width, GameBounds.Height + 20),
            _ => (-20.0, Random.Shared.NextDouble() * GameBounds.Height),
        };

        enemy.Activate(x, y, type, radius, baseSpeed * speedMult, hp, killVal, xpVal, scoreVal);
        return true;
    }

    private static EnemyType PickType()
    {
        var roll = Random.Shared.Next(100);
        return roll switch
        {
            < 3 => EnemyType.Elite,
            < 15 => EnemyType.Tank,
            < 40 => EnemyType.Runner,
            _ => EnemyType.Standard,
        };
    }
}
