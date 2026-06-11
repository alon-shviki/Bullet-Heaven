using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Client.Game.Pools;

/// <summary>
/// Fixed-size pre-allocated entity pool (POOL-001/POOL-002).
/// Every slot is constructed once up-front; spawning activates an inactive
/// slot and killing flips <see cref="Entity.IsAlive"/> back off, so steady-state
/// gameplay performs zero heap allocations for pooled entity types.
/// Callers iterate <see cref="Slots"/> with a raw for-loop and must skip
/// slots where <c>IsAlive</c> is false — dead slots keep stale state.
/// </summary>
public class EntityPool<T> where T : Entity, new()
{
    private readonly T[] _slots;
    private int _cursor;

    public EntityPool(int capacity)
    {
        _slots = new T[capacity];
        for (var i = 0; i < capacity; i++)
            _slots[i] = new T { IsAlive = false };
    }

    public int Capacity => _slots.Length;

    public T[] Slots => _slots;

    /// <summary>
    /// Activates an inactive slot, scanning round-robin from the last rent
    /// point so repeated calls stay O(1) amortized. Returns false when every
    /// slot is alive — callers drop the spawn rather than allocating.
    /// </summary>
    public bool TryRent(out T item)
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            var idx = _cursor + i;
            if (idx >= _slots.Length) idx -= _slots.Length;
            if (!_slots[idx].IsAlive)
            {
                _cursor = idx + 1 == _slots.Length ? 0 : idx + 1;
                item = _slots[idx];
                item.IsAlive = true;
                return true;
            }
        }
        item = null!;
        return false;
    }

    /// <summary>Kills every slot. Used on game restart — not in per-frame code.</summary>
    public void DeactivateAll()
    {
        for (var i = 0; i < _slots.Length; i++)
            _slots[i].IsAlive = false;
        _cursor = 0;
    }

    /// <summary>O(capacity) count of live slots. For tests/debugging, not the tick path.</summary>
    public int CountAlive()
    {
        var count = 0;
        for (var i = 0; i < _slots.Length; i++)
            if (_slots[i].IsAlive) count++;
        return count;
    }
}
