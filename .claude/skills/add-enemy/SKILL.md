---
name: add-enemy
description: Add a new enemy type to Bullet Heaven. Use when the user asks to add, create, or design a new enemy, monster, or boss variant.
---

# Add a new enemy type

> Before steps 3–4 (rendering + codex), read `references/codex-and-preview.md` — exact card HTML template, JS preview dispatch, and the existing color palette.

## Touchpoints (in order)
1. **`Game/Entities/Enemy.cs`** — add the value to `enum EnemyType`.
2. **`Game/Entities/EnemySpawner.cs`**:
   - `SpawnOne` stat switch: add `(radius, speedMult, hp, killValue, xpValue, scoreValue)` tuple. Existing scale for reference: Runner (8, 1.5×, 1 HP, 15 pts) · Tank (20, 0.5×, 3 HP, 25) · Elite (16, 1.2×, 5 HP, 80).
   - `PickType()` roll table: insert a percentage band and rebalance — bands must still cover 0–99 (currently Elite 3%, Tank 12%, Runner 25%, Standard rest).
3. **`Pages/Game.Render.cs`** — add a draw branch in the enemy-render dispatch (~line 288); follow the existing `DrawTank`/`DrawRunner` pattern. Batched calls only — no new interop round trips.
4. **`Pages/Game.razor`**:
   - Color map (~line 912): add `EnemyType.X => "#hex"`.
   - Archive → Enemies tab (~line 147): cards are **hardcoded HTML** — add an `enemy-card` block with HP/Speed/XP/Score/Spawn% matching step 2 exactly (template in `references/codex-and-preview.md`).
   - Codex preview: add a `data-entity` branch in `gameInterop.js drawEntityPreviews()` + a `_drawX` function mirroring your `Game.Render.cs` shape 1:1.
5. **`README.md`** — add a row to the "Enemy types" table.

## Constraints
- Special behavior (dashing, shooting, splitting) goes in `Enemy.Update` or the `Game.razor` update loop — per-frame code, so `.claude/rules/performance.md` applies: no allocations, no LINQ.
- Boss-likes: see `SpawnBoss` for the off-screen-edge spawn pattern and `Game.razor:526` for boss kill rewards (`_pendingExtraRewards`).

## Verify
- `dotnet build` + `dotnet test BulletHeaven.Tests`.
- Run the game: confirm spawn rate feels right, codex card matches real stats, and the new draw branch renders correctly.
