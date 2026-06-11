# Agent Pipeline — mandatory for every `.cs` / `.razor` change

Scope: triggers on `.cs` and `.razor` edits only. Skip entirely for config files, `*.json`, `*.md`, JS, CSS.
Spawn each step via the **Agent tool** before responding to the user.

## Steps
1. **`qa-reviewer`** — pass `ORIGINAL_REQUEST`, `LANGUAGE`, `DRAFT_CODE`.
   Fix every `[FAIL]` item and re-spawn until `[PASS]`.
   Extra flags it applies: game-loop files → heap allocations + per-entity interop; controllers → missing `[Authorize]`, unvalidated input, unhandled exceptions.
2. **`test-generator`** — pass `ORIGINAL_REQUEST`, `LANGUAGE`, `APPROVED_CODE`, `CHANGED_FILES`.
   It maintains `BulletHeaven.Tests` directly: baseline-runs the existing suite, appends tests for the new behavior to the matching `<Class>Tests.cs` (creates the file only if missing), then regression-runs everything.
   On `[TEST_FAIL]` (new defect OR old-test regression): fix the code and restart from step 1. Expect `[TESTS_PASS]` with the file path and added test names.
3. **`docs-generator`** — pass `ORIGINAL_REQUEST`, `LANGUAGE`, `APPROVED_CODE`, `TEST_SUITE` (= the test file path from step 2).
   Collect its full output — it is the body of the final response.
4. **`playwright-e2e`** *(conditional)* — ONLY if the changed file is `.razor` or `Game.Render.cs`. Pass `CHANGED_FILE`, `CHANGE_SUMMARY`. Requires the dev server running (`[E2E_SKIP]` means start it first).
   On `[E2E_FAIL]`: fix the razor/render code and restart from step 1.

## Finish
- Final response to the user = docs-generator output (+ E2E result if step 4 ran).
- Mark tasks complete in `tasks.md` only after the pipeline finishes.
