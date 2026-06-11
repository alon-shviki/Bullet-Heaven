using BulletHeaven.Client.Game;

namespace BulletHeaven.Client.Game.Entities;

public enum EnemyType { Standard, Runner, Tank, Elite, Boss }

public class Enemy : Entity
{
    public override double Radius { get; set; } = 12;
    public override double Speed  { get; set; } = 80;

    public EnemyType Type         { get; set; }  = EnemyType.Standard;
    public int       MaxHealth    { get; set; }  = 1;
    public int       CurrentHealth { get; set; } = 1;
    public int       KillValue    { get; set; }  = 1;
    public int       XpValue      { get; set; }  = 1;
    public int       ScoreValue   { get; set; }  = 10;
    public double    HitFlash     { get; set; }  = 0;
    public double    OrbHitCooldown { get; set; } = 0;

    /// <summary>Re-initializes a pooled slot for spawning — must reset every stat a previous life could have touched.</summary>
    public void Activate(double x, double y, EnemyType type, double radius, double speed,
                         int maxHealth, int killValue, int xpValue, int scoreValue)
    {
        X             = x;
        Y             = y;
        Type          = type;
        Radius        = radius;
        Speed         = speed;
        MaxHealth     = maxHealth;
        CurrentHealth = maxHealth;
        KillValue     = killValue;
        XpValue       = xpValue;
        ScoreValue    = scoreValue;
        HitFlash      = 0;
        OrbHitCooldown = 0;
        IsAlive       = true;
    }

    public bool TakeDamage(int amount = 1)
    {
        if (!IsAlive) return false;
        CurrentHealth -= amount;
        if (CurrentHealth > 0) return false;
        IsAlive = false;
        return true;
    }

    public void Update(double dt, double targetX, double targetY)
    {
        var (nx, ny) = GameMath.Normalize(targetX - X, targetY - Y);
        X += nx * Speed * dt;
        Y += ny * Speed * dt;
    }
}
