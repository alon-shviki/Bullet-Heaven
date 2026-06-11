using BulletHeaven.Client.Game;

namespace BulletHeaven.Client.Game.Entities;

public class Player : Entity
{
    public override double Radius { get; set; } = 15;
    public override double Speed { get; set; } = 200; // px/sec

    public int    MaxHealth    { get; private set; } = 5;
    public int    CurrentHealth { get; private set; }
    public bool   IsInvincible  { get; private set; }
    public bool   IsDead        => CurrentHealth <= 0;
    public double MagnetRange   { get; set; } = 80;
    public int    HpRegenPerKill { get; set; } = 0;

    private double _invincibilityTimer;
    private double _invincibilityDuration = 1.0;

    public Player() => Reset();

    public void Update(double dt, double vx, double vy)
    {
        TickInvincibility(dt);
        X = Math.Clamp(X + vx * Speed * dt, Radius, GameBounds.Width  - Radius);
        Y = Math.Clamp(Y + vy * Speed * dt, Radius, GameBounds.Height - Radius);
    }

    public void Heal(int amount) => CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);

    public void IncreaseMaxHealth(int n) { MaxHealth += n; CurrentHealth += n; }

    public void AddInvincibilityDuration(double s) => _invincibilityDuration += s;

    public void TakeDamage()
    {
        if (IsInvincible || IsDead) return;
        CurrentHealth--;
        IsInvincible = true;
        _invincibilityTimer = _invincibilityDuration;
    }

    public void Reset(int maxHealth = 5, double invincibilityDuration = 1.0)
    {
        _invincibilityDuration = invincibilityDuration;
        MaxHealth = maxHealth;
        X = GameBounds.CenterX; Y = GameBounds.CenterY;
        CurrentHealth = MaxHealth;
        IsInvincible = false;
        _invincibilityTimer = 0;
        IsAlive = true;
        MagnetRange    = 80;
        HpRegenPerKill = 0;
        Speed          = 200;
    }

    private void TickInvincibility(double dt)
    {
        if (!IsInvincible) return;
        _invincibilityTimer -= dt;
        if (_invincibilityTimer <= 0) IsInvincible = false;
    }
}
