using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Client.Game.Upgrades;

public enum UpgradeRarity { Common, Rare, Epic }

public record UpgradeDefinition(
    string Name,
    string Description,
    UpgradeRarity Rarity,
    int Weight,
    Action<WeaponStats, Player> Apply,
    Func<WeaponStats, Player, bool> IsUseful
);
