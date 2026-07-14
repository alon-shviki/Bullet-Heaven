# CI and Tests

## CI Pipeline

`.github/workflows/ci.yml` is a **thin caller** that runs the portal's shared reusable workflow (`alon-shviki/game-portal/.github/workflows/dotnet-ci.yml@main`) with this repo's paths. The gate itself is single-source — it's defined once in the portal, not copied here. Runs on every PR and push to `main`.

| Job | Trigger | What it does |
|-----|---------|--------------|
| `build` (reusable) | every PR + push | cache NuGet → `dotnet format --verify-no-changes` → build client → run full test suite |
| `push-image` (reusable) | merge to main only | pushes `ghcr.io/alon-shviki/bh-client:latest` to GHCR |

`main` is protected — the required check is **`ci / build`** (the reusable `build` job under our caller job `ci`). The gate includes a **formatting check**: if `dotnet format --verify-no-changes` finds unformatted code the build goes red — run `dotnet format` locally and commit before pushing. NuGet packages are cached (`actions/cache` keyed on the `.csproj` hashes) to keep the build fast.

## Test Suite

`BulletHeaven.Tests/` — xUnit unit tests covering core game logic.

| File | What it tests |
|------|--------------|
| `PlayerTests.cs` | movement, health, i-frames, XP |
| `EnemyTests.cs` / `EnemySpawnerTests.cs` | spawn logic, tracking AI |
| `ProjectileTests.cs` / `WeaponTests.cs` / `OrbWeaponTests.cs` | weapon firing, cooldowns |
| `EntityPoolTests.cs` | pool allocation, recycling |
| `QuadtreeTests.cs` | spatial partitioning, queries |
| `CollisionTests` (via entity tests) | circle overlap |
| `XpGemTests.cs` | magnet range, collection |
| `GameMathTests.cs` / `GameMathClamp01Tests.cs` | math helpers |
| `DifficultyManagerTests.cs` | wave scaling |
| `UpgradeCatalogueTests.cs` | weighted draw, eligibility |
| `ParticleSystemTests.cs` | particle lifecycle |

Run locally:
```bash
dotnet test BulletHeaven.Tests/BulletHeaven.Tests.csproj
```

## Agent Pipeline

Every `.cs`/`.razor` change goes through a mandatory sub-agent pipeline (`qa-reviewer` → `test-generator` → `docs-generator` → conditional `playwright-e2e`), defined in `.claude/rules/pipeline.md` with the agents themselves in `.claude/agents/`. This is self-contained to this repo — the portal previously kept `bh-`-prefixed copies for portal sessions, but those were reverted (portal now does the equivalent checks inline instead of spawning sub-agents). Verified 2026-07-03 that this repo's pipeline still runs standalone with those portal copies gone.

## Known Gaps

- No tests for rendering (`Game.Render.cs`) — canvas calls can't be unit tested
- E2E tests exist (`e2e/tests/ui-flows.spec.ts`, Playwright) but are not in CI yet

## Related

- [[Tech/Architecture]] — project layout, where test files fit in the structure
- [[Tech/Performance]] — performance rules that tests guard against regressions
