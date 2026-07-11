using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Client.Game.Upgrades;

public static class UpgradeCatalogue
{
    public static IReadOnlyList<UpgradeDefinition> All => _all;

    private static readonly List<UpgradeDefinition> _all = new()
    {
        new("Quick Reload",     "Fire rate +20%",             UpgradeRarity.Common, 60, (s, _) => s.FireRate     = Math.Max(0.3, s.FireRate * 0.80), (s, _) => s.FireRate > 0.31),
        new("Swift Rounds",     "Bullet speed +30%",          UpgradeRarity.Common, 60, (s, _) => s.BulletSpeed *= 1.30,                             (_, _) => true),
        new("Larger Caliber",   "Bullet size +2",             UpgradeRarity.Common, 60, (s, _) => s.BulletRadius += 2,                               (_, _) => true),
        new("Mend",             "Restore 1 HP",               UpgradeRarity.Common, 60, (_, p) => p.Heal(1),                                         (_, p) => p.CurrentHealth < p.MaxHealth),

        new("Double Tap",       "Fire rate +35%",             UpgradeRarity.Rare,   30, (s, _) => s.FireRate     = Math.Max(0.3, s.FireRate * 0.65), (s, _) => s.FireRate > 0.31),
        new("Split Shot",       "Fire 2 bullets in a spread", UpgradeRarity.Rare,   30, (s, _) => { s.BulletCount = Math.Max(s.BulletCount, 2); s.SpreadAngle = Math.PI / 12; }, (s, _) => s.BulletCount < 2),
        new("Armor-Piercing",   "Bullets pierce 1 enemy",     UpgradeRarity.Rare,   30, (s, _) => s.Piercing    += 1,                                (s, _) => s.Piercing < 999),
        new("Velocity Core",    "Bullet speed +60%",          UpgradeRarity.Rare,   30, (s, _) => s.BulletSpeed *= 1.60,                             (_, _) => true),

        new("Tri-Shot",         "Fire 3 bullets in a spread", UpgradeRarity.Epic,   10, (s, _) => { s.BulletCount = Math.Max(s.BulletCount, 3); s.SpreadAngle = Math.PI / 12; }, (s, _) => s.BulletCount < 3),
        new("Full Penetration", "Bullets pierce all enemies", UpgradeRarity.Epic,   10, (s, _) => s.Piercing     = 999,                              (s, _) => s.Piercing < 999),
        new("Gatling",          "Fire rate +60%",             UpgradeRarity.Epic,   10, (s, _) => s.FireRate     = Math.Max(0.3, s.FireRate * 0.40), (s, _) => s.FireRate > 0.31),

        // player stat upgrades
        new("Swift Feet",    "Move speed +20%",           UpgradeRarity.Common, 50, (_, p) => p.Speed           *= 1.20, (_, p) => p.Speed < 400),
        new("Iron Will",     "+1 max HP",                 UpgradeRarity.Rare,   25, (_, p) => p.IncreaseMaxHealth(1),    (_, _) => true),
        new("Life Drain",    "+1 HP on kill",             UpgradeRarity.Rare,   25, (_, p) => p.HpRegenPerKill++,        (_, p) => p.HpRegenPerKill < 2),
        new("Magnet",        "Gem magnet range +50",      UpgradeRarity.Common, 50, (_, p) => p.MagnetRange     += 50,   (_, p) => p.MagnetRange < 400),
        new("Full Recovery", "Restore to full HP",        UpgradeRarity.Epic,    8, (_, p) => p.Heal(p.MaxHealth),       (_, p) => p.CurrentHealth < p.MaxHealth),
        new("Fortify",       "Invincibility frames +0.3s",UpgradeRarity.Common, 40, (_, p) => p.AddInvincibilityDuration(0.3), (_, _) => true),

        // secondary weapons
        new("Orb Shield",   "Add 1 orbiting damage orb",  UpgradeRarity.Rare,   25, (s, _) => s.OrbCount       += 1,                                       (s, _) => s.OrbCount < 5),
        new("Wide Orbit",   "Orb orbit radius +20",        UpgradeRarity.Common, 40, (s, _) => s.OrbOrbitRadius += 20,                                      (s, _) => s.OrbCount > 0 && s.OrbOrbitRadius < 200),
        new("Pulse Nova",   "Area explosion every 3s",     UpgradeRarity.Epic,    8, (s, _) => { s.PulseInterval = 3.0; s.PulseRadius = 100; },              (s, _) => s.PulseInterval <= 0),
        new("Pulse Amp",    "Pulse explosion radius +40",  UpgradeRarity.Rare,   20, (s, _) => s.PulseRadius    += 40,                                       (s, _) => s.PulseInterval > 0),

        // new weapons
        new("Homing Rounds",   "Bullets home in on enemies",   UpgradeRarity.Rare,   20, (s, _) => s.HomingStrength  = 0.08, (s, _) => s.HomingStrength  <= 0),
        new("Ricochet",        "Bullets bounce off walls ×2",  UpgradeRarity.Rare,   20, (s, _) => s.RicochetCount  += 2,    (s, _) => s.RicochetCount   <  6),
        new("Explosive Rounds","Kills explode in an area",      UpgradeRarity.Epic,    8, (s, _) => s.ExplosionRadius = 60,   (s, _) => s.ExplosionRadius <= 0),
        new("Bigger Blast",    "Explosion radius +30",          UpgradeRarity.Rare,   20, (s, _) => s.ExplosionRadius += 30,  (s, _) => s.ExplosionRadius >  0),
        new("Damage Aura",     "Passive damage field around you",UpgradeRarity.Rare,  20, (s, _) => s.AuraRadius      = 80,   (s, _) => s.AuraRadius      <= 0),
        new("Aura Expander",   "Damage aura radius +30",        UpgradeRarity.Common, 40, (s, _) => s.AuraRadius     += 30,   (s, _) => s.AuraRadius      >  0),
    };

    public static List<UpgradeDefinition> PickThree(WeaponStats stats, Player player)
    {
        var remaining = _all.Where(u => u.IsUseful(stats, player)).ToList();
        var picked = new List<UpgradeDefinition>(3);

        for (var i = 0; i < 3 && remaining.Count > 0; i++)
        {
            var total = remaining.Sum(u => u.Weight);
            var roll = Random.Shared.NextDouble() * total;
            var cumulative = 0.0;
            foreach (var u in remaining)
            {
                cumulative += u.Weight;
                if (roll <= cumulative)
                {
                    picked.Add(u);
                    remaining.Remove(u);
                    break;
                }
            }
        }

        return picked;
    }
}
