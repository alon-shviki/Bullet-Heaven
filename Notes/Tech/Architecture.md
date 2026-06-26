# Architecture

## Project Structure

```
BulletHeaven.Client/          ← Blazor WASM game
  Pages/
    Game.razor                ← state machine + all HTML overlays
    Game.Render.cs            ← all canvas draw calls (partial class)
  Game/
    GameLoop.cs               ← tick orchestration, delta time
    GameMath.cs               ← distance, normalize, clamp helpers
    GameBounds.cs             ← canvas dimensions constants
    DifficultyManager.cs      ← wave scaling over time
    DifficultySettings.cs     ← per-phase spawn/speed config
    WeaponStats.cs            ← all weapon stat fields
    ICollidable.cs            ← circle collision interface
    Entities/
      Entity.cs               ← base: X, Y, Radius, Speed, Active
      Player.cs
      Enemy.cs
      EnemySpawner.cs
      Projectile.cs
      Weapon.cs               ← primary auto-targeting weapon
      OrbWeapon.cs            ← orbiting damage orbs
      PulseWeapon.cs          ← timed area explosion
      AuraWeapon.cs           ← passive damage field
      XpGem.cs
      Particle.cs
      ParticleSystem.cs
    Pools/
      EntityPool.cs           ← generic fixed-size pool base
      BulletPool.cs           ← 500 pre-allocated projectiles
      EnemyPool.cs            ← 1000 pre-allocated enemies
    Collision/
      Quadtree.cs             ← spatial partitioning, rebuilt every frame
    Upgrades/
      UpgradeCatalogue.cs     ← weighted random draw, eligibility checks
      UpgradeDefinition.cs    ← id, name, rarity, weight, apply fn, eligible fn
    Input/
      InputHandler.cs         ← keydown/keyup → normalized movement vector
  wwwroot/js/
    gameInterop.js            ← RAF bridge + key listeners (only JS file)

BulletHeaven.Server/          ← ASP.NET Core API
  Controllers/
    AuthController.cs         ← POST /api/register, POST /api/login
    ScoresController.cs       ← POST /api/scores  [Authorize]
    LeaderboardController.cs  ← GET /api/leaderboard (public)
  Models/
    User.cs                   ← Id, Username, PasswordHash
    Score.cs                  ← Id, UserId, Value, CreatedAt
  Data/
    AppDbContext.cs            ← EF Core DbContext (PostgreSQL)
    AppDbContextFactory.cs    ← design-time factory for migrations
  Program.cs                  ← DI wiring, JWT config, CORS, Swagger

BulletHeaven.Tests/           ← xUnit unit tests
e2e/tests/ui-flows.spec.ts    ← Playwright E2E (UI overlays, not canvas pixels)
```

## Client ↔ Server

- Client uses `HttpClient` injected into `Game.razor`
- JWT stored in `localStorage` via JS interop
- Every score submit: `Authorization: Bearer <token>` header
- CORS locked to `http://localhost:5292` in dev, nginx-proxied origin in prod

## Game Loop — JS → C# Bridge

```js
// gameInterop.js
window.startGameLoop = (dotnetRef) => {
  function tick(t) {
    dotnetRef.invokeMethodAsync('Tick', t).then(() => requestAnimationFrame(tick));
  }
  requestAnimationFrame(tick);
};
```

C# side: `[JSInvokable] public async Task Tick(double timestamp)` in `Game.razor`.

## State Machine

See [[Design/Core Loop]] for the full state diagram.

`_state` changes always call `StateHasChanged()` to trigger Blazor re-render of the correct HTML overlay.

## Rendering

`Game.Render.cs` is a partial class of `Game.razor`. All canvas calls are batched in one `BeginBatchAsync` / `EndBatchAsync` block per frame. Zero per-entity JS interop round trips.

Draw order: background → gems → particles → enemies → orbs → player → HUD (health bar, score, FPS, timer).
