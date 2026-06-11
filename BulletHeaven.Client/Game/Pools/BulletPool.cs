using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Client.Game.Pools;

/// <summary>Pre-allocated projectile pool (POOL-001): 500 bullets, zero gameplay allocations.</summary>
public sealed class BulletPool : EntityPool<Projectile>
{
    public const int Size = 500;

    public BulletPool() : base(Size) { }
}
