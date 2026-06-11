using BulletHeaven.Client.Game;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="DifficultyManager"/>: baseline values, the
/// time-based ramp, the hard caps, and reset.
/// Default settings: spawn 1.5s, speed 100, ramp +30% per minute.
/// </summary>
public class DifficultyManagerTests
{
    private const double Tolerance = 1e-9;

    // ── Baseline (t = 0) ──────────────────────────────────────────────────────

    [Fact]
    public void AtStart_UsesBaseSettings()
    {
        var d = new DifficultyManager();

        Assert.Equal(0, d.ElapsedTime);
        Assert.Equal(1.5, d.CurrentSpawnInterval, Tolerance);
        Assert.Equal(100, d.CurrentEnemySpeed, Tolerance);
        Assert.Equal(2, d.CurrentEnemiesPerWave);
        Assert.Equal(5, d.PlayerMaxHealth);
        Assert.Equal(1.0, d.InvincibilityDuration);
        Assert.Equal("Survival", d.Name);
    }

    // ── Ramp ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_AccumulatesElapsedTime()
    {
        var d = new DifficultyManager();

        d.Update(1.5);
        d.Update(2.5);

        Assert.Equal(4.0, d.ElapsedTime, Tolerance);
    }

    [Fact]
    public void AfterOneMinute_RampIsThirtyPercent()
    {
        var d = new DifficultyManager();

        d.Update(60);

        // Ramp = 1.3 → interval 1.5/1.3, speed 100×1.3, wave 2+1.
        Assert.Equal(1.5 / 1.3, d.CurrentSpawnInterval, Tolerance);
        Assert.Equal(130, d.CurrentEnemySpeed, Tolerance);
        Assert.Equal(3, d.CurrentEnemiesPerWave);
    }

    [Fact]
    public void Ramp_MakesSpawnsFasterAndEnemiesQuickerOverTime()
    {
        var d = new DifficultyManager();
        var spawn0 = d.CurrentSpawnInterval;
        var speed0 = d.CurrentEnemySpeed;

        d.Update(120);

        Assert.True(d.CurrentSpawnInterval < spawn0);
        Assert.True(d.CurrentEnemySpeed > speed0);
    }

    // ── Caps ──────────────────────────────────────────────────────────────────

    [Fact]
    public void SpawnInterval_NeverDropsBelowFloor()
    {
        var d = new DifficultyManager();

        d.Update(60 * 60); // one hour

        Assert.Equal(0.3, d.CurrentSpawnInterval, Tolerance);
    }

    [Fact]
    public void EnemySpeed_NeverExceedsCeiling()
    {
        var d = new DifficultyManager();

        d.Update(60 * 60);

        Assert.Equal(300.0, d.CurrentEnemySpeed, Tolerance);
    }

    [Fact]
    public void EnemiesPerWave_CapsAtTen()
    {
        var d = new DifficultyManager();

        d.Update(60 * 60);

        Assert.Equal(10, d.CurrentEnemiesPerWave);
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ReturnsToBaseline()
    {
        var d = new DifficultyManager();
        d.Update(300);

        d.Reset();

        Assert.Equal(0, d.ElapsedTime);
        Assert.Equal(1.5, d.CurrentSpawnInterval, Tolerance);
        Assert.Equal(100, d.CurrentEnemySpeed, Tolerance);
        Assert.Equal(2, d.CurrentEnemiesPerWave);
    }
}
