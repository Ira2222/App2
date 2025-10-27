#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${1:-http://localhost:5080}"

curl -sf "$BASE_URL/healthz/live" >/tmp/app2-live.json
echo "Live health: $(cat /tmp/app2-live.json)"

curl -sf "$BASE_URL/healthz/ready" >/tmp/app2-ready.json
echo "Ready health: $(cat /tmp/app2-ready.json)"

curl -sf -H "Accept: application/json" "$BASE_URL/api/todos" >/tmp/app2-todos.json
echo "Todos: $(cat /tmp/app2-todos.json)"

if curl -sf "$BASE_URL/reference" >/tmp/app2-reference.html; then
  echo "Scalar/Docs available at $BASE_URL/reference"
else
  echo "Scalar UI not reachable (expected outside Development)."
fi
