#!/bin/bash
# Push template tag to origin if it doesn't exist remotely

set -e

echo "🔍 Checking template-v1.0.0 tag status..."

# Check if tag exists locally
if ! git tag -l template-v1.0.0 > /dev/null 2>&1; then
    echo "❌ Error: template-v1.0.0 tag does not exist locally"
    echo "   Create it first with: git tag -a template-v1.0.0 -m 'Template baseline'"
    exit 1
fi

# Check if tag exists on remote
if git ls-remote --tags origin | grep -q "template-v1.0.0"; then
    echo "✅ template-v1.0.0 tag already exists on remote"
else
    echo "📤 Pushing template-v1.0.0 tag to origin..."
    git push origin template-v1.0.0
    echo "✅ template-v1.0.0 tag pushed successfully"
fi

echo "🎉 Template tag verification complete"

