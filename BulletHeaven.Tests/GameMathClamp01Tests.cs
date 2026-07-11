using BulletHeaven.Client.Game;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for <see cref="GameMath.Clamp01"/>.
/// Covers: happy path, boundary conditions, and edge cases (NaN, infinity, negative zero).
/// No external dependencies — Math.Clamp is BCL.
/// </summary>
public class GameMathClamp01Tests
{
    // ── Happy Path ────────────────────────────────────────────────────────────

    // Happy Path: mid-range value already in [0,1] passes through unchanged.
    [Fact]
    public void Clamp01_ValueInRange_ReturnsUnchanged()
    {
        float result = GameMath.Clamp01(0.5f);
        Assert.Equal(0.5f, result);
    }

    // Happy Path: quarter value passes through unchanged.
    [Fact]
    public void Clamp01_QuarterValue_ReturnsUnchanged()
    {
        float result = GameMath.Clamp01(0.25f);
        Assert.Equal(0.25f, result);
    }

    // Happy Path: three-quarter value passes through unchanged.
    [Fact]
    public void Clamp01_ThreeQuarterValue_ReturnsUnchanged()
    {
        float result = GameMath.Clamp01(0.75f);
        Assert.Equal(0.75f, result);
    }

    // ── Boundary Conditions ───────────────────────────────────────────────────

    // Boundary: exact lower bound returns 0.
    [Fact]
    public void Clamp01_ExactlyZero_ReturnsZero()
    {
        float result = GameMath.Clamp01(0f);
        Assert.Equal(0f, result);
    }

    // Boundary: exact upper bound returns 1.
    [Fact]
    public void Clamp01_ExactlyOne_ReturnsOne()
    {
        float result = GameMath.Clamp01(1f);
        Assert.Equal(1f, result);
    }

    // Boundary: smallest positive float below 1 passes through unchanged.
    [Fact]
    public void Clamp01_EpsilonAboveZero_ReturnsUnchanged()
    {
        float value = float.Epsilon;
        float result = GameMath.Clamp01(value);
        Assert.Equal(value, result);
    }

    // Boundary: value just below 1 by one ULP passes through unchanged.
    [Fact]
    public void Clamp01_JustBelowOne_ReturnsUnchanged()
    {
        // BitDecrement gives the largest float strictly less than 1.
        float value = MathF.BitDecrement(1f);
        float result = GameMath.Clamp01(value);
        Assert.Equal(value, result);
    }

    // Boundary: value just above 0 by one ULP passes through unchanged.
    [Fact]
    public void Clamp01_JustAboveZero_ReturnsUnchanged()
    {
        float value = MathF.BitIncrement(0f);
        float result = GameMath.Clamp01(value);
        Assert.Equal(value, result);
    }

    // Boundary: value just below 0 (tiny negative) is clamped to 0.
    [Fact]
    public void Clamp01_JustBelowZero_ReturnsZero()
    {
        float value = MathF.BitDecrement(0f); // -float.Epsilon
        float result = GameMath.Clamp01(value);
        Assert.Equal(0f, result);
    }

    // Boundary: value just above 1 by one ULP is clamped to 1.
    [Fact]
    public void Clamp01_JustAboveOne_ReturnsOne()
    {
        float value = MathF.BitIncrement(1f);
        float result = GameMath.Clamp01(value);
        Assert.Equal(1f, result);
    }

    // ── Below-Range Clamping ──────────────────────────────────────────────────

    // Below-range: small negative value clamped to 0.
    [Fact]
    public void Clamp01_SmallNegative_ReturnsZero()
    {
        float result = GameMath.Clamp01(-0.1f);
        Assert.Equal(0f, result);
    }

    // Below-range: large negative value clamped to 0.
    [Fact]
    public void Clamp01_LargeNegative_ReturnsZero()
    {
        float result = GameMath.Clamp01(-1000f);
        Assert.Equal(0f, result);
    }

    // ── Above-Range Clamping ──────────────────────────────────────────────────

    // Above-range: value slightly above 1 clamped to 1.
    [Fact]
    public void Clamp01_SlightlyAboveOne_ReturnsOne()
    {
        float result = GameMath.Clamp01(1.1f);
        Assert.Equal(1f, result);
    }

    // Above-range: large positive value clamped to 1.
    [Fact]
    public void Clamp01_LargePositive_ReturnsOne()
    {
        float result = GameMath.Clamp01(1000f);
        Assert.Equal(1f, result);
    }

    // ── Edge Cases: Infinity ──────────────────────────────────────────────────

    // Edge case: positive infinity clamped to 1.
    [Fact]
    public void Clamp01_PositiveInfinity_ReturnsOne()
    {
        float result = GameMath.Clamp01(float.PositiveInfinity);
        Assert.Equal(1f, result);
    }

    // Edge case: negative infinity clamped to 0.
    [Fact]
    public void Clamp01_NegativeInfinity_ReturnsZero()
    {
        float result = GameMath.Clamp01(float.NegativeInfinity);
        Assert.Equal(0f, result);
    }

    // ── Edge Cases: NaN ───────────────────────────────────────────────────────

    // Edge case: NaN propagates through Math.Clamp (BCL contract: NaN input yields NaN).
    // This documents the actual behaviour of Math.Clamp so callers know what to expect.
    [Fact]
    public void Clamp01_NaN_ReturnsNaN()
    {
        float result = GameMath.Clamp01(float.NaN);
        Assert.True(float.IsNaN(result),
            $"Expected NaN but got {result}. Math.Clamp propagates NaN; callers must guard against NaN inputs.");
    }

    // ── Edge Cases: Negative Zero ─────────────────────────────────────────────

    // Edge case: negative zero is within [0,1] and must be returned as-is (or +0);
    // either is acceptable — we verify the result compares equal to 0.
    [Fact]
    public void Clamp01_NegativeZero_ReturnsZero()
    {
        float negZero = -0f;
        float result = GameMath.Clamp01(negZero);
        // -0f == +0f in IEEE 754; both are valid clamped outputs.
        Assert.Equal(0f, result);
    }

    // ── Theory: InlineData sweep ──────────────────────────────────────────────

    // Theory (happy path + boundaries): parametrised sweep of representative values.
    [Theory]
    [InlineData(0f, 0f)]    // lower boundary
    [InlineData(0.25f, 0.25f)] // quarter
    [InlineData(0.5f, 0.5f)]  // midpoint
    [InlineData(0.75f, 0.75f)] // three-quarter
    [InlineData(1f, 1f)]    // upper boundary
    [InlineData(-1f, 0f)]    // below-range
    [InlineData(2f, 1f)]    // above-range
    [InlineData(-100f, 0f)]    // large negative
    [InlineData(100f, 1f)]    // large positive
    public void Clamp01_Theory_ReturnsExpected(float input, float expected)
    {
        Assert.Equal(expected, GameMath.Clamp01(input));
    }
}
