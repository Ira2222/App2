#!/bin/bash
# Workflow 1: Full Release Cycle (Test → Push → CI → Tag → Verify)
# Usage: Edit OWNER, REPO, TAG below, then run this script

set -euo pipefail

# === CONFIG (edit the 3 lines below) ===
OWNER="your-gh-owner"                   # e.g., Ira2222
REPO="your-repo"                        # e.g., App2
TAG="v0.1.0"

echo "🚀 Starting full release cycle for ${OWNER}/${REPO} with tag ${TAG}"

# === ENV for tests (mirror your Testing profile) ===
export ASPNETCORE_ENVIRONMENT=Testing
export Features__KeyVault=false
export Features__Authentication=false
export Features__RedisOutputCache=false

echo "🧪 Running tests in Testing environment..."

# === 1) Clean, restore, build, test (unit+integration) ===
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build --logger "trx;LogFileName=test-results.trx"

echo "✅ Tests passed"

# === 2) Commit and push to main ===
echo "📤 Committing and pushing to main..."
git add -A
git commit -m "chore: prepare ${TAG} (green tests)"
git push origin main

echo "✅ Pushed to main"

# === 3) Watch the latest CI run until it completes ===
echo "⏳ Waiting for CI to complete..."
gh run watch --exit-status -R "${OWNER}/${REPO}"

echo "✅ CI completed successfully"

# === 4) Create & push an annotated tag, then watch release workflow ===
echo "🏷️ Creating and pushing tag ${TAG}..."
git tag -a "${TAG}" -m "${TAG}"
git push origin "${TAG}"

echo "✅ Tag ${TAG} created and pushed"

# (optional) if your release workflow name isn't auto-picked up, filter by name:
# gh run list -R "${OWNER}/${REPO}" -w "release-provenance.yml" --json databaseId -q '.[0].databaseId' | xargs -I{} gh run watch --exit-status -R "${OWNER}/${REPO}" {}

# === 5) Verify the release attestation & image provenance ===
echo "🔍 Verifying release attestation..."
gh release verify "${TAG}" -R "${OWNER}/${REPO}"

echo "✅ Release attestation verified"

# If you publish a container to GHCR with the same tag:
echo "🐳 Verifying GHCR image provenance..."
gh attestation verify "oci://ghcr.io/${OWNER}/${REPO}:${TAG}" --repo "${OWNER}/${REPO}"

echo "✅ GHCR image provenance verified"

echo "🎉 Full release cycle completed successfully!"
echo "📋 Summary:"
echo "  - Tests: ✅ Passed"
echo "  - Push: ✅ Completed"
echo "  - CI: ✅ Green"
echo "  - Tag: ✅ ${TAG} created"
echo "  - Release: ✅ Attestation verified"
echo "  - Container: ✅ Provenance verified"
