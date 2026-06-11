---
name: verify-game
description: Build, run, and visually verify Bullet Heaven in a real browser. Use after gameplay/UI changes, or when the user asks to run the game, see it working, or take a screenshot.
---

# Verify the game end-to-end

1. **Build first** (fail fast): `dotnet build BulletHeaven.slnx --nologo -v q`
2. **Start dev server** if not already running (check `curl -s -o /dev/null -w '%{http_code}' http://localhost:5292`):
   `cd BulletHeaven.Client && dotnet watch` as a background task; wait for HTTP 200.
3. **Drive the browser** with the `playwright-cli` skill against `http://localhost:5292`:
   - Main menu shows BULLET HEAVEN + Start Game / Leaderboard / Archive buttons.
   - Click Start Game → canvas gameplay begins (HUD: health, score, FPS).
   - For UI changes: navigate to the changed overlay and screenshot it.
   - Canvas pixel content can't be asserted — verify game state via the HTML HUD/overlays and screenshots.
4. **Run the suites** when the change warrants it:
   - Logic: `dotnet test BulletHeaven.Tests`
   - UI flows: `cd e2e && npx playwright test` (needs the dev server up)
5. **Report**: state what was verified with the screenshot(s), and anything that looked wrong.

Full stack (auth/leaderboard verification) needs the API + DB: `docker compose up --build` → http://localhost:8080.
