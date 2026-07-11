namespace BulletHeaven.Client.Game.Entities;

/// <summary>
/// Pooled particle effects: a fixed slab of particles reused for the whole run.
/// A particle is live while <see cref="Particle.Life"/> &gt; 0; expired slots are
/// recycled by <see cref="Emit"/>. Renderers iterate <see cref="Particles"/> with
/// a raw for-loop and skip slots whose Life ≤ 0.
/// </summary>
public class ParticleSystem
{
    public const int Capacity = 256;

    private readonly Particle[] _particles = new Particle[Capacity];
    private int _cursor;

    public ParticleSystem()
    {
        for (var i = 0; i < Capacity; i++)
            _particles[i] = new Particle { Life = 0 };
    }

    public Particle[] Particles => _particles;

    public void Emit(double x, double y, string color, int count = 8)
    {
        for (var i = 0; i < count; i++)
        {
            var p = RentSlot();
            if (p is null) return; // pool saturated — drop the rest of the burst
            var angle = Random.Shared.NextDouble() * Math.PI * 2;
            var speed = 80 + Random.Shared.NextDouble() * 80;
            p.X = x;
            p.Y = y;
            p.Vx = Math.Cos(angle) * speed;
            p.Vy = Math.Sin(angle) * speed;
            p.Life = 1.0;
            p.Color = color;
            p.Radius = 2 + Random.Shared.NextDouble() * 2;
        }
    }

    public void Update(double dt)
    {
        for (var i = 0; i < _particles.Length; i++)
        {
            var p = _particles[i];
            if (p.Life <= 0) continue;
            p.X += p.Vx * dt;
            p.Y += p.Vy * dt;
            p.Life -= dt * 2.5;
        }
    }

    public void Clear()
    {
        for (var i = 0; i < _particles.Length; i++)
            _particles[i].Life = 0;
        _cursor = 0;
    }

    private Particle? RentSlot()
    {
        for (var i = 0; i < _particles.Length; i++)
        {
            var idx = _cursor + i;
            if (idx >= _particles.Length) idx -= _particles.Length;
            if (_particles[idx].Life <= 0)
            {
                _cursor = idx + 1 == _particles.Length ? 0 : idx + 1;
                return _particles[idx];
            }
        }
        return null;
    }
}
