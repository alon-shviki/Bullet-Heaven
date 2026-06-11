using BulletHeaven.Client.Game;

namespace BulletHeaven.Client.Game.Entities;

public class Projectile : Entity
{
    public double Vx           { get; set; }
    public double Vy           { get; set; }
    public int    PiercingLeft  { get; set; } = 0;
    public int    RicochetLeft  { get; set; } = 0;

    public override double Radius { get; set; } = 6;
    public override double Speed  { get; set; } = 400; // px/sec

    /// <summary>Re-initializes a pooled slot for firing — resets every stat a previous shot could have touched.</summary>
    public void Activate(double x, double y, double vx, double vy,
                         double speed, double radius, int piercing, int ricochet)
    {
        X            = x;
        Y            = y;
        Vx           = vx;
        Vy           = vy;
        Speed        = speed;
        Radius       = radius;
        PiercingLeft = piercing;
        RicochetLeft = ricochet;
        IsAlive      = true;
    }

    public void Update(double dt)
    {
        X += Vx * Speed * dt;
        Y += Vy * Speed * dt;
    }

    public void ApplyHoming(double homingStrength, double targetX, double targetY)
    {
        var (nx, ny) = GameMath.Normalize(targetX - X, targetY - Y);
        Vx += (nx - Vx) * homingStrength;
        Vy += (ny - Vy) * homingStrength;
        (Vx, Vy) = GameMath.Normalize(Vx, Vy);
    }
}
