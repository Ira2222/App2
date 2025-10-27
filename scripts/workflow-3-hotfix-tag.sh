#!/bin/bash
# Workflow 3: Hotfix Tag (from specific commit)
# Usage: Edit OWNER, REPO, TAG, COMMIT_SHA below, then run this script

set -euo pipefail

# === CONFIG (edit these variables) ===
OWNER="Ira2222"                         # GitHub username/org
REPO="App2"                             # Repository name
TAG="vX.Y.Z-hotfix"                    # Hotfix tag
COMMIT_SHA="abcdef1234567890"          # specific commit to tag

echo "🔧 Creating hotfix tag ${TAG} from commit ${COMMIT_SHA} for ${OWNER}/${REPO}"

# === Verify commit exists ===
echo "🔍 Verifying commit exists..."
if ! git cat-file -e "${COMMIT_SHA}^{commit}" 2>/dev/null; then
    echo "❌ Error: Commit ${COMMIT_SHA} not found in local repository"
    echo "💡 Tip: Run 'git fetch --all' to get latest commits"
    exit 1
fi

echo "✅ Commit ${COMMIT_SHA} found"

# === Create & push tag ===
echo "📝 Creating annotated tag at specific commit..."
git tag -a "${TAG}" "${COMMIT_SHA}" -m "${TAG}"
git push origin "${TAG}"

echo "✅ Hotfix tag ${TAG} created and pushed"

# === Verify release attestation ===
echo "🔍 Verifying release attestation..."
if ! gh release verify "${TAG}" -R "${OWNER}/${REPO}" --format json > /tmp/release-verify.json 2>&1; then
    echo "❌ Release attestation verification failed"
    cat /tmp/release-verify.json
    exit 1
fi

echo "✅ Release attestation verified"

# (optional) verify GHCR image
# echo "🐳 Verifying GHCR image provenance..."
# gh attestation verify "oci://ghcr.io/${OWNER}/${REPO}:${TAG}" --repo "${OWNER}/${REPO}"

echo "🎉 Hotfix tag workflow completed successfully!"
echo "📋 Summary:"
echo "  - Tag: ✅ ${TAG} created from ${COMMIT_SHA}"
echo "  - Release: ✅ Attestation verified"
