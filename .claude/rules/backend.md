# Backend Rules — BulletHeaven.Server

## Security
- Every endpoint that writes data (`POST /api/scores`, etc.) must have `[Authorize]` (JWT Bearer). Public reads: `/api/leaderboard`, `/health`.
- Validate all request bodies — reject missing/oversized/nonsense fields explicitly; never trust client-reported scores blindly.
- Passwords: bcrypt hash only (`BCrypt.Net-Next`); never log or return password fields.
- Secrets (JWT signing key, connection strings) come from `dotnet user-secrets` / environment variables — never hardcode, log, or commit them.
- Return proper status codes: 401 for bad/missing tokens, 409 for duplicate username, 400 for validation failures.

## Data
- EF Core + PostgreSQL. Schema changes go through migrations (`dotnet ef migrations add <Name>` in `BulletHeaven.Server/`), never manual SQL.
- Models live in `Models/` (`User`, `Score`); keep DTOs separate from EF entities when shapes diverge.

## Conventions
- Controllers stay thin; no business logic in `Program.cs` beyond wiring.
- Leaderboard returns top 10 by score descending — username + score only, no user IDs or emails.
- CORS is configured for the WASM client origin; don't open it to `*`.
