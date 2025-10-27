#!/bin/bash
# Workflow 2: Quick Tag & Verify (from HEAD)
# Usage: Edit OWNER, REPO, TAG below, then run this script

set -euo pipefail

# === CONFIG (edit these variables) ===
OWNER="Ira2222"                         # GitHub username/org
REPO="App2"                             # Repository name
TAG="vX.Y.Z"                           # Release tag

echo "🏷️ Creating quick tag ${TAG} for ${OWNER}/${REPO}"

# === Create & push tag ===
echo "📝 Creating annotated tag at HEAD..."
git tag -a "${TAG}" -m "${TAG}"
git push origin "${TAG}"

echo "✅ Tag ${TAG} created and pushed"

# === Verify release attestation ===
echo "🔍 Verifying release attestation..."
if ! gh release verify "${TAG}" -R "${OWNER}/${REPO}" --format json > /tmp/release-verify.json 2>&1; then
    echo "❌ Release attestation verification failed"
    cat /tmp/release-verify.json
    exit 1
fi

echo "✅ Release attestation verified"

# (optional) verify specific asset
# echo "📦 Verifying specific asset..."
# gh release verify-asset "${TAG}" ./dist/your-asset.zip -R "${OWNER}/${REPO}"

# (optional) verify GHCR image
# echo "🐳 Verifying GHCR image provenance..."
# gh attestation verify "oci://ghcr.io/${OWNER}/${REPO}:${TAG}" --repo "${OWNER}/${REPO}"

echo "🎉 Quick tag workflow completed successfully!"
echo "📋 Summary:"
echo "  - Tag: ✅ ${TAG} created"
echo "  - Release: ✅ Attestation verified"
