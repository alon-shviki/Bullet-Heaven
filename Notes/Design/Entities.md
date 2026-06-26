# Entities

## Player

**Class:** `BulletHeaven.Client/Game/Entities/Player.cs`

| Stat | Default | Notes |
|------|---------|-------|
| Speed | 200 px/sec | Upgradeable to 400 max |
| MaxHealth | 5 HP | Upgradeable |
| MagnetRange | 80 px | Range at which gems pull toward player |
| HpRegenPerKill | 0 | Life Drain upgrade adds 1 (max 2) |
| InvincibilityDuration | 1.0 s | I-frames after taking a hit |

Movement is clamped to canvas bounds (`GameBounds.Width / Height`). `TakeDamage()` sets `IsInvincible = true` for the i-frame window. `IsDead` fires game over.

On `Reset()`: all stats return to defaults. Called on new game start.

---

## Enemies

**Class:** `BulletHeaven.Client/Game/Entities/Enemy.cs`  
**Pool:** `EnemyPool` — 1000 pre-allocated slots, zero `new` during gameplay

### Enemy Types

| Type | Radius | Speed | HP | XP | Score | Notes |
|------|--------|-------|----|----|-------|-------|
| Standard | 12 | 80 | 1 | 1 | 10 | Basic tracker |
| Runner | smaller | faster | 1 | varies | varies | Rushes player |
| Tank | larger | slower | higher | higher | higher | Takes multiple hits |
| Elite | larger | medium | high | high | high | Mini-boss in waves |
| Boss | large | variable | very high | high | high | Appears at intervals |

Exact stats per type are set by `EnemySpawner.Activate(...)` using `DifficultySettings` for the current wave.

### Enemy Behaviour

Each tick: compute normalized vector toward player `(px - ex, py - ey)`, move `Speed * dt` along it. No pathfinding — pure homing.

Collision radius is `enemy.Radius`. Circle-vs-circle check against projectiles via the quadtree. Circle-vs-circle against player directly (no quadtree — one check).

---

## Projectile

**Class:** `BulletHeaven.Client/Game/Entities/Projectile.cs`  
**Pool:** `BulletPool` — 500 pre-allocated slots

Fields: `X, Y, VX, VY, Radius, Active, Piercing` (remaining pierce count), `RicochetCount` (remaining bounces), `HomingStrength`.

Deactivated (not deleted) when: off-screen, piercing exhausted, or ricochet count exhausted.

---

## XpGem

**Class:** `BulletHeaven.Client/Game/Entities/XpGem.cs`

Spawned at enemy death position. If within `player.MagnetRange`, accelerates toward player each tick. On overlap with player: collected, `player.CurrentXp += gem.Value`.

---

## Particle / ParticleSystem

**Class:** `BulletHeaven.Client/Game/Entities/Particle.cs` + `ParticleSystem.cs`

Visual-only. Spawned on enemy death and explosions. Each particle has position, velocity, lifetime, and alpha. Fades out over lifetime. Pre-allocated pool; no heap alloc during gameplay.
