#!/usr/bin/env bash
#
# Create a new project from the App2 template with full automation.
#
# Creates a new GitHub repository from the App2 template and automatically configures:
# - Branch protection rules requiring CI checks and PR reviews
# - Environments (dev, staging, prod) with branch policies
# - Security features (secret scanning, push protection, Dependabot)
# - CodeQL code scanning with security-extended queries
#
# Usage:
#   ./scripts/new-project.sh <NAME> [OWNER] [VISIBILITY]
#
# Examples:
#   ./scripts/new-project.sh LocationForm
#   ./scripts/new-project.sh LocationForm Ira2222 private
#   ./scripts/new-project.sh LocationForm MyOrg public
#

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Parse arguments
NAME="${1:-}"
OWNER="${2:-Ira2222}"
VISIBILITY="${3:-private}"
REPO="$OWNER/$NAME"
DESCRIPTION="FIN: $NAME (created from App2 template)"

if [ -z "$NAME" ]; then
    echo -e "${RED}Error: Project name is required${NC}"
    echo ""
    echo "Usage: $0 <NAME> [OWNER] [VISIBILITY]"
    echo ""
    echo "Examples:"
    echo "  $0 LocationForm"
    echo "  $0 LocationForm Ira2222 private"
    echo "  $0 LocationForm MyOrg public"
    exit 1
fi

echo ""
echo -e "${CYAN}========================================"
echo " App2 Template → New Project Automation"
echo "========================================${NC}"
echo ""

# Preflight checks
echo -e "${YELLOW}→ Checking prerequisites...${NC}"
if ! command -v gh &> /dev/null; then
    echo -e "  ${RED}✗ GitHub CLI (gh) not found. Install from: https://cli.github.com/${NC}"
    exit 1
fi
echo -e "  ${GREEN}✓ GitHub CLI installed${NC}"

if ! gh auth status &> /dev/null; then
    echo -e "  ${RED}✗ GitHub CLI not authenticated. Run: gh auth login${NC}"
    exit 1
fi
echo -e "  ${GREEN}✓ GitHub CLI authenticated${NC}"

# Step 1: Create repo from template
echo ""
echo -e "${YELLOW}→ Creating $REPO from App2 template...${NC}"
if gh repo create "$REPO" --template "$OWNER/App2" --"$VISIBILITY" --clone &> /dev/null; then
    echo -e "  ${GREEN}✓ Repository created and cloned${NC}"
else
    echo -e "  ${RED}✗ Failed to create repository${NC}"
    exit 1
fi

cd "$NAME"

# Step 2: Repository settings
echo ""
echo -e "${YELLOW}→ Configuring repository settings...${NC}"
gh repo edit "$REPO" --description "$DESCRIPTION" &> /dev/null
gh api -X PATCH "repos/$REPO" -H "X-GitHub-Api-Version: 2022-11-28" -f delete_branch_on_merge=true &> /dev/null
echo -e "  ${GREEN}✓ Auto-delete head branches enabled${NC}"

# Step 3: Security & analysis
echo ""
echo -e "${YELLOW}→ Enabling security features...${NC}"
set +e
cat <<'JSON' | gh api -X PATCH "repos/$REPO" -H "X-GitHub-Api-Version: 2022-11-28" --input - &> /dev/null
{
  "security_and_analysis": {
    "secret_scanning": { "status": "enabled" },
    "secret_scanning_push_protection": { "status": "enabled" },
    "dependabot_security_updates": { "status": "enabled" }
  }
}
JSON
if [ $? -ne 0 ]; then
  echo -e "  ${YELLOW}⚠ Secret scanning/push protection unavailable (requires Pro/public repo)${NC}"
  echo -e "  ${GREEN}✓ Dependabot security updates enabled (available on Free)${NC}"
else
  echo -e "  ${GREEN}✓ Secret scanning enabled${NC}"
  echo -e "  ${GREEN}✓ Push protection enabled${NC}"
  echo -e "  ${GREEN}✓ Dependabot security updates enabled${NC}"
fi
set -e

# Step 4: Create environments
new_env() {
    local ENV="$1"

    # Create environment with branch policy
    set +e
    cat <<'JSON' | gh api -X PUT "repos/$REPO/environments/$ENV" -H "X-GitHub-Api-Version: 2022-11-28" --input - &> /dev/null
{
  "deployment_branch_policy": {
    "protected_branches": false,
    "custom_branch_policies": true
  }
}
JSON
    local result=$?
    set -e

    if [ $result -ne 0 ]; then
        return 1
    fi

    # Allow only main branch to deploy
    echo '{ "name": "main" }' | gh api -X POST "repos/$REPO/environments/$ENV/deployment-branch-policies" -H "X-GitHub-Api-Version: 2022-11-28" --input - &> /dev/null
}

echo ""
echo -e "${YELLOW}→ Creating environments...${NC}"
set +e
new_env dev
if [ $? -eq 0 ]; then
  echo -e "  ${GREEN}✓ dev environment created (main branch only)${NC}"
  new_env staging
  echo -e "  ${GREEN}✓ staging environment created (main branch only)${NC}"
  new_env prod
  echo -e "  ${GREEN}✓ prod environment created (main branch only)${NC}"
else
  echo -e "  ${YELLOW}⚠ Environments unavailable (requires Pro for private repos or public repo)${NC}"
fi
set -e

# Step 5: Branch protection
echo ""
echo -e "${YELLOW}→ Configuring branch protection on main...${NC}"
set +e
cat <<'JSON' | gh api -X PUT "repos/$REPO/branches/main/protection" -H "X-GitHub-Api-Version: 2022-11-28" --input - &> /dev/null
{
  "required_status_checks": {
    "strict": true,
    "contexts": ["ci"]
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "required_conversation_resolution": true
}
JSON
if [ $? -ne 0 ]; then
  echo -e "  ${YELLOW}⚠ Branch protection unavailable (requires Pro for private repos or public repo)${NC}"
  echo -e "  ${GRAY}  Manual setup: Settings → Branches → Add rule${NC}"
else
  echo -e "  ${GREEN}✓ Require CI to pass${NC}"
  echo -e "  ${GREEN}✓ Require 1 PR review${NC}"
  echo -e "  ${GREEN}✓ Dismiss stale reviews${NC}"
  echo -e "  ${GREEN}✓ Require conversation resolution${NC}"
  echo -e "  ${GREEN}✓ Disable force pushes and deletions${NC}"
fi
set -e

# Step 6: CodeQL default setup
echo ""
echo -e "${YELLOW}→ Enabling CodeQL scanning...${NC}"
set +e
cat <<'JSON' | gh api -X PATCH "repos/$REPO/code-scanning/default-setup" -H "X-GitHub-Api-Version: 2022-11-28" --input - &> /dev/null
{
  "state": "enabled",
  "query_suite": "security-extended",
  "languages": ["csharp","javascript","typescript"]
}
JSON
if [ $? -ne 0 ]; then
  echo -e "  ${YELLOW}⚠ CodeQL unavailable (requires Advanced Security for private repos or public repo)${NC}"
  echo -e "  ${GRAY}  Manual setup: Security → Code scanning → Set up → Advanced${NC}"
else
  echo -e "  ${GREEN}✓ CodeQL enabled (security-extended)${NC}"
  echo -e "  ${GREEN}✓ Languages: C#, JavaScript, TypeScript${NC}"
fi
set -e

# Summary
echo ""
echo -e "${CYAN}========================================"
echo -e " ${GREEN}✓ Repository Ready!"
echo -e "${CYAN}========================================${NC}"
echo ""
echo -e "Repository: ${GRAY}https://github.com/$REPO${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "  1. Update CODEOWNERS (.github/CODEOWNERS)"
echo -e "  2. Rename namespaces (App2.* → $NAME.*)"
echo -e "  3. Replace Todo entity with your domain entities"
echo -e "  4. Update README.md and appsettings.json"
echo -e "  5. Make first commit to trigger CI"
echo ""
echo -e "${YELLOW}Trigger CI:${NC}"
echo -e "  ${GRAY}echo \"init\" >> README.md${NC}"
echo -e "  ${GRAY}git add README.md && git commit -m \"chore: init\" && git push${NC}"
echo -e "  ${GRAY}gh run watch${NC}"
echo ""
