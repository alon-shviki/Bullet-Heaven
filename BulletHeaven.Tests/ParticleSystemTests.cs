using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Tests;

/// <summary>
/// xUnit tests for the pooled <see cref="ParticleSystem"/>: fixed 256-slot
/// slab (live = Life &gt; 0), Emit initialisation and drop-on-saturation,
/// expired-slot recycling, Update integration/decay, and Clear.
/// </summary>
public class ParticleSystemTests
{
    private static int CountLive(ParticleSystem ps) =>
        ps.Particles.Count(p => p.Life > 0);

    // ── Construction ──────────────────────────────────────────────────────────

    // Happy path: the slab is pre-allocated at 256 slots, all dead.
    [Fact]
    public void Constructor_PreallocatesCapacitySlotsAllDead()
    {
        var ps = new ParticleSystem();

        Assert.Equal(256, ParticleSystem.Capacity);
        Assert.Equal(ParticleSystem.Capacity, ps.Particles.Length);
        Assert.All(ps.Particles, p =>
        {
            Assert.NotNull(p);
            Assert.True(p.Life <= 0);
        });
    }

    // ── Emit ──────────────────────────────────────────────────────────────────

    // Happy path: default burst activates 8 particles.
    [Fact]
    public void Emit_DefaultCount_ActivatesEightParticles()
    {
        var ps = new ParticleSystem();

        ps.Emit(100, 200, "#f00");

        Assert.Equal(8, CountLive(ps));
    }

    // Happy path: emitted particles start at the emit point with full life,
    // the requested colour, and velocity/radius inside the documented ranges.
    [Fact]
    public void Emit_InitialisesParticleState()
    {
        var ps = new ParticleSystem();

        ps.Emit(100, 200, "#0ff", count: 5);

        var live = ps.Particles.Where(p => p.Life > 0).ToList();
        Assert.Equal(5, live.Count);
        Assert.All(live, p =>
        {
            Assert.Equal(100, p.X);
            Assert.Equal(200, p.Y);
            Assert.Equal(1.0, p.Life);
            Assert.Equal("#0ff", p.Color);
            var speed = Math.Sqrt(p.Vx * p.Vx + p.Vy * p.Vy);
            Assert.InRange(speed, 80, 160);
            Assert.InRange(p.Radius, 2, 4);
        });
    }

    // ── Saturation cap ────────────────────────────────────────────────────────

    // Boundary: the live count never exceeds the fixed capacity, no matter
    // how many particles are requested.
    [Fact]
    public void Emit_RequestBeyondCapacity_CapsAt256()
    {
        var ps = new ParticleSystem();

        ps.Emit(0, 0, "#fff", count: 500);

        Assert.Equal(ParticleSystem.Capacity, CountLive(ps));
    }

    // Expected error path: a saturated pool drops new bursts instead of
    // overwriting live slots (no slot regains full life).
    [Fact]
    public void Emit_WhenSaturated_DropsBurstWithoutOverwritingLiveSlots()
    {
        var ps = new ParticleSystem();
        ps.Emit(0, 0, "#fff", count: ParticleSystem.Capacity);
        ps.Update(0.1); // every live slot now has Life = 1 - 0.25 = 0.75

        ps.Emit(50, 50, "#f00", count: 8); // no free slot — must be dropped

        Assert.Equal(ParticleSystem.Capacity, CountLive(ps));
        Assert.DoesNotContain(ps.Particles, p => p.Life == 1.0);
        Assert.DoesNotContain(ps.Particles, p => p.Color == "#f00");
    }

    // Recycling: once particles expire, Emit reuses those slots — same fixed
    // array, live count reflects only the new burst.
    [Fact]
    public void Emit_AfterParticlesExpire_RecyclesExpiredSlots()
    {
        var ps = new ParticleSystem();
        var slab = ps.Particles;
        ps.Emit(0, 0, "#fff", count: ParticleSystem.Capacity);
        ps.Update(1.0); // life 1 - 2.5 → all expired

        ps.Emit(10, 20, "#0f0", count: 12);

        Assert.Equal(12, CountLive(ps));
        Assert.Same(slab, ps.Particles); // still the same pre-allocated slab
    }

    // ── Update ────────────────────────────────────────────────────────────────

    // Happy path: live particles integrate velocity and decay at 2.5/s.
    [Fact]
    public void Update_MovesLiveParticles_AndDecaysLife()
    {
        var ps = new ParticleSystem();
        ps.Emit(100, 100, "#fff", count: 1);
        var p = ps.Particles.First(q => q.Life > 0);
        var (vx, vy) = (p.Vx, p.Vy);

        ps.Update(0.2);

        Assert.Equal(100 + vx * 0.2, p.X, 1e-9);
        Assert.Equal(100 + vy * 0.2, p.Y, 1e-9);
        Assert.Equal(0.5, p.Life, 1e-9); // 1.0 - 0.2 × 2.5
    }

    // Dead slots are skipped — stale state must not drift.
    [Fact]
    public void Update_DeadSlots_AreNotMoved()
    {
        var ps = new ParticleSystem();
        var dead = ps.Particles[0];
        dead.X = 42; dead.Y = 24; dead.Vx = 100; dead.Vy = 100; dead.Life = 0;

        ps.Update(1.0);

        Assert.Equal(42, dead.X);
        Assert.Equal(24, dead.Y);
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    // Restart path: Clear expires every particle.
    [Fact]
    public void Clear_ExpiresAllParticles()
    {
        var ps = new ParticleSystem();
        ps.Emit(0, 0, "#fff", count: 100);

        ps.Clear();

        Assert.Equal(0, CountLive(ps));
    }

    // After Clear the full capacity is available again.
    [Fact]
    public void Clear_MakesFullCapacityAvailableAgain()
    {
        var ps = new ParticleSystem();
        ps.Emit(0, 0, "#fff", count: ParticleSystem.Capacity);
        ps.Clear();

        ps.Emit(0, 0, "#fff", count: ParticleSystem.Capacity);

        Assert.Equal(ParticleSystem.Capacity, CountLive(ps));
    }
}
