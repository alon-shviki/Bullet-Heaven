using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Client.Game.Entities;

public class OrbWeapon : ISecondaryWeapon
{
    public const int MaxOrbs = 16;
    private const double HitCooldown = 0.4;

    private double _angle;
    private readonly (double X, double Y)[] _positions = new (double, double)[MaxOrbs];

    /// <summary>Number of valid entries in the orb position buffer this frame.</summary>
    public int ActiveOrbCount { get; private set; }

    public (double X, double Y) GetOrbPosition(int index) => _positions[index];

    public int Update(double dt, Player player, EntityPool<Enemy> enemies, WeaponStats stats)
    {
        var orbCount = Math.Min(stats.OrbCount, MaxOrbs);
        if (orbCount == 0) { ActiveOrbCount = 0; return 0; }

        _angle += dt * Math.PI;

        var slots = enemies.Slots;

        // per-enemy hit cooldown lives on the pooled Enemy itself — no dictionary churn
        for (var i = 0; i < slots.Length; i++)
        {
            var e = slots[i];
            if (e.IsAlive && e.OrbHitCooldown > 0) e.OrbHitCooldown -= dt;
        }

        var kills = 0;

        for (var i = 0; i < orbCount; i++)
        {
            var a  = _angle + 2 * Math.PI * i / orbCount;
            var ox = player.X + Math.Cos(a) * stats.OrbOrbitRadius;
            var oy = player.Y + Math.Sin(a) * stats.OrbOrbitRadius;
            _positions[i] = (ox, oy);

            for (var j = 0; j < slots.Length; j++)
            {
                var e = slots[j];
                if (!e.IsAlive || e.OrbHitCooldown > 0) continue;
                if (GameMath.Distance(ox, oy, e.X, e.Y) < e.Radius + 8)
                {
                    e.OrbHitCooldown = HitCooldown;
                    if (e.TakeDamage()) kills++;
                }
            }
        }

        ActiveOrbCount = orbCount;
        return kills;
    }

    public void Reset() { _angle = 0; ActiveOrbCount = 0; }
}
