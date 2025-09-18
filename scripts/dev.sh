#!/usr/bin/env bash
set -euo pipefail

# Combined local dev runner: .NET API + SvelteKit dev server
# - API on http://localhost:5000
# - Web on http://localhost:5173 (uses PRIVATE_DOTNET_API_BASE_URL)

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB_DIR="$ROOT_DIR/src/web"
API_DIR="$ROOT_DIR/src/Randomizer"

API_URL="http://localhost:5000"

echo "[dev] Ensuring web deps installed (npm ci)"
cd "$WEB_DIR"
npm ci --no-audit --no-fund

echo "[dev] Applying DB migrations (Drizzle)"
npm run db:migrate || true

echo "[dev] Starting .NET API on $API_URL"
(cd "$API_DIR" && \
  dotnet run --configuration Debug -- api --urls "$API_URL") &
API_PID=$!

cleanup() {
  echo "\n[dev] Shutting down..."
  if kill -0 "$API_PID" 2>/dev/null; then
    kill "$API_PID" 2>/dev/null || true
    wait "$API_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

# Wait briefly for API to come up (best-effort)
for i in {1..30}; do
  if curl -fsS "$API_URL/meta" >/dev/null 2>&1; then
    break
  fi
  sleep 0.2
done

echo "[dev] Starting SvelteKit (http://localhost:5173)"
export PRIVATE_DOTNET_API_BASE_URL="$API_URL"
export DATABASE_URL="sqlite:dev.db"
export PUBLIC_SPRITES_BASE_URL="/sprites"

npm run dev

