# Backend

**Project:** `BulletHeaven.Server` (ASP.NET Core .NET 10)  
**Database:** PostgreSQL via EF Core  
**Auth:** JWT Bearer + bcrypt (`BCrypt.Net-Next`)

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/register` | Public | Create account. Returns 409 if username taken. |
| POST | `/api/login` | Public | Validate credentials, return signed JWT. Returns 401 on bad creds. |
| POST | `/api/scores` | Bearer JWT | Save a completed run score. Returns 401 if token missing/invalid. |
| GET | `/api/leaderboard` | Public | Top 10 scores (username + score), ordered descending. |
| GET | `/health` | Public | Liveness check, returns `{ status: "ok" }`. |

## Database Schema

### User

| Column | Type | Notes |
|--------|------|-------|
| Id | int PK | Auto-increment |
| Username | string | Unique index |
| PasswordHash | string | bcrypt hash — never stored plain |

### Score

| Column | Type | Notes |
|--------|------|-------|
| Id | int PK | Auto-increment |
| UserId | int FK | → User.Id |
| Value | int | Final score from the run |
| CreatedAt | DateTime | UTC timestamp |

Relationships: `User` has many `Score`. Leaderboard query: `ORDER BY Value DESC LIMIT 10`.

## Security Rules

- Passwords: bcrypt hash only, never logged or returned
- JWT signing key: `dotnet user-secrets` in dev, environment variable in Docker — never hardcoded
- All write endpoints decorated with `[Authorize]`
- CORS locked to WASM client origin (not `*`)
- Validate all request bodies — reject missing/oversized fields with 400

## Running Locally

```bash
# Dev (in-memory or local postgres)
cd BulletHeaven.Server
dotnet user-secrets set "Jwt:Key" "your-dev-secret"
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Database=bulletheaven;..."
dotnet run

# Full stack
docker compose up --build
# PostgreSQL + server + client all wired up
```

## EF Core Migrations

```bash
cd BulletHeaven.Server
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Never edit migration files manually. Never use raw SQL for schema changes.
