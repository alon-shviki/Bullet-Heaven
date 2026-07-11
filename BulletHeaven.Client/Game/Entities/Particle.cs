namespace BulletHeaven.Client.Game.Entities;

public class Particle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Vx { get; set; }
    public double Vy { get; set; }
    public double Life { get; set; } = 1.0;
    public string Color { get; set; } = "#fff";
    public double Radius { get; set; } = 3;
}
