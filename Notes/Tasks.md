# Tasks — Bullet Heaven

See `tasks.md` in repo root for full task specs.

## Status Summary

| ID | Task | Status |
|----|------|--------|
| ENGINE-001 | Engine & stack decision | ✅ Done |
| SETUP-001 | Directory structure + canvas baseline | ✅ Done |
| LOOP-001 | RAF game loop with delta time | ✅ Done |
| INPUT-001 | Keyboard input + movement vector | ✅ Done |
| RENDER-001 | Player entity + boundary clamp | ✅ Done |
| ENEMY-001 | Enemy factory + tracking AI | ✅ Done |
| WEAPON-001 | Auto-targeting projectile weapon | ✅ Done |
| COLLISION-001 | Circle-vs-circle collision | ✅ Done |
| STATS-001 | Player health, i-frames | ✅ Done |
| XP-001 | XP gems, magnet behavior | ✅ Done |
| LEVEL-001 | Centralized state + level-up trigger | ✅ Done |
| UI-001 | Upgrade selection overlay | ✅ Done |
| POOL-001 | Bullet object pool (×500) | ✅ Done |
| POOL-002 | Enemy object pool (×1000) | ✅ Done |
| QUAD-001 | Quadtree class | ✅ Done |
| QUAD-002 | Quadtree integrated into collision | ✅ Done |
| BACK-001 | ASP.NET Core API skeleton | ✅ Done |
| AUTH-001 | Register/login JWT API | ✅ Done |
| AUTH-002 | Login/register UI overlay | ✅ Done |
| LEADER-001 | Save score API | ✅ Done |
| LEADER-002 | Leaderboard UI panel | ✅ Done |
| **WORK-001** | **Web Worker setup** | ⏳ Pending |
| **WORK-002** | **Delegate physics to Web Worker** | ⏳ Pending |

## WORK-001 / WORK-002 — Web Worker Physics

Goal: move all entity physics + quadtree into `physicsWorker.js` so the main thread only renders.

**Design:**
1. Create `wwwroot/js/physicsWorker.js`
2. Main thread sends `{ type: 'TICK', dt, input, playerPos }` via `postMessage`
3. Worker runs enemy tracking, projectile movement, quadtree collision
4. Worker replies with `{ type: 'TICK_REPLY', entities: [...] }`
5. Main thread renders the returned coordinate payload — no physics math

**Constraint:** Blazor WASM can't use `new Worker()` directly from C#; must go through `gameInterop.js` and expose `window.postToWorker`.
