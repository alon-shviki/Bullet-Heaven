# Backend Rules

**Bullet Heaven has no backend.** The portal auth server (`~/Desktop/game/portal-auth/`) owns auth, scores, and the leaderboard; BH's nginx proxies `/api/scores` and `/api/leaderboard` to it (see `nginx.conf`).

- Never add auth endpoints, controllers, or a DB to this repo.
- Score/leaderboard changes happen in the portal repo, not here.
- `BulletHeaven.Server/` was deleted (July 2026) — it was a legacy duplicate of portal-auth.
