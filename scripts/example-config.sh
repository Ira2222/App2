#!/bin/bash
# Example configuration for the release workflows
# Copy this file and edit the values for your repository

# === GITHUB REPOSITORY CONFIG ===
export OWNER="your-github-username"        # e.g., "Ira2222"
export REPO="your-repository-name"         # e.g., "App2"

# === RELEASE TAGS ===
export MAIN_TAG="v0.1.0"                   # Main release tag
export PATCH_TAG="v0.1.1"                  # Patch release tag
export HOTFIX_TAG="v0.1.0-hotfix"          # Hotfix tag

# === COMMIT SHA FOR HOTFIX (example) ===
export HOTFIX_COMMIT_SHA="abcdef1234567890"  # Replace with actual commit SHA

# === OPTIONAL: GHCR CONFIG ===
export GHCR_IMAGE="ghcr.io/${OWNER}/${REPO}"
export GHCR_TAG="${MAIN_TAG}"

# === USAGE EXAMPLES ===
echo "Example usage:"
echo "1. Full release cycle:"
echo "   OWNER=\"${OWNER}\" REPO=\"${REPO}\" TAG=\"${MAIN_TAG}\" ./scripts/workflow-1-full-release.sh"
echo ""
echo "2. Quick tag:"
echo "   OWNER=\"${OWNER}\" REPO=\"${REPO}\" TAG=\"${PATCH_TAG}\" ./scripts/workflow-2-quick-tag.sh"
echo ""
echo "3. Hotfix tag:"
echo "   OWNER=\"${OWNER}\" REPO=\"${REPO}\" TAG=\"${HOTFIX_TAG}\" COMMIT_SHA=\"${HOTFIX_COMMIT_SHA}\" ./scripts/workflow-3-hotfix-tag.sh"
echo ""
echo "4. Source this file to set environment variables:"
echo "   source scripts/example-config.sh"
