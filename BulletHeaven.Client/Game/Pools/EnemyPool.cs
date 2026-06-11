using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Client.Game.Pools;

/// <summary>Pre-allocated enemy pool (POOL-002): 1000 enemies, zero gameplay allocations.</summary>
public sealed class EnemyPool : EntityPool<Enemy>
{
    public const int Size = 1000;

    public EnemyPool() : base(Size) { }
}
