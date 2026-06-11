namespace BulletHeaven.Client.Game.Entities;

public class XpGem : Entity
{
    public override double Radius { get; set; } = 7;
    public override double Speed  { get; set; } = 0;
    public int Value { get; set; } = 1;

    /// <summary>Re-initializes a pooled slot at a kill location.</summary>
    public void Activate(double x, double y, int value = 1)
    {
        X       = x;
        Y       = y;
        Value   = value;
        IsAlive = true;
    }

    public void Update(double dt, double px, double py, double magnetRange)
    {
        var dist = GameMath.Distance(X, Y, px, py);
        if (dist > magnetRange || dist < 0.1) return;
        var spd          = Math.Max(120.0, 300.0 * (1 - dist / magnetRange));
        var (nx, ny)     = GameMath.Normalize(px - X, py - Y);
        X += nx * spd * dt;
        Y += ny * spd * dt;
    }
}
