### [ENGINE-001] Engine & Stack Decision — COMPLETE

**Decision:**
- **Language:** C# (.NET 8+)
- **Platform:** Blazor WebAssembly — runs entirely in the browser, no server required for gameplay
- **Rendering:** HTML5 `<canvas>` driven from C# via `Blazor.Extensions.Canvas` (JS interop bridge to the 2D context)
- **Game Loop:** `window.requestAnimationFrame` bridged to C# via `DotNetObjectReference` callback
- **Backend (leaderboard/auth):** ASP.NET Core minimal API — **not** Node/Express

**Acceptance Criteria:**
- [x] Engine and language chosen: Blazor WASM + C#
- [x] Rendering strategy chosen: HTML5 Canvas via Blazor.Extensions.Canvas
- [x] Game loop strategy chosen: RAF → JS interop → C# callback
- [x] Backend strategy chosen: ASP.NET Core

---

### [SETUP-001] Establish directory structure and render baseline HTML5 canvas

**Context:**
Starting with a clean slate without heavy frameworks is crucial. It ensures we build our game loops and optimizations from the ground up, making performance bottlenecks easier to identify and debug later.

**Description:**
Set up a clean, zero-dependency project structure consisting of an index.html, styles.css, and an main.js file. Define an HTML5 canvas element with a fixed logical resolution (e.g., 800x600 pixels) centered on the screen via CSS.

**Acceptance Criteria:**
- [ ] The index.html file references styles.css and main.js as an ES module.
- [ ] An HTML5 <canvas> element with an ID of 'gameCanvas' is declared in the HTML.
- [ ] The canvas is styled to be centered vertically and horizontally in the viewport.
- [ ] The browser console displays no errors when loading the index.html page.

**2-Minute Starter Action:**
Run `dotnet new blazorwasm -n BulletHeaven.Client` and add `<canvas id="gameCanvas" width="800" height="600"></canvas>` to `wwwroot/index.html`.

---

### [LOOP-001] Implement requestAnimationFrame Game Loop with Delta Time

**Context:**
A consistent game loop is the heartbeat of any interactive application. Using delta time prevents the game from running too fast on 144Hz monitors or too slow on laggy 30Hz displays.

**Description:**
Write a robust game loop inside main.js using the browser's native requestAnimationFrame API. Calculate delta time (the difference in milliseconds between the current and previous frame) to ensure that game speed remains consistent regardless of the screen's refresh rate.

**Acceptance Criteria:**
- [ ] A mainLoop function is declared and recursively schedules itself via requestAnimationFrame.
- [ ] Delta time is calculated on each frame and logged to a debug overlay or console without flooding memory.
- [ ] The loop calculates and displays the current frames per second (FPS) in the top-left corner of the canvas.
- [ ] A toggle exists to pause and resume the loop execution.

**2-Minute Starter Action:**
In `gameInterop.js` expose a RAF bridge: `window.startGameLoop = (dotnetRef) => { function tick(t) { dotnetRef.invokeMethodAsync('Tick', t).then(() => requestAnimationFrame(tick)); } requestAnimationFrame(tick); };` then call it from `Game.razor` via `IJSRuntime`.

---

### [INPUT-001] Capture keyboard inputs and map to player velocity vector

**Context:**
Smooth movement begins with accurate input capture. Normalizing the diagonal movement vector prevents the player from moving root-2 (~1.41 times) faster when pressing two keys simultaneously.

**Description:**
Create an input handler that listens to 'keydown' and 'keyup' events for movement keys (W, A, S, D, and Arrow Keys). Store the state of these keys in an active object and calculate a normalized 2D movement vector.

**Acceptance Criteria:**
- [ ] Keydown events set the corresponding key in an activeKeys object to true.
- [ ] Keyup events set the corresponding key in an activeKeys object to false.
- [ ] A getMovementVector function returns normalized x and y values between -1 and 1.
- [ ] Diagonal movement (e.g., holding W and D) produces a vector magnitude of exactly 1.0 (or 0 when no keys are pressed).

**2-Minute Starter Action:**
In `gameInterop.js` write: `window.activeKeys = {}; window.addEventListener('keydown', e => activeKeys[e.key.toLowerCase()] = true); window.addEventListener('keyup', e => activeKeys[e.key.toLowerCase()] = false);` and read the state from C# with `await JS.InvokeAsync<bool[]>("getMovementState")`.

---

### [RENDER-001] Draw player entity on canvas and apply velocity boundary checks

**Context:**
Visual feedback is immediate proof of concept. Having a moving shape with boundaries sets the stage for spawning enemies and defining a play arena.

**Description:**
Define a player object with x, y, speed, and radius parameters. Update the player position inside the game loop using the input vector and delta time, drawing a basic geometric shape (e.g., a blue circle) on the canvas. Clamp coordinates so the player cannot move off-screen.

**Acceptance Criteria:**
- [ ] The player object has x, y, radius, and speed properties.
- [ ] The player is drawn on the canvas on every frame using ctx.arc() or a similar rendering API.
- [ ] The player's position changes relative to the normalized input vector multiplied by player speed and delta time.
- [ ] The player cannot move outside the canvas boundaries.

**2-Minute Starter Action:**
Using `Blazor.Extensions.Canvas`, call `await _ctx.BeginPathAsync(); await _ctx.ArcAsync(player.X, player.Y, 15, 0, Math.PI * 2); await _ctx.FillAsync();` inside your render method.

---

### [ENEMY-001] Create enemy entity factory and simple tracking movement behavior

**Context:**
Bullet heaven games require endless waves of tracking enemies. Establishing an off-screen spawning mechanism prevents enemies from instantly materializing on top of the player.

**Description:**
Implement an array to manage active enemies. Write a spawn function that creates a simple red circle (enemy) at a random off-screen location. Update each enemy's position in the game loop so it calculates a vector toward the player's current position and moves along it.

**Acceptance Criteria:**
- [ ] Enemies spawn at dynamic intervals outside the visible boundaries of the canvas.
- [ ] Each enemy calculates the normalized heading angle toward the player's x and y coordinates on every frame.
- [ ] Enemies move along this heading vector toward the player using their own distinct speed variable.
- [ ] Enemies are rendered on screen as distinct shapes (e.g., red circles).

**2-Minute Starter Action:**
Write `Enemy SpawnEnemy() => new Enemy { X = Random.Shared.Next(-100, 0), Y = Random.Shared.Next(-100, 0), Speed = 1.5f };` and add to a `List<Enemy> _enemies`.

---

### [WEAPON-001] Implement automatic projectile weapon targeting nearest enemy

**Context:**
Automated weapons are the defining mechanic of the bullet heaven genre. The player focuses on evasion while weapons auto-target based on distance heuristics.

**Description:**
Implement a weapon cooldown system that fires standard projectiles automatically. The weapon must identify the closest active enemy in the enemies array, compute the trajectory vector from the player to that enemy, and spawn a projectile moving along that vector.

**Acceptance Criteria:**
- [ ] A weapon object maintains a cooldown timer that decrements using delta time.
- [ ] When the cooldown hits zero, the weapon searches the active enemies array to find the enemy with the smallest Euclidean distance to the player.
- [ ] A projectile object is instantiated with a velocity pointing toward that nearest enemy.
- [ ] Projectiles are added to an active projectiles array and are rendered as yellow circles.

**2-Minute Starter Action:**
Write `static float GetDistance(Entity a, Entity b) => MathF.Sqrt(MathF.Pow(b.X - a.X, 2) + MathF.Pow(b.Y - a.Y, 2));` in a `CollisionHelper` static class.

---

### [COLLISION-001] Implement radial collision detection between entities

**Context:**
Accurate hit registration is critical. Overlapping circles is the cheapest computational way to verify hits before we introduce heavy algorithms like Quadtrees.

**Description:**
Create highly optimized collision check routines. Write logic using distance checks (circle-vs-circle) to detect when projectiles hit enemies, and when enemies hit the player. Trigger instant removal of both projectile and enemy upon contact.

**Acceptance Criteria:**
- [ ] A function circlesOverlap(c1, c2) returns true if the distance between their centers is less than the sum of their radii.
- [ ] When a projectile collides with an enemy, both are flagged for destruction and removed from their respective arrays.
- [ ] When an enemy collides with the player, a visual feedback event occurs (e.g., a brief red flash or console log).
- [ ] Off-screen projectiles are culled from memory automatically.

**2-Minute Starter Action:**
Write `bool CirclesOverlap(Circle a, Circle b) => GetDistance(a, b) < a.Radius + b.Radius;` then loop projectiles × enemies candidates and call `Console.WriteLine("Hit!")` when true.

---

### [STATS-001] Implement player health, damage limits, and invulnerability frames

**Context:**
Without damage throttling, overlapping enemies will deplete a player's health pool in a single frame. This introduces essential pacing and survival mechanics.

**Description:**
Add a health pool to the player object and display a health bar above the player's head. When a collision with an enemy is detected, deduct health and activate a short invulnerability frame window (i-frames) to prevent instantaneous death from multi-overlapping enemy hitboxes.

**Acceptance Criteria:**
- [ ] The player object has maxHealth, currentHealth, and isInvincible properties.
- [ ] Colliding with an enemy reduces currentHealth only if isInvincible is false.
- [ ] Taking damage triggers a temporary invincibility cooldown (e.g., 500ms) where the player flashes visually.
- [ ] If currentHealth drops to or below 0, a game over state is triggered, stopping the gameplay loop.

**2-Minute Starter Action:**
Add `bool IsInvincible` and `double InvincibilityTimer` to `Player`. On hit: `IsInvincible = true; InvincibilityTimer = 0.5;` then decrement with delta time each tick and clear the flag when it reaches zero.

---

### [XP-001] Develop XP drops, gem collection, and magnet behavior

**Context:**
The reward loop in survival games depends on picking up currency to grow stronger. Magnet logic adds kinetic satisfying motion to the pickup items.

**Description:**
When an enemy dies, spawn an Experience (XP) gem at its death coordinates. Implement a collection mechanism where gems pull toward the player when they come within a specific magnet range threshold, rewarding XP upon collection.

**Acceptance Criteria:**
- [ ] Defeated enemies add a new gem object to an activeGems array at their exact x and y coordinate.
- [ ] Gems render on screen as small green diamonds or dots.
- [ ] If the distance between the player and a gem is less than player.magnetRange, the gem accelerates toward the player.
- [ ] When a gem overlaps with the player's hitbox, it is destroyed, and player.xp increases.

**2-Minute Starter Action:**
In the update loop: `if (GetDistance(player, gem) < player.MagnetRange) { gem.X += (player.X - gem.X) * 0.1f; gem.Y += (player.Y - gem.Y) * 0.1f; }`

---

### [LEVEL-001] Create centralized state management and trigger level ups

**Context:**
Decoupling game rules from direct loops keeps the architecture clean. A centralized state prevents inconsistent logic across disparate modules.

**Description:**
Design a centralized state manager tracking score, active entities, levels, and XP targets. Build calculations to increase the required XP threshold for successive level-ups, pausing the game loop state and triggering a menu event when a level-up occurs.

**Acceptance Criteria:**
- [ ] A global gameState object tracks score, playerLevel, currentXp, and xpNeededForNextLevel.
- [ ] An increase in currentXp past the threshold increments playerLevel and scales up xpNeededForNextLevel (e.g., * 1.5).
- [ ] Upon leveling up, the game state changes to 'PAUSED_LEVEL_UP'.
- [ ] The normal requestAnimationFrame loops do not execute entity updates while in the pause state.

**2-Minute Starter Action:**
Define `enum GamePhase { Playing, PausedLevelUp }` and `class GameState { public GamePhase Phase; public int Score, PlayerLevel, CurrentXp, XpNeeded = 100; }` in `Game/State/GameState.cs`.

---

### [UI-001] Build UI overlay menu for upgrade selection

**Context:**
Upgrades provide replayability and strategy. Drawing upgrade menus using HTML and DOM elements is much faster and cleaner than drawing complex text interfaces on canvas.

**Description:**
Construct an HTML overlay div that sits directly on top of the game canvas. Make it hidden by default, rendering only when the gameState enters the pause state. Generate three randomized upgrades (e.g., +Max HP, +Speed, +Weapon Damage) for the player to select.

**Acceptance Criteria:**
- [ ] An HTML div overlay exists with an absolute position matching the size and coordinates of the canvas.
- [ ] The overlay becomes visible only when gameState.state === 'PAUSED_LEVEL_UP'.
- [ ] Three distinct clickable button elements are dynamically populated with randomized player upgrade choices.
- [ ] Clicking any option applies the stat change to the player, clears the overlay, and returns the game to the 'PLAYING' state.

**2-Minute Starter Action:**
Add `@if (_gameState.Phase == GamePhase.PausedLevelUp) { <div class="upgrade-overlay"> ... </div> }` in `Game.razor` — Blazor re-renders the overlay automatically when state changes.

---

### [POOL-001] Refactor projectile spawning pipeline to use Object Pooling — COMPLETE

**Context:**
Garbage collection pauses are a death sentence for web performance. Recyclable object pools stop the browser engine from constantly allocating memory on heap and triggering frame drops.

**Description:**
Instead of instantiating and garbage collecting hundreds of bullet objects dynamically, initialize a fixed-size pre-allocated pool of projectile objects. Recycle inactive projectiles from this pool during weapon fire operations.

**Acceptance Criteria:**
- [x] A BulletPool class is implemented containing an array of fixed size (e.g., 500 bullets).
- [x] Firing a bullet retrieves an inactive item from the pool, initializes its coordinates, and marks it active.
- [x] When a bullet goes off-screen or hits an enemy, it is flagged as inactive instead of being spliced from the array.
- [x] No new memory allocations (new Projectile()) occur during active gameplay updates for projectiles.

**2-Minute Starter Action:**
`var _pool = new Bullet[500]; for (int i = 0; i < 500; i++) _pool[i] = new Bullet(); Bullet Get() => Array.Find(_pool, b => !b.Active);`

---

### [POOL-002] Refactor enemy spawning pipeline to use Object Pooling — COMPLETE

**Context:**
With hundreds of monsters spawning and dying every minute, memory thrashing is a massive bottleneck. Extending pooling to enemies completes our baseline memory footprint optimization.

**Description:**
Apply the Object Pool pattern to the enemy pipeline. Pre-allocate an array of enemy objects and recycle them on demand. Deactivating them instead of deleting them stabilizes RAM usage as enemy wave sizes grow.

**Acceptance Criteria:**
- [x] An EnemyPool class manages a maximum of 1,000 pre-instantiated enemy structures.
- [x] The spawnEnemy function retrieves a disabled enemy from the pool and moves it to target off-screen coordinates.
- [x] Defeated enemies are returned to the pool by toggling their active boolean flag to false.
- [x] Memory footprint remains flat while enemies are continuously spawned and destroyed.

**2-Minute Starter Action:**
`foreach (var enemy in _enemyPool) { if (enemy.Active) UpdateEnemy(enemy, dt); }` — no LINQ inside the hot loop; iterate the raw array.

---

### [QUAD-001] Implement a 2D Quadtree class for spatial partitioning — COMPLETE

**Context:**
Comparing every bullet to every enemy has a time complexity of O(N*M). A Quadtree allows us to segment space and perform checks only on elements inside immediate geographic vicinity.

**Description:**
Create a generic, recursive Quadtree class in JavaScript. A Quadtree splits the 2D plane into quadrants, with each node containing a maximum limit of objects. If the limit is exceeded, the node subdivides itself into four child nodes.

**Acceptance Criteria:**
- [x] The Quadtree constructor accepts a boundary object (x, y, width, height) and capacity.
- [x] The insert() function adds elements to the quadtree, partitioning the node if current elements exceed capacity.
- [x] The retrieve() function returns an array of elements stored in all quadrants overlapping with a query boundary (`Query(x, y, radius, results)` fills a reused list).
- [x] The clear() method resets the tree recursively so it can be rebuilt on every frame.

**2-Minute Starter Action:**
`class Quadtree { public Rectangle Boundary; public int Capacity; public List<Entity> Points = new(); public Quadtree[]? Children; }` in `Game/Collision/Quadtree.cs`.

---

### [QUAD-002] Integrate Quadtree spatial partitioning into collision detection — COMPLETE

**Context:**
This integration eliminates redundant collision calculations, bringing complexity down from O(N^2) to roughly O(N log N). This enables thousands of concurrent entities on screen without lag.

**Description:**
Rebuild the spatial Quadtree in the update loop on every frame. Populate it with all active enemies, then query the Quadtree using each projectile's and the player's bounding range to check for localized collisions.

**Acceptance Criteria:**
- [x] At the start of the physics update step, the Quadtree is cleared and rebuilt with all active enemy coordinates.
- [x] The collision check routine queries the Quadtree for candidate enemies matching the projectile's local bounding box.
- [x] Full circle distance checks are executed only on the candidate enemies returned by the Quadtree query.
- [ ] CPU usage profiles in Chrome DevTools show a significant drop in collision computation overhead during heavy load. *(integration verified by review + unit tests; profiling session not yet captured)*

**2-Minute Starter Action:**
`_quadtree.Clear(); foreach (var e in _enemyPool) if (e.Active) _quadtree.Insert(e); var candidates = _quadtree.Retrieve(bullet.Bounds);` then circle-check only `candidates`.

---

### [WORK-001] Create Web Worker setup for background thread computation

**Context:**
The single-threaded nature of JavaScript means heavy math can block rendering, causing visual stuttering. Running physics in a Web Worker keeps the screen refresh rate constant.

**Description:**
Set up a Web Worker (physicsWorker.js) to offload physics calculations and spatial indexing. Configure message passing inside main.js to exchange state payload with the background thread without blocking the browser UI loop.

**Acceptance Criteria:**
- [ ] A separate physicsWorker.js file is loaded from main.js as a native Worker.
- [ ] A secure message interface uses postMessage to transmit coordinate structures.
- [ ] The worker successfully processes a test event and responds with a handshake message.
- [ ] No security or MIME-type errors block the loading of the worker in local development.

**2-Minute Starter Action:**
In `gameInterop.js`: `const worker = new Worker('physicsWorker.js'); worker.onmessage = e => console.log(e.data);` then expose `window.postToWorker = (msg) => worker.postMessage(msg);` for C# to call via `IJSRuntime`.

---

### [WORK-002] Delegate entity tracking and movement updates to the Web Worker

**Context:**
By doing this, the main thread's only job is drawing shapes. Even if thousands of entities are tracking and updating coordinates, frame updates will remain smooth.

**Description:**
Migrate the coordinates calculation, enemy tracking movement, and projectile update loops into the physicsWorker.js thread. Send input states and timing flags from the main thread, and receive updated coordinates arrays back for rendering.

**Acceptance Criteria:**
- [ ] The main thread sends player coordinate changes and keyboard updates to the worker thread via postMessage.
- [ ] The Web Worker processes the movement math and executes Quadtree collision routines internally.
- [ ] The Web Worker posts a clean rendering package of active coordinates back to the main thread.
- [ ] The main thread consumes the payload and renders the entities without processing physics calculations.

**2-Minute Starter Action:**
In `physicsWorker.js`: `self.onmessage = e => { const result = runPhysics(e.data); self.postMessage({ type: 'TICK_REPLY', entities: result }); };` C# receives the payload via an interop callback and feeds it straight to the canvas renderer.

---

### [BACK-001] Setup ASP.NET Core Web API skeleton with database connection

**Context:**
To support database features like logins, persistent profiles, and global leaderboards, a secure full-stack backend service is required. The backend is a separate ASP.NET Core project (`BulletHeaven.Server`) that the Blazor WASM client calls via HTTP.

**Description:**
Create a minimal ASP.NET Core Web API with CORS, HTTPS, and EF Core (PostgreSQL or SQLite). Establish a database connection and expose a `/health` endpoint to verify connectivity. Use `dotnet user-secrets` for credentials in development.

**Acceptance Criteria:**
- [ ] An ASP.NET Core server listening on an environment port (e.g., 5000/5001) runs without crashing.
- [ ] An API endpoint GET /health returns status 200.
- [ ] EF Core `DbContext` connects successfully and migrations apply cleanly.
- [ ] Secrets (connection strings, JWT key) are managed via `dotnet user-secrets` / environment variables, not checked in.

**2-Minute Starter Action:**
Run `dotnet new webapi -n BulletHeaven.Server` then add `app.MapGet("/health", () => Results.Ok(new { status = "ok" }));` in `Program.cs`. Use `dotnet user-secrets` for DB credentials.

---

### [AUTH-001] Develop registration and login JWT APIs with secure database schemas

**Context:**
Authentication guarantees that leaderboard scores are authenticated and player achievements cannot be falsified easily.

**Description:**
Build user validation and registration backend APIs. Securely hash user passwords with bcrypt before saving to the database, and configure a POST /login endpoint that issues JSON Web Tokens (JWT) for authentication.

**Acceptance Criteria:**
- [ ] A database schema contains unique fields for username and securely hashed passwords.
- [ ] POST /api/register creates a new user, responding with an error if the username is taken.
- [ ] POST /api/login checks bcrypt credentials and responds with a signed JWT token on success.
- [ ] Invalid credentials return an explicit HTTP 401 Unauthorized status.

**2-Minute Starter Action:**
Add NuGet packages `BCrypt.Net-Next` and `Microsoft.AspNetCore.Authentication.JwtBearer`. Define a `User` EF Core entity with `Username` and `PasswordHash` string properties.

---

### [AUTH-002] Build frontend login and registration UI panel overlay

**Context:**
Integrating a UI on top of the game loops lets users log in seamlessly and view personalized stats without navigating away from the game screen.

**Description:**
Create registration and login forms in HTML that slide over the main canvas. Add standard token storage (localStorage) in main.js, allowing players to authenticate before starting a run.

**Acceptance Criteria:**
- [ ] An HTML overlay screen displays login and registration input forms.
- [ ] Submit handlers trigger POST fetch requests to the authentication backend endpoints.
- [ ] A successful response saves the returned JWT securely in localStorage.
- [ ] When a token is detected, the form overlay hides itself, revealing the game UI and player dashboard.

**2-Minute Starter Action:**
In `Game.razor`, add `@inject HttpClient Http` and handle the form with `async Task HandleLogin() { var res = await Http.PostAsJsonAsync("/api/login", credentials); var token = await res.Content.ReadAsStringAsync(); await JS.InvokeVoidAsync("localStorage.setItem", "jwt", token); }`

---

### [LEADER-001] Develop verified Save Score API endpoint

**Context:**
High scores drive replayability. Protecting score submission APIs with JWT validations stops users from spoofing high-score payloads via simple terminal requests.

**Description:**
Design a secure endpoint POST /api/scores to store run statistics (XP collected, run survival time, monsters killed). Use standard authorization middleware to parse and verify the JWT header before recording the data.

**Acceptance Criteria:**
- [ ] A Score model tracks score, time, date, and user reference object.
- [ ] A custom middleware authenticates incoming JWTs from the Authorization header.
- [ ] POST /api/scores stores the run details in the database associated with the active user.
- [ ] An unauthenticated request to POST /api/scores returns an HTTP 401 error.

**2-Minute Starter Action:**
Decorate the scores endpoint with `[Authorize]` and register JWT bearer auth in `Program.cs` via `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`.

---

### [LEADER-002] Build leaderboard panel components on frontend

**Context:**
Leaderboards build community engagement. Comparing performance with other real-world players transforms a simple prototype into a highly competitive platform.

**Description:**
Implement an API endpoint GET /api/leaderboard that retrieves the top 10 highest-scoring game records. Create a stylish leaderboard interface overlay that populates dynamically using the API's returned data.

**Acceptance Criteria:**
- [ ] The backend GET /api/leaderboard retrieves the top 10 scores sorted in descending order.
- [ ] The endpoint returns structured JSON containing usernames and score values.
- [ ] A 'Leaderboard' HTML overlay component renders when the user clicks 'View Scores' or dies.
- [ ] The leaderboard dynamic content accurately updates without requiring a page reload.

**2-Minute Starter Action:**
In `Leaderboard.razor`: `var scores = await Http.GetFromJsonAsync<ScoreDto[]>("/api/leaderboard");` then render with `@foreach (var s in scores) { <tr><td>@s.Username</td><td>@s.Score</td></tr> }`.