# Performance Rules — game-loop / per-frame code

Applies to: `GameLoop`, `Game.Render.cs`, everything in `Game/Entities/`, and any code executed per tick.

## Always
- **No heap allocations per frame.** No `new`, no closures, no string concatenation, no boxing inside the tick path. Reuse fields/buffers.
- **No LINQ in hot loops.** Iterate raw `for` over arrays/lists.
- **Batch all canvas calls.** One `BeginBatchAsync` … `EndBatchAsync` per frame; never a JS interop round trip per entity.
- **Delta time everywhere.** All movement/timers scale by `dt` — never assume 60 fps.
- Culling: off-screen projectiles must be deactivated, not leaked.

## Current reality (do not assume otherwise)
- Collision is brute-force O(N²) circle-vs-circle over `List<T>`.
- There is **no** BulletPool, EnemyPool, Quadtree, or Web Worker yet — these are pending tasks POOL-001/002, QUAD-001/002, WORK-001/002 in `tasks.md`.

## When implementing the pending perf tasks
- **Pools:** fixed-size pre-allocated arrays (≈500 bullets, ≈1000 enemies); spawn = activate an inactive slot, kill = flip `Active` flag. Zero `new` during gameplay.
- **Quadtree:** `Clear()` + rebuild from scratch every frame from active enemies; query per projectile, then circle-check only the candidates.
- **Web Worker:** physics + quadtree run in `physicsWorker.js`; main thread sends input/timing via `postMessage` and only renders the returned coordinate payload.

## Verifying
Profile with Chrome DevTools (Performance tab) — watch for GC sawtooth in the memory graph and long scripting blocks during heavy waves.
