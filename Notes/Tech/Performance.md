# Performance

## Frame Budget Rules

These are hard constraints — never break them in per-frame code paths:

| Rule | Why |
|------|-----|
| No `new` in the tick path | GC pauses cause visible frame drops |
| No LINQ in hot loops | Allocates enumerators; use raw `for` |
| One `BeginBatchAsync`…`EndBatchAsync` per frame | Each JS interop call is expensive |
| All movement scales by `dt` | Game speed is monitor-independent |
| Off-screen projectiles deactivated, not deleted | Deletion would thrash pool |

## Object Pools

### BulletPool (`Pools/BulletPool.cs`)
- 500 pre-allocated `Projectile` instances at startup
- `Get()` scans for first inactive slot, initialises fields, marks active
- On hit or off-screen: `Active = false` — returned to pool implicitly
- Zero `new Projectile()` calls during gameplay

### EnemyPool (`Pools/EnemyPool.cs`)
- 1000 pre-allocated `Enemy` instances at startup
- Same activate/deactivate pattern
- `EnemySpawner` calls `pool.Get()` on spawn interval

### EntityPool<T> (`Pools/EntityPool.cs`)
- Generic base used by both pools
- Fixed array; `Get()` is O(n) scan — acceptable at pool sizes ≤1000

## Quadtree Collision

**Class:** `Game/Collision/Quadtree.cs`

Every frame:
1. `_quadtree.Clear()` — O(n) reset
2. Rebuild: insert all active enemies — O(n log n)
3. Per projectile: `_quadtree.Query(x, y, radius, candidates)` fills a reused list — avoids allocation
4. Circle-check only the candidates from the query

Brings collision from O(N×M) brute force to O(N log N) average. Reused candidate list means zero per-frame allocation from the query.

## Profiling

Use Chrome DevTools → Performance tab:
- Watch for **GC sawtooth** in the memory graph (indicates heap alloc)
- Watch for **long scripting blocks** in heavy waves (indicates frame overrun)
- Target: <16 ms/frame at 60 fps, <8.3 ms at 120 fps

## Pending: Web Worker Physics

Tasks WORK-001 / WORK-002 — move entity movement, quadtree, and collision into `physicsWorker.js`.

Main thread becomes: receive coordinate payload → render only.

See [[Tasks]] for the design spec.
