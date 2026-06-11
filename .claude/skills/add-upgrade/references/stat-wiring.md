# UpgradeDefinition + WeaponStats wiring map

## The record (Game/Upgrades/UpgradeDefinition.cs)
```csharp
public record UpgradeDefinition(
    string Name, string Description, UpgradeRarity Rarity, int Weight,
    Action<WeaponStats, Player> Apply,
    Func<WeaponStats, Player, bool> IsUseful);
```
`UpgradeRarity { Common, Rare, Epic }`. `PickThree` filters by `IsUseful` first, then weighted-rolls — a bad `IsUseful` means a dead pick keeps appearing.

## Every WeaponStats property and where it's consumed
| Property | Default | Consumed in | Effect |
|---|---|---|---|
| `FireRate` | 1.5 | `Weapon.cs` | seconds between shots — LOWER is faster; floor at 0.3 (`Math.Max(0.3, …)`) |
| `BulletSpeed` | 400 | `Weapon.cs`/`Projectile.cs` | px/s |
| `BulletRadius` | 6 | `Weapon.cs` (spawn) + render | hitbox + visual |
| `BulletCount` | 1 | `Weapon.cs` | bullets per shot; pair with `SpreadAngle` |
| `SpreadAngle` | 0 | `Weapon.cs` | radians between spread bullets (`Math.PI / 12` standard) |
| `Piercing` | 0 | `Projectile.cs` + `Game.razor` | enemies passed through; 999 = infinite |
| `OrbCount` | 0 | `OrbWeapon.cs` | orbiting orbs; cap 5 |
| `OrbOrbitRadius` | 80 | `OrbWeapon.cs` | orbit distance; cap 200, only useful if `OrbCount > 0` |
| `PulseInterval` | 0 | `PulseWeapon.cs` | 0 = disabled; enable by setting 3.0 |
| `PulseRadius` | 0 | `PulseWeapon.cs` | explosion radius; only useful if `PulseInterval > 0` |
| `HomingStrength` | 0 | `Game.razor` update loop | 0 = off; 0.08 standard |
| `RicochetCount` | 0 | `Weapon.cs` | wall bounces; cap 6 |
| `ExplosionRadius` | 0 | kill handling in `Game.razor` | 0 = off; enable 60, upgrade +30 |
| `AuraRadius` | 0 | `AuraWeapon.cs` | 0 = off; enable 80, expand +30 |

## Player-side stats (Game/Entities/Player.cs)
`Speed` (cap 400) · `MaxHealth`/`CurrentHealth` via `Heal()`/`IncreaseMaxHealth()` · `HpRegenPerKill` (cap 2) · `MagnetRange` (cap 400) · `AddInvincibilityDuration()`.

## Patterns to copy
- **Enable-then-upgrade pair:** an Epic/Rare that turns a system on (`s.AuraRadius = 80`, useful while `<= 0`) + a cheaper booster (`s.AuraRadius += 30`, useful while `> 0`).
- **Capped stack:** `(s,_) => s.RicochetCount += 2` with `IsUseful: s.RicochetCount < 6`.
- **One-shot consumable:** `Mend`/`Full Recovery` — useful only while `CurrentHealth < MaxHealth`.

## Weight reference (existing distribution)
Common 40–60 · Rare 20–30 · Epic 8–10. New enabling-Epics: 8. New Common boosters: 40.
