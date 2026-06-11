using BulletHeaven.Client.Game.Entities;
using BulletHeaven.Client.Game.Pools;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="EntityPool{T}"/> (POOL-001/POOL-002):
/// up-front slot pre-allocation, TryRent activation + round-robin cursor,
/// drop-on-exhaustion, DeactivateAll, CountAlive, and the fixed sizes of
/// <see cref="BulletPool"/> (500) and <see cref="EnemyPool"/> (1000).
/// Small-capacity pools of <see cref="Projectile"/> are used so slot-level
/// behaviour is observable without scanning hundreds of entries.
/// </summary>
public class EntityPoolTests
{
    // ── Construction ──────────────────────────────────────────────────────────

    // Happy path: every slot is constructed up-front and starts dead.
    [Fact]
    public void Constructor_PreallocatesAllSlotsDead()
    {
        var pool = new EntityPool<Projectile>(4);

        Assert.Equal(4, pool.Slots.Length);
        Assert.All(pool.Slots, s =>
        {
            Assert.NotNull(s);
            Assert.False(s.IsAlive);
        });
        Assert.Equal(0, pool.CountAlive());
    }

    // Happy path: Capacity reflects the constructor argument.
    [Fact]
    public void Capacity_MatchesConstructorArgument()
    {
        Assert.Equal(7, new EntityPool<Projectile>(7).Capacity);
    }

    // ── TryRent: activation ───────────────────────────────────────────────────

    // Happy path: renting from a fresh pool activates a slot and returns it.
    [Fact]
    public void TryRent_OnFreshPool_ReturnsFirstSlotAlive()
    {
        var pool = new EntityPool<Projectile>(3);

        Assert.True(pool.TryRent(out var item));

        Assert.Same(pool.Slots[0], item);
        Assert.True(item.IsAlive);
        Assert.Equal(1, pool.CountAlive());
    }

    // Happy path: consecutive rents hand out distinct slots, never a live one twice.
    [Fact]
    public void TryRent_ConsecutiveRents_ReturnDistinctSlots()
    {
        var pool = new EntityPool<Projectile>(3);

        Assert.True(pool.TryRent(out var a));
        Assert.True(pool.TryRent(out var b));
        Assert.True(pool.TryRent(out var c));

        Assert.NotSame(a, b);
        Assert.NotSame(b, c);
        Assert.NotSame(a, c);
        Assert.Equal(3, pool.CountAlive());
    }

    // ── TryRent: round-robin cursor ───────────────────────────────────────────

    // Round-robin: the scan continues from the last rent point instead of
    // re-checking earlier slots, so a freed earlier slot is not returned
    // while a later untouched slot is still free.
    [Fact]
    public void TryRent_ContinuesFromCursor_NotFromFreedEarlierSlot()
    {
        var pool = new EntityPool<Projectile>(3);
        pool.TryRent(out var first);  // slot 0
        pool.TryRent(out _);          // slot 1
        first.IsAlive = false;        // free slot 0

        Assert.True(pool.TryRent(out var third));

        Assert.Same(pool.Slots[2], third); // cursor was at 2 — slot 0 is skipped for now
    }

    // Round-robin: after the cursor passes the end it wraps and reuses the
    // freed slot — same instance, no allocation.
    [Fact]
    public void TryRent_WrapsAround_ReusesFreedSlotInstance()
    {
        var pool = new EntityPool<Projectile>(3);
        pool.TryRent(out var first);  // slot 0
        pool.TryRent(out _);          // slot 1
        first.IsAlive = false;
        pool.TryRent(out _);          // slot 2, cursor wraps to 0

        Assert.True(pool.TryRent(out var reused));

        Assert.Same(first, reused);
        Assert.True(reused.IsAlive);
    }

    // Round-robin: a live slot is never re-returned even when the cursor
    // points at it — the scan walks past live slots to the next free one.
    [Fact]
    public void TryRent_CursorOnLiveSlot_SkipsToNextFreeSlot()
    {
        var pool = new EntityPool<Projectile>(3);
        pool.TryRent(out _);          // slot 0, cursor 1
        pool.TryRent(out _);          // slot 1, cursor 2
        pool.TryRent(out _);          // slot 2, cursor 0 — all alive
        pool.Slots[1].IsAlive = false;

        Assert.True(pool.TryRent(out var item)); // cursor at 0 (alive) → must skip to 1

        Assert.Same(pool.Slots[1], item);
    }

    // ── TryRent: exhaustion ───────────────────────────────────────────────────

    // Expected error: a full pool returns false (drop-on-exhaustion, no growth).
    [Fact]
    public void TryRent_WhenAllSlotsAlive_ReturnsFalse()
    {
        var pool = new EntityPool<Projectile>(2);
        pool.TryRent(out _);
        pool.TryRent(out _);

        Assert.False(pool.TryRent(out _));
        Assert.Equal(2, pool.CountAlive()); // nothing new was activated
    }

    // Boundary: a full pool with exactly one freed slot rents exactly that slot.
    [Fact]
    public void TryRent_OneFreeSlotInFullPool_FindsIt()
    {
        var pool = new EntityPool<Projectile>(4);
        while (pool.TryRent(out _)) { }
        pool.Slots[2].IsAlive = false;

        Assert.True(pool.TryRent(out var item));

        Assert.Same(pool.Slots[2], item);
        Assert.False(pool.TryRent(out _)); // full again
    }

    // ── DeactivateAll ─────────────────────────────────────────────────────────

    // Happy path: restart kills every slot.
    [Fact]
    public void DeactivateAll_KillsEverySlot()
    {
        var pool = new EntityPool<Projectile>(5);
        while (pool.TryRent(out _)) { }

        pool.DeactivateAll();

        Assert.Equal(0, pool.CountAlive());
        Assert.All(pool.Slots, s => Assert.False(s.IsAlive));
    }

    // DeactivateAll also resets the rent cursor — the next rent starts at slot 0.
    [Fact]
    public void DeactivateAll_ResetsCursorToSlotZero()
    {
        var pool = new EntityPool<Projectile>(3);
        pool.TryRent(out _);
        pool.TryRent(out _); // cursor now at 2

        pool.DeactivateAll();
        Assert.True(pool.TryRent(out var item));

        Assert.Same(pool.Slots[0], item);
    }

    // ── CountAlive ────────────────────────────────────────────────────────────

    // CountAlive tracks rents and kills exactly.
    [Fact]
    public void CountAlive_TracksRentsAndKills()
    {
        var pool = new EntityPool<Projectile>(4);
        pool.TryRent(out var a);
        pool.TryRent(out _);
        pool.TryRent(out _);
        Assert.Equal(3, pool.CountAlive());

        a.IsAlive = false; // kill = flip the flag

        Assert.Equal(2, pool.CountAlive());
    }

    // ── Concrete pool sizes ───────────────────────────────────────────────────

    // POOL-001: the bullet pool is fixed at 500 slots.
    [Fact]
    public void BulletPool_HasFixedCapacity500()
    {
        var pool = new BulletPool();

        Assert.Equal(500, BulletPool.Size);
        Assert.Equal(500, pool.Capacity);
        Assert.Equal(500, pool.Slots.Length);
    }

    // POOL-002: the enemy pool is fixed at 1000 slots.
    [Fact]
    public void EnemyPool_HasFixedCapacity1000()
    {
        var pool = new EnemyPool();

        Assert.Equal(1000, EnemyPool.Size);
        Assert.Equal(1000, pool.Capacity);
        Assert.Equal(1000, pool.Slots.Length);
    }
}
