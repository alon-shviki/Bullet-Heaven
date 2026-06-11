namespace BulletHeaven.Client.Game;

public record DifficultySettings(
    string Name,
    double SpawnInterval,
    double EnemySpeed,
    int    PlayerMaxHealth,
    double InvincibilityDuration,
    int    EnemiesPerWave,
    double RampRate
);
