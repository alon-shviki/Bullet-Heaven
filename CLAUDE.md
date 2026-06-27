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
- Pools (×500 bullets, ×1000 enemies) and Quadtree collision are complete. Web Worker physics (WORK-001/002) is pending — see GitHub issues #4 and #5.
- Every `.cs`/`.razor` change goes through the agent pipeline before responding.
- Do **not** add auth endpoints to BH — login is portal-only.
- Do **not** add a scores/leaderboard DB to BH — portal owns that data.
- `BulletHeaven.Server/` is legacy and not deployed — do not extend it.

## Read Before Working

| Task touches | Read first |
|---|---|
| Any `.cs`/`.razor` change (mandatory pipeline) | `.claude/rules/pipeline.md` |
| Game loop, entities, rendering, per-frame code | `Notes/Tech/Performance.md` |
| Project layout, state machine, client↔portal flow | `Notes/Tech/Architecture.md` |
| CI, test suite, known gaps | `Notes/Tech/CI and Tests.md` |

## Skills — use for these workflows (don't improvise the procedure)
`add-upgrade` · `add-enemy` · `verify-game` (run + browser-check the game)

## Documentation Rule

**After completing any task or finding any problem: write or update a `.md` in `Notes/`.**

- Feature done → update or create in `Notes/Tech/` or `Notes/Design/`
- Bug or concern outside current task → open a GitHub issue AND note it in the relevant doc
- Use `[[Wiki Links]]` to connect related notes
- `Notes/` is visible to Obsidian — never put docs in hidden folders

## Workflow

This game is managed from the portal hub (`~/Desktop/game`). For full triage across all games, use the portal. Full workflow docs: portal's `Tech/Agentic Pipeline.md` and `Tech/Scripts.md`.

For BH issues (run from portal directory):
```bash
start-issue <number> bh
```

New tasks go as GitHub issues, not doc edits:
```bash
gh issue create --repo alon-shviki/Bullet-Heaven --title "..." --body "..." --label "enhancement,priority:medium"
```

Spot a bug outside the current task → open a GitHub issue immediately, continue with the task. Use `bug` · `question` · `enhancement` labels. Always set a priority.

Never commit directly to `main`.

## Automated guardrails (hooks — don't fight them)
- Edits to `Migrations/`, `bin/`, `obj/`, `package-lock.json` are blocked (generated files).
- After any `.cs`/`.razor` edit, the QA pipeline is required and a **build gate** compiles the solution before you can finish — a broken build blocks completion.
