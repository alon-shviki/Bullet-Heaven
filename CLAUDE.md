# Bullet Heaven

Top-down survival shooter — Blazor WASM client, deployed via the portal.

## Commands

- **Full stack** (recommended): `cd ~/Desktop/game && docker compose up` → BH at http://localhost:8080
- **Client dev only** (no scores/auth): `cd BulletHeaven.Client && dotnet run` → http://localhost:5292

## Architecture

BH is a **client-only game** in the portal stack. There is no standalone BH API server in deployment.

- Auth: portal login sets `localStorage["jwt"]`; BH client reads it on startup
- Scores: BH nginx proxies `POST /api/scores` → `portal-auth:5001/api/scores/bullet-heaven`
- Leaderboard: BH nginx proxies `GET /api/leaderboard` → `portal-auth:5001/api/leaderboard/bullet-heaven`

See [[Notes/Tech/Backend]] and [[Notes/Tech/Architecture]] for full details.

## CI / Docker

- CI: `.github/workflows/docker.yml` — builds `bh-client` image on every push to `main`, pushes to `ghcr.io/alon-shviki/bh-client:latest`
- Portal compose pulls from GHCR — no local build required

## Key Files

| File | What it does |
|------|-------------|
| `BulletHeaven.Client/Pages/Game.razor` | State machine, all UI overlays, score submit |
| `BulletHeaven.Client/Pages/Game.Render.cs` | All canvas draw calls (partial class) |
| `nginx.conf` | Client nginx — proxies API paths to portal-auth |
| `.github/workflows/docker.yml` | CI: build + push to GHCR |

## Hard Rules

- `.obsidian/` is gitignored — never commit it.
- Do not add auth endpoints to BH. Login lives in the portal only.
- Do not add a scores/leaderboard DB to BH. Portal owns that data.
- `BulletHeaven.Server/` is legacy — it is not deployed and should not be extended.
