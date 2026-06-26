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

BulletHeaven.Server/          ← ASP.NET Core API (legacy — not deployed)
  Controllers/
    AuthController.cs         ← was: POST /api/register, POST /api/login
    ScoresController.cs       ← was: POST /api/scores  [Authorize]
    LeaderboardController.cs  ← was: GET /api/leaderboard (public)
  (replaced by portal auth server — see [[Tech/Backend]])

BulletHeaven.Tests/           ← xUnit unit tests
e2e/tests/ui-flows.spec.ts    ← Playwright E2E (UI overlays, not canvas pixels)
```

## Client ↔ Portal

- Client uses `HttpClient` injected into `Game.razor`
- JWT read from `localStorage["jwt"]` (set by portal login) via JS interop
- Score submit: `POST /api/scores` + `Authorization: Bearer <token>` → proxied by BH nginx to portal auth server
- Leaderboard: `GET /api/leaderboard` → proxied to portal
- No CORS needed — nginx proxy makes all calls same-origin from browser's POV

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
