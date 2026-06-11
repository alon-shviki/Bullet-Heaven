# Bullet Heaven

A browser-based survival game built with **Blazor WebAssembly** and **.NET 10**. Fight endless waves of enemies, collect XP gems, level up, and climb the global leaderboard — all without leaving the browser tab.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Blazor WASM (.NET 10) |
| Rendering | HTML5 Canvas via `Blazor.Extensions.Canvas` |
| Game Loop | `requestAnimationFrame` → JS interop → C# callback |
| Physics | JS Web Worker (off main thread) |
| Backend | ASP.NET Core Minimal API |
| Database | PostgreSQL 17 (EF Core) |
| Auth | JWT Bearer tokens |
| Tests | xUnit (C#) · Playwright (E2E) |
| Deployment | Docker Compose + nginx |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node.js 20+ (for E2E tests)
- Docker + Docker Compose (optional, for full stack)

### Run the client (game only)

```bash
cd BulletHeaven.Client
dotnet watch
```

Open `http://localhost:5292` — **do not open `index.html` directly**, the game requires an HTTP context.

### Run the full stack (game + API + database)

```bash
docker compose up --build
```

Visit `http://localhost:8080`.

### Run tests

```bash
# Unit tests
dotnet test BulletHeaven.Tests

# E2E tests (requires the dev server to be running)
cd e2e && npm install && npx playwright test
```

## Project Structure

```
Bullet-Heaven/
├── BulletHeaven.Client/       # Blazor WASM game
│   ├── Game/                  # Core game logic (loop, physics, pools, math)
│   ├── Pages/                 # Game.razor + Game.Render.cs
│   └── wwwroot/               # index.html, JS interop, assets
├── BulletHeaven.Server/       # ASP.NET Core API (auth, leaderboard)
├── BulletHeaven.Tests/        # xUnit unit tests
├── e2e/                       # Playwright browser tests
├── docker-compose.yml
└── nginx.conf
```

## Gameplay

- **Move** — WASD or Arrow Keys
- **Space** — pause / resume
- **R** — restart after game over
- **Weapons fire automatically** toward the nearest enemy
- **Collect XP gems** dropped by defeated enemies; gems magnetically pull toward the player when in range
- **Level up** (via kills or XP) to choose one of three weighted-random upgrades
- **Boss** spawns every 2 minutes — killing it grants 2 bonus upgrade picks
- Survive as long as possible; high scores are posted to the global leaderboard if you're signed in

### Enemy types

| Enemy | HP | Speed | Score |
|-------|----|-------|-------|
| Standard | 1 | normal | 10 |
| Runner | 1 | 1.5× | 15 |
| Tank | 3 | 0.5× | 25 |
| Elite | 5 | 1.2× | 80 |
| Boss | 40 | 0.6× | 500 |

### Weapons & upgrades

The primary weapon fires automatically and supports split shot (2–3 bullets), piercing, homing, ricochet, and explosive kills. Three secondary weapons are unlockable via upgrades: **Orb Shield** (orbiting contact-damage orbs), **Pulse Nova** (area explosion every 3 s), and **Damage Aura** (passive damage field). A full weighted-rarity upgrade catalogue (Common / Rare / Epic) is browsable in the in-game Archive.

## Architecture Notes

- **Batched canvas calls** — all draw operations are collected and flushed once per frame via `BeginBatchAsync` / `EndBatchAsync`; no per-entity JS interop round trips.
- **Game state** — `GameState` enum: `MainMenu` | `Playing` | `PausedLevelUp` | `GameOver` | `Login` | `Leaderboard` | `Codex`. The RAF loop skips entity updates in any non-Playing state.
- **Combo multiplier** — kill streaks within 3 s stack a combo (1.5× at 5, 2× at 10, 3× at 20) that multiplies score.
- **Screen shake** — triggered on player damage; decays over 0.25 s via canvas translate jitter.

> **Pending performance work:** `BulletPool` / `EnemyPool` object pooling, Quadtree spatial partitioning, and Web Worker physics offload are planned but not yet implemented. The game currently uses `List<T>` with O(N²) collision.

## Roadmap

| ID | Feature | Status |
|----|---------|--------|
| ENGINE-001 | Engine & stack decision | ✅ Done |
| SETUP-001 | Project scaffold + canvas baseline | ✅ Done |
| LOOP-001 | RAF game loop with delta time + FPS counter | ✅ Done |
| INPUT-001 | Keyboard input → normalized movement vector | ✅ Done |
| RENDER-001 | Player entity + boundary clamping | ✅ Done |
| ENEMY-001 | Enemy factory + tracking AI (5 types incl. Boss) | ✅ Done |
| WEAPON-001 | Auto-targeting projectile weapon | ✅ Done |
| COLLISION-001 | Circle-vs-circle collision detection | ✅ Done |
| STATS-001 | Health, damage, i-frames | ✅ Done |
| XP-001 | XP gems + magnet behavior | ✅ Done |
| LEVEL-001 | State manager + level-up pause | ✅ Done |
| UI-001 | Upgrade selection overlay (weighted-rarity catalogue) | ✅ Done |
| BACK-001 | ASP.NET Core API + PostgreSQL + EF Core | ✅ Done |
| AUTH-001 | Registration + login JWT API | ✅ Done |
| AUTH-002 | Frontend auth UI (sign in / create account) | ✅ Done |
| LEADER-001 | Save score API (JWT-protected) | ✅ Done |
| LEADER-002 | Leaderboard panel UI | ✅ Done |
| POOL-001 | BulletPool object pooling | Pending |
| POOL-002 | EnemyPool object pooling | Pending |
| QUAD-001 | Quadtree spatial partitioning | Pending |
| QUAD-002 | Quadtree integrated into collision | Pending |
| WORK-001 | Web Worker setup | Pending |
| WORK-002 | Physics delegated to Web Worker | Pending |

## Contributing

1. Fork the repo and create a feature branch.
2. Run `dotnet test` — all tests must pass.
3. For `.cs` / `.razor` changes: ensure no heap allocations inside the game loop and no per-entity JS interop calls.
4. Open a pull request with a clear description of the change.


