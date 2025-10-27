#!/bin/bash
# Prerequisites checker for release workflows
# Run this before using any of the workflow scripts

set -euo pipefail

echo "🔍 Checking prerequisites for release workflows..."

# === GIT CHECK ===
echo "📁 Checking Git..."
if ! command -v git &> /dev/null; then
    echo "❌ Git is not installed"
    exit 1
fi

if [ ! -d .git ]; then
    echo "❌ Not in a Git repository"
    echo "💡 Run: git init"
    exit 1
fi

echo "✅ Git repository found"

# === GITHUB CLI CHECK ===
echo "🐙 Checking GitHub CLI..."
if ! command -v gh &> /dev/null; then
    echo "❌ GitHub CLI (gh) is not installed"
    echo "💡 Install from: https://cli.github.com/"
    exit 1
fi

if ! gh auth status &> /dev/null; then
    echo "❌ GitHub CLI not authenticated"
    echo "💡 Run: gh auth login"
    exit 1
fi

echo "✅ GitHub CLI authenticated"

# === DOTNET CHECK ===
echo "🔧 Checking .NET..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed"
    echo "💡 Install from: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"

# === DOCKER CHECK (optional) ===
echo "🐳 Checking Docker (optional)..."
if command -v docker &> /dev/null; then
    echo "✅ Docker found: $(docker --version)"
else
    echo "⚠️  Docker not found (optional for GHCR image verification)"
fi

# === REPOSITORY CONFIG CHECK ===
echo "📋 Checking repository configuration..."

# Check if we have a remote origin
if ! git remote get-url origin &> /dev/null; then
    echo "⚠️  No remote origin configured"
    echo "💡 Run: git remote add origin <your-repo-url>"
else
    ORIGIN_URL=$(git remote get-url origin)
    echo "✅ Remote origin: ${ORIGIN_URL}"
fi

# Check if we're on a branch
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo "✅ Current branch: ${CURRENT_BRANCH}"

# === PROJECT STRUCTURE CHECK ===
echo "📁 Checking project structure..."

if [ ! -f "App2.sln" ]; then
    echo "❌ App2.sln not found"
    echo "💡 Make sure you're in the App2 project root"
    exit 1
fi

if [ ! -d "src/App2.Api" ]; then
    echo "❌ src/App2.Api directory not found"
    echo "💡 Make sure you're in the App2 project root"
    exit 1
fi

echo "✅ Project structure looks good"

# === TEST CONFIGURATION CHECK ===
echo "🧪 Checking test configuration..."

if [ ! -f "src/App2.Api/appsettings.Testing.json" ]; then
    echo "⚠️  appsettings.Testing.json not found"
    echo "💡 This file is recommended for integration tests"
fi

if [ ! -d "tests" ]; then
    echo "⚠️  tests directory not found"
    echo "💡 No test projects detected"
fi

echo "✅ Test configuration checked"

# === WORKFLOW SCRIPTS CHECK ===
echo "📜 Checking workflow scripts..."

if [ ! -f "scripts/workflow-1-full-release.sh" ]; then
    echo "❌ workflow-1-full-release.sh not found"
    exit 1
fi

if [ ! -f "scripts/workflow-2-quick-tag.sh" ]; then
    echo "❌ workflow-2-quick-tag.sh not found"
    exit 1
fi

if [ ! -f "scripts/workflow-3-hotfix-tag.sh" ]; then
    echo "❌ workflow-3-hotfix-tag.sh not found"
    exit 1
fi

echo "✅ All workflow scripts found"

# === FINAL SUMMARY ===
echo ""
echo "🎉 Prerequisites check completed!"
echo ""
echo "📋 Summary:"
echo "  - Git: ✅ $(git --version)"
echo "  - GitHub CLI: ✅ $(gh --version | head -n1)"
echo "  - .NET: ✅ $(dotnet --version)"
echo "  - Docker: $(command -v docker &> /dev/null && echo "✅ $(docker --version)" || echo "⚠️  Not installed")"
echo "  - Repository: ✅ $(git remote get-url origin 2>/dev/null || echo "No origin configured")"
echo "  - Project: ✅ App2 solution found"
echo "  - Scripts: ✅ All workflow scripts present"
echo ""
echo "🚀 You're ready to use the release workflows!"
echo ""
echo "Next steps:"
echo "1. Edit the config variables in the workflow scripts"
echo "2. Run: ./scripts/workflow-1-full-release.sh"
echo "3. Or run: ./scripts/example-config.sh for examples"
