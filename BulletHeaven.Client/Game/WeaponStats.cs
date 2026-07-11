namespace BulletHeaven.Client.Game;

public class WeaponStats
{
    public double FireRate { get; set; } = 1.5;
    public double BulletSpeed { get; set; } = 400;
    public double BulletRadius { get; set; } = 6;
    public int BulletCount { get; set; } = 1;
    public double SpreadAngle { get; set; } = 0;
    public int Piercing { get; set; } = 0;
    public int OrbCount { get; set; } = 0;
    public double OrbOrbitRadius { get; set; } = 80;
    public double PulseInterval { get; set; } = 0;
    public double PulseRadius { get; set; } = 0;
    public double HomingStrength { get; set; } = 0;
    public int RicochetCount { get; set; } = 0;
    public double ExplosionRadius { get; set; } = 0;
    public double AuraRadius { get; set; } = 0;

}
