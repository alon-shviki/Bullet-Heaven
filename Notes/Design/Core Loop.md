# Core Loop

## Game Flow

```
Main Menu
  ↓  Start
Playing  ←──────────────────────┐
  ↓  XP threshold reached       │
PausedLevelUp                   │
  ↓  Upgrade picked             │
Playing  ───────────────────────┘
  ↓  Player HP = 0
Game Over
  ↓  Submit score → Leaderboard
Main Menu
```

## State Machine

Defined as a **private enum inside `Game.razor`** (~line 349):

```csharp
private enum GameState { MainMenu, Playing, PausedLevelUp, GameOver, Codex, Login, Leaderboard }
private GameState _state = GameState.MainMenu;
```

The RAF tick fires in every state but entity updates only run in `Playing`. All other states call `Render()` only (to draw the static background/HUD while the overlay is visible).

## Tick Pipeline

Every frame:

```
requestAnimationFrame (gameInterop.js)
  → JS interop → C# Tick(double timestamp)
      1. Calculate dt (delta time in seconds)
      2. if not Playing → Render() only, return
      3. InputHandler.GetMovementVector()
      4. Player.Update(dt, vx, vy)
      5. EnemySpawner.Update(dt)         ← spawns from EnemyPool
      6. Weapon.Update(dt)               ← fires from BulletPool
      7. OrbWeapon / PulseWeapon / AuraWeapon .Update(dt)
      8. Projectile updates (BulletPool)
      9. Quadtree.Clear() + rebuild from active enemies
     10. Collision: projectile × Quadtree candidates, player × enemies
     11. XpGem magnet + collection
     12. ParticleSystem.Update(dt)
     13. DifficultyManager.Update(dt)
     14. Render()  ← one BeginBatchAsync…EndBatchAsync
```

## XP and Leveling

- Enemies drop XP gems on death
- Gems with `magnetRange` of player (default 80 px, upgradeable) move toward the player
- XP threshold starts at 100, scales per level (×1.5 each level)
- On threshold reached: `_state = GameState.PausedLevelUp`, `UpgradeCatalogue.Roll3()` picks 3 distinct upgrades
- Player picks one → upgrade applied → `_state = GameState.Playing`

## Score

- Each enemy kill adds `enemy.ScoreValue` to score
- Final score submitted to `POST /api/scores` (JWT required) on game over
- Leaderboard shows top 10 globally
