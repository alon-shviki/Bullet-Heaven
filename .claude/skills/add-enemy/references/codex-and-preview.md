# Codex card + preview canvas — exact integration

Adding an enemy requires THREE visual integrations: the in-game draw (C#), the codex card (HTML), and the codex preview (JS). The JS preview must mirror the C# draw 1:1.

## 1. Codex card HTML (Game.razor, enemies tab ~line 147)
Cards are hardcoded — copy this template, keep stats in sync with `EnemySpawner.SpawnOne`:

```razor
<div class="enemy-card">
    <div class="enemy-card-top">
        <canvas data-entity="NEWNAME" width="90" height="90" class="enemy-canvas"></canvas>
        <div><div class="enemy-name">NewName</div><div class="enemy-tier">Common|Rare|Elite</div></div>
    </div>
    <div class="enemy-desc">One-line flavor text with a tactical hint.</div>
    <div class="enemy-stats">
        <span>HP</span><b>?</b>
        <span>Speed</span><b>?× normal</b>
        <span>XP</span><b>?</b>
        <span>Score</span><b>?</b>
        <span>Spawn</span><b>?% chance</b>
    </div>
</div>
```

## 2. JS preview (wwwroot/js/gameInterop.js)
`drawEntityPreviews()` (~line 56) dispatches on `data-entity` — add a branch:
```js
if (type === 'newname') _drawNewName(ctx, cx, cy);
```
Then add `function _drawNewName(ctx, x, y) { ... }` next to `_drawStandard`/`_drawRunner`/`_drawTank` — these mirror `Game.Render.cs` shapes 1:1 (same radii, same hex colors). Plain canvas 2D, no batching needed here (codex is not the game loop).

Preview rendering is triggered from `Game.razor` (~line 420) when `_state == GameState.Codex && _codexTab == "enemies"` — no extra wiring needed if you reuse `data-entity`.

## 3. Existing palette (keep new colors distinct)
| Type | Fill | Accent |
|---|---|---|
| Standard | `#8a5cd0` purple hexagon | `#371c5e` stroke |
| Runner | `#f5a623` orange 8-point star | `#6e3d00` stroke |
| Tank | purple octagon (see `_drawTank`) | — |
| Elite | yellow `#eab308` | — |
| Boss | red `#dc2626` | — |

Color map for in-game effects: `Game.razor` ~line 912 (`EnemyType.X => "#hex"`).
