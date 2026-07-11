# Architecture

## Stack
Blazor WASM (.NET 10) client — **client-only**, the portal auth server owns all backend concerns. Canvas rendering via `Blazor.Extensions.Canvas`. Game loop: `requestAnimationFrame` in `gameInterop.js` → JS interop → C# tick callback. No game engine (no Unity/Godot/MonoGame).

## Codebase map
| Path | Owns |
|---|---|
| `BulletHeaven.Client/Pages/Game.razor` | All UI overlays (menu, level-up, game over, login, leaderboard, codex) + the `GameState` machine + game orchestration |
| `BulletHeaven.Client/Pages/Game.Render.cs` | All canvas drawing — partial class of Game.razor |
| `BulletHeaven.Client/Game/` | `GameLoop`, `GameMath`, `GameBounds`, `DifficultyManager`, `WeaponStats`, `ICollidable` |
| `BulletHeaven.Client/Game/Entities/` | `Player`, `Enemy`, `EnemySpawner`, `Projectile`, `Weapon` + secondaries (`OrbWeapon`, `PulseWeapon`, `AuraWeapon`), `XpGem`, `Particle`/`ParticleSystem` |
| `BulletHeaven.Client/Game/Upgrades/` | `UpgradeCatalogue` (weighted Common/Rare/Epic), `UpgradeDefinition` |
| `BulletHeaven.Client/Game/Input/InputHandler.cs` | Keyboard state → normalized movement vector |
| `BulletHeaven.Client/wwwroot/js/gameInterop.js` | RAF bridge + key listeners — the only JS file |
| `BulletHeaven.Tests/` | xUnit unit tests |
| `e2e/tests/ui-flows.spec.ts` | Playwright UI-flow tests (no canvas-pixel assertions) |

## State machine
`GameState` is a **private enum inside Game.razor** (~line 325):
`MainMenu | Playing | PausedLevelUp | GameOver | Codex | Login | Leaderboard`

- The RAF tick still fires in every state but skips entity updates unless `Playing`.
- Level-up sets `PausedLevelUp`; picking an upgrade returns to `Playing`.

## UI conventions
- Menus/overlays are Blazor HTML positioned over the canvas — never drawn on canvas.
- UI state changes go through `_state = GameState.X; StateHasChanged();`.
- Gameplay HUD (health bar, score, FPS) is drawn on canvas in `Game.Render.cs`.

## Client↔Portal
- Client calls `/api/scores` and `/api/leaderboard` with `HttpClient`; BH's nginx proxies them to the portal auth server (see `.claude/rules/backend.md`).
- JWT arrives from the portal via URL hash (`#portal_token=…`), stored in `localStorage` via JS interop; attached as `Authorization: Bearer` header.
