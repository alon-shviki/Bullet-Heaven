using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Client.Game.Entities;

public class AuraWeapon : ISecondaryWeapon
{
    private double _timer;
    private const double TickInterval = 0.5;

    public int Update(double dt, Player player, EntityPool<Enemy> enemies, WeaponStats stats)
    {
        if (stats.AuraRadius <= 0) return 0;

        _timer -= dt;
        if (_timer > 0) return 0;

        _timer = TickInterval;
        var kills = 0;

        var slots = enemies.Slots;
        for (var i = 0; i < slots.Length; i++)
        {
            var e = slots[i];
            if (!e.IsAlive) continue;
            if (GameMath.Distance(player, e) < stats.AuraRadius + e.Radius)
                if (e.TakeDamage()) kills++;
        }

        return kills;
    }

    public void Reset() => _timer = 0;
}
