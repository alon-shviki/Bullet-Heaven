using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Client.Game.Entities;

public class Weapon
{
    private double _cooldown;

    public void Reset() => _cooldown = 0;

    public void Update(double dt, Player player, EntityPool<Enemy> enemies, EntityPool<Projectile> projectiles, WeaponStats stats)
    {
        _cooldown -= dt;
        if (_cooldown > 0) return;

        var nearest = NearestEnemy(player, enemies);
        if (nearest is null) return;

        var angle = Math.Atan2(nearest.Y - player.Y, nearest.X - player.X);
        for (var i = 0; i < stats.BulletCount; i++)
        {
            // pool exhausted (POOL-001) — drop the remaining shots rather than allocate
            if (!projectiles.TryRent(out var p)) break;
            var offset = (i - (stats.BulletCount - 1) / 2.0) * stats.SpreadAngle;
            var a = angle + offset;
            p.Activate(
                player.X, player.Y,
                Math.Cos(a), Math.Sin(a),
                stats.BulletSpeed, stats.BulletRadius,
                stats.Piercing, stats.RicochetCount);
        }

        _cooldown = stats.FireRate;
    }

    private static Enemy? NearestEnemy(Player player, EntityPool<Enemy> enemies)
    {
        Enemy? nearest = null;
        var minDist = double.MaxValue;
        var slots = enemies.Slots;
        for (var i = 0; i < slots.Length; i++)
        {
            var e = slots[i];
            if (!e.IsAlive) continue;
            var d = GameMath.Distance(player, e);
            if (d < minDist) { minDist = d; nearest = e; }
        }
        return nearest;
    }
}
