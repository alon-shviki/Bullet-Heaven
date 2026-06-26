# Weapons & Upgrades

## Primary Weapon (Auto-targeting)

**Class:** `BulletHeaven.Client/Game/Entities/Weapon.cs`

Auto-fires toward the nearest enemy on cooldown. Stats controlled by `WeaponStats`.

### WeaponStats defaults

| Stat | Default | Description |
|------|---------|-------------|
| FireRate | 1.5 s | Seconds between shots |
| BulletSpeed | 400 px/s | Projectile velocity |
| BulletRadius | 6 px | Hitbox + visual size |
| BulletCount | 1 | Bullets per shot (spread) |
| SpreadAngle | 0 rad | Angle between bullets in multi-shot |
| Piercing | 0 | Extra enemies a bullet passes through |
| HomingStrength | 0 | How hard bullets curve toward enemies |
| RicochetCount | 0 | Wall bounces per bullet |
| ExplosionRadius | 0 | Blast radius on kill (0 = disabled) |

---

## Secondary Weapons

### Orb Shield

**Class:** `OrbWeapon.cs` — up to 16 orbs, max 5 via upgrades

Orbs orbit the player at `OrbOrbitRadius` (default 80 px), rotating continuously. Each orb damages enemies on contact. Position calculated geometrically each tick — no physics simulation.

### Pulse Nova

**Class:** `PulseWeapon.cs`

Radial explosion every `PulseInterval` seconds. Damages all enemies within `PulseRadius`. `JustPulsed` flag is read by the renderer for the visual flash.

### Damage Aura

**Class:** `AuraWeapon.cs`

Passive continuous damage field. Ticks every 0.5 s, hits all enemies within `AuraRadius`. Unlocked by the "Damage Aura" upgrade.

---

## Upgrade Catalogue

**Class:** `UpgradeCatalogue.cs` — weighted random draw of 3 distinct eligible upgrades per level-up.

### Rarities & Weights

| Rarity | Weight |
|--------|--------|
| Common | 40–60 |
| Rare | 20–30 |
| Epic | 8–10 |

### Weapon Upgrades

| Name | Rarity | Effect |
|------|--------|--------|
| Quick Reload | Common | Fire rate ×0.80 (faster) |
| Swift Rounds | Common | Bullet speed ×1.30 |
| Larger Caliber | Common | Bullet radius +2 |
| Double Tap | Rare | Fire rate ×0.65 |
| Split Shot | Rare | 2 bullets in spread (unlocks once) |
| Armor-Piercing | Rare | Piercing +1 |
| Velocity Core | Rare | Bullet speed ×1.60 |
| Tri-Shot | Epic | 3 bullets in spread (unlocks once) |
| Full Penetration | Epic | Pierce all enemies |
| Gatling | Epic | Fire rate ×0.40 |
| Homing Rounds | Rare | Bullets home in (HomingStrength = 0.08) |
| Ricochet | Rare | Bullets bounce ×2 off walls |
| Explosive Rounds | Epic | Kills explode in 60 px radius |
| Bigger Blast | Rare | Explosion radius +30 |

### Player Upgrades

| Name | Rarity | Effect |
|------|--------|--------|
| Mend | Common | Restore 1 HP (only if not full) |
| Swift Feet | Common | Speed ×1.20 (max 400) |
| Iron Will | Rare | +1 max HP |
| Life Drain | Rare | +1 HP per kill (max 2) |
| Magnet | Common | Magnet range +50 (max 400) |
| Full Recovery | Epic | Restore to full HP |
| Fortify | Common | I-frame duration +0.3 s |

### Secondary / Special Upgrades

| Name | Rarity | Effect |
|------|--------|--------|
| Orb Shield | Rare | Add 1 orbiting orb (max 5) |
| Wide Orbit | Common | Orb orbit radius +20 (max 200, requires orb) |
| Pulse Nova | Epic | Unlocks area explosion every 3 s |
| Pulse Amp | Rare | Pulse radius +40 (requires Pulse Nova) |
| Damage Aura | Rare | Unlocks passive damage aura (80 px radius) |
| Aura Expander | Common | Aura radius +30 (requires aura active) |

### Eligibility

Each upgrade has a predicate (`IsEligible`) that prevents duplicates or maxed stats from appearing:
- Split Shot won't appear again once `BulletCount >= 2`
- Mend won't appear at full HP
- Wide Orbit requires at least one orb active
