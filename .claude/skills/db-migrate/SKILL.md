---
name: db-migrate
description: Change the Bullet Heaven database schema via EF Core migrations. Use when adding/altering server models (User, Score), columns, indexes, or tables.
---

# Database schema changes (EF Core + PostgreSQL)

Never hand-edit the database or existing files in `BulletHeaven.Server/Migrations/` — they are generated.

1. Edit the model in `BulletHeaven.Server/Models/` and/or the DbContext in `Data/`.
2. Generate: `cd BulletHeaven.Server && dotnet ef migrations add <DescriptivePascalName>`
3. Review the generated `Up()`/`Down()` — destructive ops (column drops, type changes that lose data) must be flagged to the user before applying.
4. Apply locally: `dotnet ef database update` (DB must be running — `docker compose up db` if not).
5. Verify: `dotnet build` + hit `GET /health`; exercise the affected endpoint.

## Rules
- Connection strings/credentials come from `dotnet user-secrets` or env vars — never hardcode or commit them.
- Keep DTOs separate from EF entities when API shape differs from storage shape.
- New user-facing fields on `Score`/leaderboard responses: expose username + score only — never IDs, emails, or password hashes.
