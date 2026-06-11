# Bullet Heaven

Blazor WASM (.NET 10) survival game + ASP.NET Core API. Canvas via `Blazor.Extensions.Canvas`; loop = `requestAnimationFrame` → JS interop → C#. No game engine.

## Commands
- Run game: `cd BulletHeaven.Client && dotnet watch` → http://localhost:5292 (HTTP context required — never open index.html from disk)
- Full stack: `docker compose up --build` → http://localhost:8080
- Unit tests: `dotnet test BulletHeaven.Tests` · E2E: `cd e2e && npx playwright test` (dev server must be running)

## Hard rules (always)
- `GameState` (private enum, `Game.razor`): `MainMenu | Playing | PausedLevelUp | GameOver | Codex | Login | Leaderboard` — loop skips updates unless `Playing`.
- No heap allocations / LINQ / per-entity JS interop in per-frame code; batch canvas calls once per frame.
- Pools, Quadtree, and Web Worker physics do **not** exist yet — pending tasks POOL/QUAD/WORK in `tasks.md`.
- Every `.cs`/`.razor` change goes through the agent pipeline before responding.

## Detailed rules — read the matching file before working on:
| Task touches | Read first |
|---|---|
| Any `.cs`/`.razor` change (mandatory pipeline) | `.claude/rules/pipeline.md` |
| Game loop, entities, rendering, per-frame code | `.claude/rules/performance.md` |
| Project layout, state machine, UI overlays, client↔server flow | `.claude/rules/architecture.md` |
| `BulletHeaven.Server` (controllers, auth, EF Core) | `.claude/rules/backend.md` |

## Skills — use for these workflows (don't improvise the procedure)
`add-upgrade` · `add-enemy` · `verify-game` (run + browser-check the game) · `db-migrate` (EF Core schema changes)

## Automated guardrails (hooks — don't fight them)
- Edits to `Migrations/`, `bin/`, `obj/`, `package-lock.json` are blocked (generated files).
- After any `.cs`/`.razor` edit, the QA pipeline is required and a **build gate** compiles the solution before you can finish — a broken build blocks completion.
