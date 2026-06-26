# Difficulty & Wave Scaling

**Classes:** `DifficultyManager.cs`, `DifficultySettings.cs`

## How It Works

`DifficultyManager` tracks elapsed game time and advances through a series of `DifficultySettings` records. Each record defines the parameters for one phase of the game. `EnemySpawner` reads the current settings each tick.

## DifficultySettings Fields

| Field | Type | Description |
|-------|------|-------------|
| SpawnInterval | double | Seconds between enemy spawns |
| EnemySpeed | double | Base movement speed for enemies in this phase |
| PlayerMaxHealth | int | (reference only — player HP is set at game start) |
| EnemiesPerWave | int | How many enemies spawn per interval |

## General Scaling Pattern

As time increases:
- `SpawnInterval` decreases (faster spawns)
- `EnemySpeed` increases (enemies move faster)
- `EnemiesPerWave` increases (more per spawn burst)
- Higher `EnemyType` variants (Runner, Tank, Elite, Boss) introduced

Elites and Bosses appear at milestone intervals, requiring more hits and giving more XP/score.

## Spawn Position

Enemies always spawn outside the visible canvas bounds, at a random edge position, so they approach from off-screen. Guarantees no instant overlap with the player on spawn.
