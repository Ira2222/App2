#!/usr/bin/env bash
# one-shot: test → export swagger → generate TS client → push → wait for CI → tag

set -euo pipefail

# ==== config (edit if needed) ====
OWNER="${OWNER:-Ira2222}"
REPO="${REPO:-App2}"
BRANCH="${BRANCH:-main}"
TAG_NAME="${TAG_NAME:-v0.1.0}"
RUN_INTEGRATION="${RUN_INTEGRATION:-true}"
TEST_TIMEOUT_SEC="${TEST_TIMEOUT_SEC:-120}"
API_PORT="${API_PORT:-5081}"

# ==== preflight ====
echo "🔍 Checking prerequisites..."
command -v gh >/dev/null       || { echo "❌ Missing: gh (GitHub CLI)"; exit 1; }
command -v dotnet >/dev/null   || { echo "❌ Missing: dotnet SDK"; exit 1; }
command -v npm >/dev/null      || { echo "❌ Missing: npm/node"; exit 1; }
command -v curl >/dev/null     || { echo "❌ Missing: curl"; exit 1; }
command -v jq >/dev/null       || { echo "❌ Missing: jq"; exit 1; }

# ensure we're in a git repo root
ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
cd "$ROOT"

# ensure origin is set
if ! git remote get-url origin >/dev/null 2>&1; then
  echo "📌 Adding git remote origin..."
  git remote add origin "git@github.com:${OWNER}/${REPO}.git"
fi

git checkout -B "$BRANCH"

# bail if API port is busy
if lsof -i:${API_PORT} >/dev/null 2>&1; then
  echo "❌ Port ${API_PORT} is in use. Please free it first."
  exit 1
fi

echo "✅ All prerequisites met"
echo ""

# ==== local tests (Testing env, no external deps) ====
echo "🧪 Running tests in Testing environment..."
export ASPNETCORE_ENVIRONMENT=Testing
export Features__KeyVault=false
export Features__Authentication=false
export Features__RedisOutputCache=false
export Features__SecurityHeaders=true

dotnet restore
dotnet build -c Release

if [ "$RUN_INTEGRATION" = "true" ]; then
  echo "   Running all tests (unit + integration)..."
  dotnet test -c Release --no-build --blame-hang --blame-hang-timeout "${TEST_TIMEOUT_SEC}s"
else
  echo "   Running unit tests only..."
  dotnet test -c Release --no-build --filter "FullyQualifiedName!~Integration"
fi

echo "✅ Tests passed"
echo ""

# ==== verify OpenAPI spec exists ====
echo "📋 Verifying OpenAPI specification..."
if [ -f "openapi/app2.openapi.json" ]; then
  echo "   ✅ OpenAPI spec found: openapi/app2.openapi.json"
  echo "   ℹ️  Using handcrafted TypeScript client (see ADR 001)"
else
  echo "   ⚠️  OpenAPI spec not found - export manually if needed:"
  echo "      ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/App2.Api"
  echo "      curl http://localhost:5000/swagger/v1/swagger.json > openapi/app2.openapi.json"
fi
echo ""

# ==== commit any changes ====
echo "📝 Checking for uncommitted changes..."
if git diff --quiet && git diff --staged --quiet; then
  echo "   ℹ️  No changes to commit"
else
  echo "   📝 Uncommitted changes found - please commit them first"
  git status --short
  exit 1
fi
echo ""

# ==== push to remote ====
echo "🚀 Pushing to ${OWNER}/${REPO}:${BRANCH}..."
git push -u origin "$BRANCH"
echo "   ✅ Pushed to remote"
echo ""

# ==== wait for CI to pass ====
echo "⏳ Waiting for CI to pass on ${OWNER}/${REPO}..."
echo "   Fetching latest CI workflow run..."

# wait a moment for GitHub to register the push
sleep 5

# get the specific CI workflow run ID
RUN_ID=$(gh run list --workflow=ci.yml --limit 1 --json databaseId --jq '.[0].databaseId')

if [ -z "$RUN_ID" ]; then
  echo "   ❌ Could not find CI workflow run"
  exit 1
fi

echo "   Watching CI run: https://github.com/${OWNER}/${REPO}/actions/runs/${RUN_ID}"

# watch the specific run and exit with its status
if gh run watch "$RUN_ID" --exit-status; then
  echo "   ✅ CI passed"
else
  echo "   ❌ CI failed"
  echo ""
  echo "View logs at: https://github.com/${OWNER}/${REPO}/actions/runs/${RUN_ID}"
  exit 1
fi
echo ""

# ==== tag & push tag ====
echo "🏷️  Creating and pushing tag ${TAG_NAME}..."

# check if tag already exists
if git rev-parse "$TAG_NAME" >/dev/null 2>&1; then
  echo "   ⚠️  Tag ${TAG_NAME} already exists locally"
  read -p "   Delete and recreate? (y/N): " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Yy]$ ]]; then
    git tag -d "$TAG_NAME"
    git push origin ":refs/tags/${TAG_NAME}" 2>/dev/null || true
  else
    echo "   Skipping tag creation"
    exit 0
  fi
fi

git tag -a "$TAG_NAME" -m "Release $TAG_NAME"
git push origin "$TAG_NAME"

echo "   ✅ Tagged and pushed ${TAG_NAME}"
echo ""

# ==== summary ====
OWNER_LOWER=$(echo "$OWNER" | tr '[:upper:]' '[:lower:]')
REPO_LOWER=$(echo "$REPO" | tr '[:upper:]' '[:lower:]')

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🎉 Release automation complete!"
echo ""
echo "📦 Tag:        ${TAG_NAME}"
echo "🔗 Repository: https://github.com/${OWNER}/${REPO}"
echo "🏷️  Release:    https://github.com/${OWNER}/${REPO}/releases/tag/${TAG_NAME}"
echo "🐳 Container:  ghcr.io/${OWNER_LOWER}/${REPO_LOWER}:${TAG_NAME}"
echo ""
echo "Next steps:"
echo "  1. Wait for release workflow to complete (creates GitHub release + GHCR image)"
echo "  2. Verify release: gh release view ${TAG_NAME}"
echo "  3. Verify image:   gh attestation verify oci://ghcr.io/${OWNER_LOWER}/${REPO_LOWER}:${TAG_NAME} --owner ${OWNER} --repo ${REPO}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
