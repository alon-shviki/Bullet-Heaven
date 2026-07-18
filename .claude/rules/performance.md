# Performance Rules — game-loop / per-frame code

Applies to: `GameLoop`, `Game.Render.cs`, everything in `Game/Entities/`, and any code executed per tick.

## Always
- **No heap allocations per frame.** No `new`, no closures, no string concatenation, no boxing inside the tick path. Reuse fields/buffers.
- **No LINQ in hot loops.** Iterate raw `for` over arrays/lists.
- **Batch all canvas calls.** One `BeginBatchAsync` … `EndBatchAsync` per frame; never a JS interop round trip per entity.
- **Delta time everywhere.** All movement/timers scale by `dt` — never assume 60 fps.
- Culling: off-screen projectiles must be deactivated, not leaked.

## Current reality
- Collision runs through `Quadtree` (`Game/Collision/Quadtree.cs`) — cleared and rebuilt from active enemies every frame, queried per projectile so only nearby candidates get the circle check. Not brute-force O(N²).
- `BulletPool` (500) and `EnemyPool` (1000) are pre-allocated; spawn activates an inactive slot, kill flips `Active` — zero `new` during gameplay.
- A Web Worker scaffold exists (`wwwroot/js/physicsWorker.js`, handshake message only), but physics and collision still run on the main thread. Delegating the actual update/collision step to the worker is the one remaining perf task — WORK-002, issue #5.

## When implementing WORK-002 (Web Worker physics)
- Physics + Quadtree move into `physicsWorker.js`; main thread sends input/timing via `postMessage` and only renders the returned coordinate payload.

## Verifying
Profile with Chrome DevTools (Performance tab) — watch for GC sawtooth in the memory graph and long scripting blocks during heavy waves.
