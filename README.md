# Bullet Heaven

A browser-based survival game built with **Blazor WebAssembly** and **.NET 10**. Fight endless waves of enemies, collect XP gems, level up, and climb the global leaderboard — all without leaving the browser tab.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Blazor WASM (.NET 10) |
| Rendering | HTML5 Canvas via `Blazor.Extensions.Canvas` |
| Game Loop | `requestAnimationFrame` → JS interop → C# callback |
| Collision | Quadtree spatial partitioning over pooled entities |
| Auth / Scores / Leaderboard | Portal auth server — **no game-specific backend or database** |
| Tests | xUnit (C#) · Playwright (E2E) |
| Deployment | Docker Compose + nginx |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node.js 20+ (for E2E tests)
- Docker + Docker Compose (optional, for full stack)

### Run the client (game only, no portal auth)

```bash
cd BulletHeaven.Client
dotnet watch
```

Open `http://localhost:5292` — **do not open `index.html` directly**, the game requires an HTTP context.

### Run the full stack (game + portal auth + leaderboard)

```bash
cd ~/Desktop/game && docker compose up --build
```

Visit `http://localhost:3000/bh/` (dev-only direct port: `http://localhost:8080`).

### Run tests

```bash
# Unit tests
dotnet test BulletHeaven.Tests

# E2E tests (requires the dev server running; not yet wired into CI)
cd e2e && npm install && npx playwright test
```

## Project Structure

```
Bullet-Heaven/
├── BulletHeaven.Client/        # Blazor WASM game — the entire codebase (client-only)
│   ├── Game/
│   │   ├── Entities/           # Player, Enemy, EnemySpawner, Projectile, Weapon + secondaries, XpGem, Particle
│   │   ├── Pools/              # BulletPool (500), EnemyPool (1000) — zero per-frame allocations
│   │   ├── Collision/          # Quadtree spatial partitioning
│   │   ├── Upgrades/           # UpgradeCatalogue (weighted Common/Rare/Epic), UpgradeDefinition
│   │   ├── Input/               # InputHandler — keyboard state → normalized movement vector
│   │   ├── GameLoop.cs · GameMath.cs · GameBounds.cs · DifficultyManager.cs · WeaponStats.cs
│   ├── Pages/
│   │   ├── Game.razor          # All UI overlays + GameState machine + orchestration
│   │   └── Game.Render.cs      # All canvas drawing (partial class of Game.razor)
│   └── wwwroot/js/gameInterop.js  # RAF bridge + key listeners — the only JS file
├── BulletHeaven.Tests/          # xUnit unit tests
├── e2e/                         # Playwright browser tests
├── docker-compose.yml
└── nginx.conf                   # Proxies /api/scores, /api/leaderboard to portal-auth
```

There is no server project in this repo — `BulletHeaven.Server` was removed; the portal auth server owns all backend concerns (auth, scores, leaderboard, database).

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

- **Client-only.** No backend, no database, no `/register` or `/login` — all owned by the [portal](https://github.com/alon-shviki/game-portal)'s auth server. JWT arrives via `#portal_token=` URL hash on launch from the portal and is stored in `localStorage["jwt"]`.
- `nginx.conf` proxies `POST /api/scores` and `GET /api/leaderboard` to `portal-auth:5001/api/{scores,leaderboard}/bullet-heaven`.
- **Batched canvas calls** — all draw operations are collected and flushed once per frame via `BeginBatchAsync` / `EndBatchAsync`; no per-entity JS interop round trips.
- **Game state** — `GameState` enum (private, in `Game.razor`): `MainMenu | Playing | PausedLevelUp | GameOver | Login | Leaderboard | Codex`. The RAF loop skips entity updates in any non-Playing state.
- **Pooling + Quadtree** — `BulletPool` (500) / `EnemyPool` (1000) eliminate per-frame allocations; collision runs through a Quadtree rebuilt each frame, not brute-force O(N²).
- **Combo multiplier** — kill streaks within 3 s stack a combo (1.5× at 5, 2× at 10, 3× at 20) that multiplies score.
- **Screen shake** — triggered on player damage; decays over 0.25 s via canvas translate jitter.

> **Pending:** Web Worker physics offload (moving the update/collision step off the main thread) is planned but not yet implemented — see GitHub issues #4/#5.

## CI / Deployment

`.github/workflows/ci.yml` is a thin caller of the portal's shared reusable workflow (`alon-shviki/game-portal/.github/workflows/dotnet-ci.yml@main`): cache NuGet → `dotnet format --verify-no-changes` → build → test, and on push to `main`, pushes `ghcr.io/alon-shviki/bh-client:latest` (+ sha tag). Required check: `ci / build`. E2E (Playwright) tests exist but aren't wired into CI yet.

## Claude Code Setup

Developed primarily with [Claude Code](https://docs.claude.com/en/docs/claude-code), combining the portal's shared pipeline with a pipeline of its own for game code.

**`CLAUDE.md`** carries the stack commands, the portal integration contract, the hard rules (client-only — no auth or DB in this repo), and the Game Context Rule pointer that `start-issue` prints when work begins here.

**`.claude/rules/`**:

- `architecture.md` — the codebase map, the `GameState` machine, and the client↔portal data flow
- `backend.md` — why there's no backend in this repo (the portal owns it)
- `performance.md` — the per-frame rules (no heap allocations, no LINQ, no per-entity JS interop) and the current performance reality
- `pipeline.md` — the agent pipeline below, and when each step runs

**`.claude/agents/`** is a real 4-step pipeline that runs on every `.cs`/`.razor` change:

- `qa-reviewer` — reviews drafted code against the original request line by line; fails on any concrete issue (logic bugs, security flaws, performance traps) before anything is shown to the user
- `test-generator` — maintains `BulletHeaven.Tests`, appending tests for new behavior and regression-running the whole suite
- `docs-generator` — writes the documentation block that becomes the final response
- `playwright-e2e` — runs browser-level UI-flow tests against the live dev server, but only when a `.razor` file or `Game.Render.cs` changed

This repo's game-loop code is intricate enough that isolated review/test/doc passes earn their keep — `.claude/rules/pipeline.md` has the full sequencing.

**`.claude/skills/`**:

- `add-enemy` — scaffolds a new enemy type, including the codex card and preview
- `add-upgrade` — scaffolds a new upgrade catalogue entry, including stat wiring and weight conventions
- `verify-game` — builds, runs, and visually verifies the game in a real browser after gameplay/UI changes
- `playwright-cli` — drives browser automation for the Playwright test suite
- `ci-cd` — the CI/CD mental model shared across the three repos
- `obsidian-vault` — finds, creates, and organizes notes in this repo's vault

**`.claude/settings.json` hooks**: `PreToolUse` blocks hand-edits to generated files (`Migrations/`, `bin/`, `obj/`, `package-lock.json`); `PostToolUse` flags any `.cs`/`.razor` edit as requiring the agent pipeline above; a `Stop` hook recompiles the whole solution and blocks the session from finishing on a broken build.

## Obsidian

`Notes/` is this repo's own vault. `Home.md` is the quick-start dashboard, `Tasks.md` tracks the build-out task list, `Design/` holds the game-design notes (`Core Loop.md`, `Entities.md`, `Difficulty.md`, `Weapons & Upgrades.md`), and `Tech/` holds the engineering notes (`Architecture.md`, `Performance.md`, `Backend.md`, `CI and Tests.md`, `Web Worker.md`).

It's symlinked into the portal vault at `~/Desktop/game/Games/Bullet-Heaven`, so the same notes are browsable and editable from either vault — nothing is duplicated between them.

## Contributing

1. Work happens in a worktree via the portal's agentic scripts — never commit directly to `main`:
   ```bash
   bash ~/Desktop/game/.claude/scripts/start-issue <number>   # auto-detects this repo
   bash ~/Desktop/game/.claude/scripts/finish-issue           # tests → push → PR → wait for CI → merge
   ```
2. `dotnet test` — all tests must pass; `dotnet format` before pushing (CI enforces it).
3. For `.cs` / `.razor` changes: no heap allocations or LINQ inside the game loop, no per-entity JS interop calls — batch canvas calls once per frame.
4. Open a pull request with a clear description of the change.

See `CLAUDE.md` and `.claude/rules/` for the full contract and hard rules.
