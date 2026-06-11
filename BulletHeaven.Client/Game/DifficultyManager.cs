namespace BulletHeaven.Client.Game;

public class DifficultyManager
{
    private static readonly DifficultySettings Default = new(
        "Survival", SpawnInterval: 1.5, EnemySpeed: 100, PlayerMaxHealth: 5,
        InvincibilityDuration: 1.0, EnemiesPerWave: 2, RampRate: 0.30);

    private readonly DifficultySettings _settings = Default;
    private double _elapsed;

    public string Name                  => _settings.Name;
    public double ElapsedTime           => _elapsed;
    public int    PlayerMaxHealth       => _settings.PlayerMaxHealth;
    public double InvincibilityDuration => _settings.InvincibilityDuration;

    public void Update(double dt) => _elapsed += dt;
    public void Reset()           => _elapsed = 0;

    private double Ramp => 1 + (_elapsed / 60.0) * _settings.RampRate;
    public double CurrentSpawnInterval  => Math.Max(0.3,   _settings.SpawnInterval / Ramp);
    public double CurrentEnemySpeed     => Math.Min(300.0, _settings.EnemySpeed    * Ramp);
    public int    CurrentEnemiesPerWave => Math.Min(10,    _settings.EnemiesPerWave + (int)(_elapsed / 60.0));
}
