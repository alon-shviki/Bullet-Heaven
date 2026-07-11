using BulletHeaven.Client.Game;

namespace BulletHeaven.Client.Game.Entities;

public abstract class Entity : ICollidable
{
    public double X { get; set; }
    public double Y { get; set; }
    public abstract double Radius { get; set; }
    public abstract double Speed { get; set; }

    public bool IsAlive { get; set; } = true;

    public bool IsOffScreen() =>
        X < -Radius || X > GameBounds.Width + Radius ||
        Y < -Radius || Y > GameBounds.Height + Radius;
}
