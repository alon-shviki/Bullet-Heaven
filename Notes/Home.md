# Bullet Heaven — Vault Home

Top-down survival shooter. Blaze through endless enemy waves, level up, pick upgrades, and post your score.

## Quick Start

```bash
# Full portal stack (recommended) — from the portal repo
cd ~/Desktop/game && docker compose up
# Portal → http://localhost:3000  |  BH → http://localhost:8080

# BH client only (Blazor WASM dev server, no scores/auth)
cd BulletHeaven.Client && dotnet run   # → http://localhost:5292
```

Auth and scores live in the portal. See [[Tech/Backend]].

## Vault Map

### Design
- [[Design/Core Loop]] — game states, tick, XP, leveling
- [[Design/Entities]] — Player, Enemy types and stats
- [[Design/Weapons & Upgrades]] — all weapons, full upgrade catalogue
- [[Design/Difficulty]] — wave scaling, difficulty curve

### Tech
- [[Tech/Architecture]] — project layout, file map, client↔server
- [[Tech/Performance]] — frame budget rules, pools, quadtree
- [[Tech/Backend]] — API endpoints, auth, database schema
- [[Tech/CI and Tests]] — CI pipeline, test suite coverage, known gaps

### Work
- [[Tasks]] — pending tasks (WORK-001/002 remaining)
