using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Entities;
using BulletHeaven.Client.Game.Upgrades;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="UpgradeCatalogue.PickThree"/> and the effects /
/// usefulness gates of individual <see cref="UpgradeDefinition"/>s.
/// PickThree is random, so tests assert invariants (count, distinctness,
/// usefulness filter) rather than specific picks.
/// </summary>
public class UpgradeCatalogueTests
{
    private static UpgradeDefinition Find(string name) =>
        UpgradeCatalogue.All.First(u => u.Name == name);

    // ── PickThree invariants ──────────────────────────────────────────────────

    [Fact]
    public void PickThree_ReturnsThreeDistinctUpgrades()
    {
        var stats  = new WeaponStats();
        var player = new Player();

        for (var run = 0; run < 50; run++)
        {
            var picks = UpgradeCatalogue.PickThree(stats, player);

            Assert.Equal(3, picks.Count);
            Assert.Equal(3, picks.Distinct().Count());
        }
    }

    [Fact]
    public void PickThree_OnlyReturnsUsefulUpgrades()
    {
        var stats  = new WeaponStats();
        var player = new Player(); // full HP — "Mend" / "Full Recovery" are useless

        for (var run = 0; run < 50; run++)
        {
            var picks = UpgradeCatalogue.PickThree(stats, player);

            Assert.All(picks, u => Assert.True(
                u.IsUseful(stats, player),
                $"'{u.Name}' was offered despite IsUseful == false"));
        }
    }

    [Fact]
    public void PickThree_FullHealthPlayer_NeverOffersHeals()
    {
        var stats  = new WeaponStats();
        var player = new Player();

        for (var run = 0; run < 50; run++)
        {
            var picks = UpgradeCatalogue.PickThree(stats, player);
            Assert.DoesNotContain(picks, u => u.Name is "Mend" or "Full Recovery");
        }
    }

    // ── Usefulness gates ──────────────────────────────────────────────────────

    [Fact]
    public void Mend_IsUsefulOnlyWhenHurt()
    {
        var stats  = new WeaponStats();
        var player = new Player();
        var mend   = Find("Mend");

        Assert.False(mend.IsUseful(stats, player));

        player.TakeDamage();
        Assert.True(mend.IsUseful(stats, player));
    }

    [Fact]
    public void SplitShot_IsUselessOnceBulletCountReachesTwo()
    {
        var stats = new WeaponStats { BulletCount = 2 };

        Assert.False(Find("Split Shot").IsUseful(stats, new Player()));
    }

    [Fact]
    public void WideOrbit_RequiresAnOrbFirst()
    {
        var stats = new WeaponStats(); // OrbCount == 0

        Assert.False(Find("Wide Orbit").IsUseful(stats, new Player()));

        stats.OrbCount = 1;
        Assert.True(Find("Wide Orbit").IsUseful(stats, new Player()));
    }

    // ── Apply effects ─────────────────────────────────────────────────────────

    [Fact]
    public void QuickReload_ReducesFireRateByTwentyPercent()
    {
        var stats = new WeaponStats { FireRate = 1.0 };

        Find("Quick Reload").Apply(stats, new Player());

        Assert.Equal(0.8, stats.FireRate, 1e-9);
    }

    // Fire-rate upgrades must never push the interval below the 0.3s floor.
    [Fact]
    public void FireRateUpgrades_RespectTheFloor()
    {
        var stats  = new WeaponStats();
        var player = new Player();
        var gatling = Find("Gatling");

        for (var i = 0; i < 20; i++)
            gatling.Apply(stats, player);

        Assert.True(stats.FireRate >= 0.3, $"fire rate {stats.FireRate} broke the 0.3 floor");
    }

    [Fact]
    public void TriShot_SetsThreeBulletsWithSpread()
    {
        var stats = new WeaponStats();

        Find("Tri-Shot").Apply(stats, new Player());

        Assert.Equal(3, stats.BulletCount);
        Assert.True(stats.SpreadAngle > 0);
    }

    [Fact]
    public void IronWill_GrantsOneMaxHp()
    {
        var player = new Player();

        Find("Iron Will").Apply(new WeaponStats(), player);

        Assert.Equal(6, player.MaxHealth);
        Assert.Equal(6, player.CurrentHealth);
    }

    [Fact]
    public void FullRecovery_HealsToFull()
    {
        var player = new Player();
        player.TakeDamage();

        Find("Full Recovery").Apply(new WeaponStats(), player);

        Assert.Equal(player.MaxHealth, player.CurrentHealth);
    }

    // ── Catalogue sanity ──────────────────────────────────────────────────────

    [Fact]
    public void AllUpgrades_HaveUniqueNamesAndPositiveWeights()
    {
        Assert.Equal(UpgradeCatalogue.All.Count, UpgradeCatalogue.All.Select(u => u.Name).Distinct().Count());
        Assert.All(UpgradeCatalogue.All, u => Assert.True(u.Weight > 0, $"'{u.Name}' has non-positive weight"));
    }
}
