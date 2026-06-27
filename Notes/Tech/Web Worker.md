# Web Worker

Physics offload: separates heavy per-frame math from the browser render thread so frame rate stays smooth under high entity counts.

## Setup (WORK-001, done)

| File | Role |
|------|------|
| `wwwroot/js/physicsWorker.js` | Worker script; runs in its own thread |
| `wwwroot/js/gameInterop.js` | Loads the worker on startup; exposes `window.postToWorker` |

`gameInterop.js` initialises the worker once at load time:

```js
const _physicsWorker = new Worker('js/physicsWorker.js');
_physicsWorker.onmessage = e => console.log('[physicsWorker]', e.data);
window.postToWorker = msg => _physicsWorker.postMessage(msg);
```

C# calls it via IJSRuntime:

```csharp
await JS.InvokeVoidAsync("postToWorker", new { type = "handshake" });
```

The worker responds with `{ type: "handshake-ack", ok: true }`.

## Constraint

Blazor WASM can't call `new Worker()` from C# — it must go through a JS interop bridge. `gameInterop.js` is that bridge.

## Pending (WORK-002)

Entity state (positions, velocities) is passed to the worker each frame via `postMessage`. The worker runs collision detection and spatial indexing, then posts results back. See GitHub issue #5.

## Related

- [[Tech/Performance]] — frame budget rules the worker is designed to protect
- [[Tech/Architecture]] — how gameInterop.js fits into the client stack
