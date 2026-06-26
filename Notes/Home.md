# Bullet Heaven — Vault Home

Top-down survival shooter. Blaze through endless enemy waves, level up, pick upgrades, and post your score.

## Quick Start

```bash
cd BulletHeaven.Client && dotnet run
# → http://localhost:5292

docker compose up --build
# → http://localhost:8080 (full stack with PostgreSQL)

dotnet test BulletHeaven.Tests
cd e2e && npx playwright test   # requires dev server running
```

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

### Work
- [[Tasks]] — pending tasks (WORK-001/002 remaining)
