using BulletHeaven.Client.Game;
using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="Player"/>: health/damage, invincibility frames,
/// healing, movement clamping, and reset.
/// Note: relies on the default <see cref="GameBounds"/> of 800×600 — tests never
/// mutate the shared static bounds, so parallel test classes stay safe.
/// </summary>
public class PlayerTests
{
    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void NewPlayer_StartsAtCenterWithFullHealth()
    {
        var p = new Player();

        Assert.Equal(GameBounds.CenterX, p.X);
        Assert.Equal(GameBounds.CenterY, p.Y);
        Assert.Equal(5, p.MaxHealth);
        Assert.Equal(p.MaxHealth, p.CurrentHealth);
        Assert.False(p.IsInvincible);
        Assert.False(p.IsDead);
    }

    // ── TakeDamage / invincibility ────────────────────────────────────────────

    [Fact]
    public void TakeDamage_ReducesHealthByOne_AndGrantsInvincibility()
    {
        var p = new Player();

        p.TakeDamage();

        Assert.Equal(4, p.CurrentHealth);
        Assert.True(p.IsInvincible);
    }

    [Fact]
    public void TakeDamage_WhileInvincible_IsIgnored()
    {
        var p = new Player();

        p.TakeDamage();
        p.TakeDamage(); // still invincible — must not stack

        Assert.Equal(4, p.CurrentHealth);
    }

    [Fact]
    public void Invincibility_ExpiresAfterDuration()
    {
        var p = new Player();
        p.TakeDamage();

        p.Update(1.01, 0, 0); // default duration is 1.0s

        Assert.False(p.IsInvincible);
    }

    [Fact]
    public void Invincibility_PersistsBeforeDurationElapses()
    {
        var p = new Player();
        p.TakeDamage();

        p.Update(0.5, 0, 0);

        Assert.True(p.IsInvincible);
    }

    [Fact]
    public void AddInvincibilityDuration_ExtendsTheWindow()
    {
        var p = new Player();
        p.AddInvincibilityDuration(0.5); // now 1.5s
        p.TakeDamage();

        p.Update(1.2, 0, 0);
        Assert.True(p.IsInvincible);

        p.Update(0.4, 0, 0);
        Assert.False(p.IsInvincible);
    }

    [Fact]
    public void Player_DiesAfterMaxHealthHits()
    {
        var p = new Player();

        for (var i = 0; i < 5; i++)
        {
            p.TakeDamage();
            p.Update(1.01, 0, 0); // wait out invincibility between hits
        }

        Assert.True(p.IsDead);
        Assert.Equal(0, p.CurrentHealth);
    }

    [Fact]
    public void TakeDamage_WhenDead_DoesNotGoNegative()
    {
        var p = new Player();
        for (var i = 0; i < 5; i++)
        {
            p.TakeDamage();
            p.Update(1.01, 0, 0);
        }

        p.TakeDamage();

        Assert.Equal(0, p.CurrentHealth);
    }

    // ── Heal / max health ─────────────────────────────────────────────────────

    [Fact]
    public void Heal_RestoresHealth_CappedAtMax()
    {
        var p = new Player();
        p.TakeDamage();

        p.Heal(10);

        Assert.Equal(p.MaxHealth, p.CurrentHealth);
    }

    [Fact]
    public void IncreaseMaxHealth_RaisesBothMaxAndCurrent()
    {
        var p = new Player();

        p.IncreaseMaxHealth(2);

        Assert.Equal(7, p.MaxHealth);
        Assert.Equal(7, p.CurrentHealth);
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    [Fact]
    public void Update_MovesBySpeedTimesDeltaTime()
    {
        var p = new Player();
        var startX = p.X;

        p.Update(0.5, 1, 0); // speed 200 × 0.5s = 100px

        Assert.Equal(startX + 100, p.X, 1e-9);
    }

    [Fact]
    public void Update_ClampsToBounds_NeverLeavesPlayField()
    {
        var p = new Player();

        p.Update(100, 1, 1); // huge dt pushes far past the edge

        Assert.Equal(GameBounds.Width - p.Radius, p.X);
        Assert.Equal(GameBounds.Height - p.Radius, p.Y);

        p.Update(100, -1, -1);

        Assert.Equal(p.Radius, p.X);
        Assert.Equal(p.Radius, p.Y);
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_RestoresDefaultsAfterUpgradesAndDamage()
    {
        var p = new Player();
        p.IncreaseMaxHealth(3);
        p.TakeDamage();
        p.Speed = 350;
        p.MagnetRange = 200;
        p.HpRegenPerKill = 2;
        p.Update(100, 1, 1);

        p.Reset();

        Assert.Equal(5, p.MaxHealth);
        Assert.Equal(5, p.CurrentHealth);
        Assert.Equal(200, p.Speed);
        Assert.Equal(80, p.MagnetRange);
        Assert.Equal(0, p.HpRegenPerKill);
        Assert.False(p.IsInvincible);
        Assert.Equal(GameBounds.CenterX, p.X);
        Assert.Equal(GameBounds.CenterY, p.Y);
    }

    [Fact]
    public void Reset_HonoursCustomMaxHealthFromDifficulty()
    {
        var p = new Player();

        p.Reset(maxHealth: 3, invincibilityDuration: 2.0);

        Assert.Equal(3, p.MaxHealth);
        Assert.Equal(3, p.CurrentHealth);

        // The custom 2.0s invincibility window is in effect.
        p.TakeDamage();
        p.Update(1.5, 0, 0);
        Assert.True(p.IsInvincible);
        p.Update(0.6, 0, 0);
        Assert.False(p.IsInvincible);
    }
}
