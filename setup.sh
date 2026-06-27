#!/usr/bin/env bash
# New machine setup for Bullet Heaven.
# Shared agentic scripts (start-issue, finish-issue, auto-pr) live in the portal repo.
# This script wires them into ~/.local/bin/ from the portal, then wires any BH-specific scripts too.
set -e

PORTAL="${PORTAL_ROOT:-$HOME/Desktop/game}"
BH_SCRIPTS="$(cd "$(dirname "$0")/.claude/scripts" 2>/dev/null && pwd || true)"

if [ ! -d "$PORTAL/.claude/scripts" ]; then
  echo "Portal not found at $PORTAL"
  echo "Clone it first: git clone git@github.com:alon-shviki/game-portal.git ~/Desktop/game"
  echo "Then re-run this script, or set PORTAL_ROOT=/path/to/game bash setup.sh"
  exit 1
fi

mkdir -p ~/.local/bin

# Shared scripts from portal
for s in "$PORTAL/.claude/scripts"/*; do
  ln -sf "$s" ~/.local/bin/"$(basename "$s")"
  echo "linked: $(basename "$s")"
done

# BH-specific scripts (if any)
if [ -n "$BH_SCRIPTS" ]; then
  for s in "$BH_SCRIPTS"/*; do
    [ -e "$s" ] || continue
    ln -sf "$s" ~/.local/bin/"$(basename "$s")"
    echo "linked (bh): $(basename "$s")"
  done
fi

echo "Done — scripts available in ~/.local/bin/"
