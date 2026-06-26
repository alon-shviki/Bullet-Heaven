# Backend

Bullet Heaven has **no standalone API server in production**. Auth, scores, and leaderboard are all owned by the portal auth server. The game client's nginx proxies the relevant paths there.

## How it works

```
BH client (Blazor WASM, port 8080)
  → POST /api/scores          → nginx → portal-auth:5001/api/scores/bullet-heaven
  → GET  /api/scores/me       → nginx → portal-auth:5001/api/leaderboard/bullet-heaven/me
  → GET  /api/leaderboard     → nginx → portal-auth:5001/api/leaderboard/bullet-heaven
```

Auth token: read from `localStorage["jwt"]` — set by the portal when the player logs in. One login covers the portal + all games.

## Portal Auth Server Endpoints Used

| Method | Path (via portal) | Auth | Description |
|--------|-------------------|------|-------------|
| POST | `/api/scores/bullet-heaven` | Bearer | Submit a run score |
| GET | `/api/leaderboard/bullet-heaven` | — | Top 10 |
| GET | `/api/leaderboard/bullet-heaven/me` | Bearer | Player's top 5 runs |

Score payload: `{ value, kills, level }`.

## Legacy — BulletHeaven.Server

The server project (`BulletHeaven.Server/`) still exists in the repo with its own auth and scores controllers. It is **not deployed** — `docker-compose.yml` no longer includes a `bh-api` service. It can be deleted or repurposed later.

## Running the full stack

```bash
# From the portal repo
cd ~/Desktop/game && docker compose up
# Portal → localhost:3000  |  BH → localhost:8080
```
