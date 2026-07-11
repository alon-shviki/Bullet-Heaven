using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Client.Game.Entities;

public class PulseWeapon : ISecondaryWeapon
{
    private double _timer;
    public bool JustPulsed { get; private set; }

    public int Update(double dt, Player player, EntityPool<Enemy> enemies, WeaponStats stats)
    {
        JustPulsed = false;
        if (stats.PulseInterval <= 0) return 0;

        _timer -= dt;
        if (_timer > 0) return 0;

        _timer = stats.PulseInterval;
        JustPulsed = true;
        var kills = 0;

        var slots = enemies.Slots;
        for (var i = 0; i < slots.Length; i++)
        {
            var e = slots[i];
            if (!e.IsAlive) continue;
            if (GameMath.Distance(player, e) < stats.PulseRadius + e.Radius)
                if (e.TakeDamage(e.CurrentHealth)) kills++;
        }

        return kills;
    }

    public void Reset() { _timer = 0; JustPulsed = false; }
}
