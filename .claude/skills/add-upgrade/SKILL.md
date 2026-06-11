---
name: add-upgrade
description: Add a new upgrade to the Bullet Heaven upgrade catalogue. Use when the user asks to add, create, or design a new upgrade, perk, or power-up.
---

# Add a new upgrade

> Before writing the entry, read `references/stat-wiring.md` — every `WeaponStats` property with defaults/caps/consumers, player-stat caps, proven entry patterns (enable-then-upgrade, capped stack, consumable), and weight conventions.

## Touchpoints (in order)
1. **`Game/Upgrades/UpgradeCatalogue.cs`** — add a `new(...)` entry to `_all`:
   `new("Name", "Short description", UpgradeRarity.X, weight, applyAction, isUsefulPredicate)`
   - Signature: `(WeaponStats s, Player p)` for both delegates.
   - Weight conventions already in the file: Common 40–60, Rare 20–30, Epic 8–10.
   - `IsUseful` must return `false` once the upgrade is maxed/redundant (e.g. `s.Piercing < 999`) so it stops appearing in `PickThree`.
2. **New stat?** If the effect needs a property that doesn't exist, add it to `Game/WeaponStats.cs` (weapon behavior) or `Game/Entities/Player.cs` (player stat), then consume it where it acts: `Weapon.cs`/`Projectile.cs` for bullets, `OrbWeapon`/`PulseWeapon`/`AuraWeapon` for secondaries, `Game.razor` update loop for player effects.
3. **Codex**: nothing to do — the Archive upgrades tab auto-lists from `UpgradeCatalogue.All` grouped by rarity.
4. **README.md** — only if it's a new *weapon* (Weapons & upgrades paragraph), not for stat tweaks.

## Constraints
- The apply action runs once at pick time (game paused) — allocations are fine there. The *effect* runs per frame — follow `.claude/rules/performance.md` in the consuming code.
- Stack-safety: clamp like existing entries (`Math.Max`, ceiling checks) so repeated picks can't break the game (e.g. FireRate floor 0.3).

## Verify
- `dotnet build` then `dotnet test BulletHeaven.Tests`.
- Add an xUnit test if the upgrade has math (clamps, multipliers).
- Run the game and confirm the upgrade appears on level-up and in the Archive.
