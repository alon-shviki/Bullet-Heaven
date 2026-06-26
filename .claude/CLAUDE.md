# Bullet Heaven

Blazor WASM (.NET 10) survival game. Canvas via `Blazor.Extensions.Canvas`; loop = `requestAnimationFrame` → JS interop → C#. No game engine. **Client-only in production** — auth, scores, and leaderboard are owned by the portal auth server.

## Commands
- Run game (dev, no auth): `cd BulletHeaven.Client && dotnet watch` → http://localhost:5292 (HTTP context required — never open index.html from disk)
- Full stack (recommended): `cd ~/Desktop/game && docker compose up` → http://localhost:8080
- Unit tests: `dotnet test BulletHeaven.Tests` · E2E: `cd e2e && npx playwright test` (dev server must be running)

## Portal integration
- Auth: user logs in at the portal (`localhost:3000`); JWT passed via URL hash (`#portal_token=...`) on game launch and stored in `localStorage["jwt"]`
- Scores: `nginx.conf` proxies `POST /api/scores` → `portal-auth:5001/api/scores/bullet-heaven`
- Leaderboard: `nginx.conf` proxies `GET /api/leaderboard` → `portal-auth:5001/api/leaderboard/bullet-heaven`
- CI: `.github/workflows/docker.yml` builds and pushes `ghcr.io/alon-shviki/bh-client:latest` on every push to `main`

## Hard rules (always)
- `GameState` (private enum, `Game.razor`): `MainMenu | Playing | PausedLevelUp | GameOver | Codex | Leaderboard` — loop skips updates unless `Playing`.
- No heap allocations / LINQ / per-entity JS interop in per-frame code; batch canvas calls once per frame.
- Pools, Quadtree, and Web Worker physics do **not** exist yet — pending tasks POOL/QUAD/WORK in `tasks.md`.
- Every `.cs`/`.razor` change goes through the agent pipeline before responding.
- Do **not** add auth endpoints to BH — login is portal-only.
- Do **not** add a scores/leaderboard DB to BH — portal owns that data.
- `BulletHeaven.Server/` is legacy and not deployed — do not extend it.

## Detailed rules — read the matching file before working on:
| Task touches | Read first |
|---|---|
| Any `.cs`/`.razor` change (mandatory pipeline) | `.claude/rules/pipeline.md` |
| Game loop, entities, rendering, per-frame code | `.claude/rules/performance.md` |
| Project layout, state machine, UI overlays, client↔portal flow | `.claude/rules/architecture.md` |

## Skills — use for these workflows (don't improvise the procedure)
`add-upgrade` · `add-enemy` · `verify-game` (run + browser-check the game)

## Automated guardrails (hooks — don't fight them)
- Edits to `Migrations/`, `bin/`, `obj/`, `package-lock.json` are blocked (generated files).
- After any `.cs`/`.razor` edit, the QA pipeline is required and a **build gate** compiles the solution before you can finish — a broken build blocks completion.
