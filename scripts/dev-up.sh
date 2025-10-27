#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_DIR="$ROOT_DIR/src/App2.Api"
WEB_DIR="$ROOT_DIR/apps/web"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet SDK is required" >&2
  exit 1
fi

if ! command -v npm >/dev/null 2>&1; then
  echo "npm is required" >&2
  exit 1
fi

pushd "$WEB_DIR" >/dev/null
if [ ! -d node_modules ]; then
  echo "Installing web dependencies..."
  npm install
fi
popd >/dev/null

# Run API
(
  cd "$API_DIR"
  echo "Starting App2 API..."
  ASPNETCORE_ENVIRONMENT=Development dotnet watch run --no-hot-reload --urls http://localhost:5080
) &
API_PID=$!

# Run web
(
  cd "$WEB_DIR"
  echo "Starting Vite dev server..."
  npm run dev -- --host
) &
WEB_PID=$!

echo "API PID: $API_PID"
echo "Web PID: $WEB_PID"

echo "Press Ctrl+C to stop both services."
trap 'kill $API_PID $WEB_PID 2>/dev/null || true' INT TERM
wait
