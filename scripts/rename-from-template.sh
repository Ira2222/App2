#!/usr/bin/env bash
#
# rename-from-template.sh - Rename App2 template to a new project name
#
# Usage: ./scripts/rename-from-template.sh <NewName>
# Example: ./scripts/rename-from-template.sh LocationForm
#
# This script performs cross-platform renaming (macOS and Linux compatible):
# 1. Replaces "App2" with the new name in file contents
# 2. Renames files and directories containing "App2"
#

set -euo pipefail

OLD="App2"
NEW="${1:?Usage: $0 <NewName>}"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo ""
echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW} Renaming ${OLD} → ${NEW}${NC}"
echo -e "${YELLOW}========================================${NC}"
echo ""

# Detect OS for sed compatibility
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS (BSD sed)
    SED_INPLACE="sed -i ''"
    echo -e "${GREEN}→ Detected macOS (BSD sed)${NC}"
else
    # Linux (GNU sed)
    SED_INPLACE="sed -i"
    echo -e "${GREEN}→ Detected Linux (GNU sed)${NC}"
fi

# Step 1: Replace contents in tracked and untracked files
echo ""
echo -e "${YELLOW}→ Replacing '${OLD}' with '${NEW}' in file contents...${NC}"

FILE_PATTERNS=(
    "*.sln"
    "*.csproj"
    "*.cs"
    "*.ts"
    "*.tsx"
    "*.json"
    "*.yml"
    "*.yaml"
    "*.md"
    "*.txt"
)

# Build find command with multiple -name patterns
FIND_CMD="find . -type f \("
for i in "${!FILE_PATTERNS[@]}"; do
    if [ $i -eq 0 ]; then
        FIND_CMD="$FIND_CMD -name '${FILE_PATTERNS[$i]}'"
    else
        FIND_CMD="$FIND_CMD -o -name '${FILE_PATTERNS[$i]}'"
    fi
done
FIND_CMD="$FIND_CMD \) ! -path '*/node_modules/*' ! -path '*/.git/*' ! -path '*/bin/*' ! -path '*/obj/*'"

# Execute find and replace
eval "$FIND_CMD" | while IFS= read -r file; do
    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i '' "s/${OLD}/${NEW}/g" "$file"
    else
        sed -i "s/${OLD}/${NEW}/g" "$file"
    fi
done

echo -e "${GREEN}  ✓ Content replacement complete${NC}"

# Step 2: Rename files and directories
echo ""
echo -e "${YELLOW}→ Renaming files and directories containing '${OLD}'...${NC}"

# Find all paths containing OLD, sort by depth (longest first to avoid path issues)
find . -depth -name "*${OLD}*" ! -path '*/node_modules/*' ! -path '*/.git/*' ! -path '*/bin/*' ! -path '*/obj/*' | \
    awk '{print length, $0}' | sort -rn | cut -d' ' -f2- | \
    while IFS= read -r path; do
        dir=$(dirname "$path")
        base=$(basename "$path")
        newbase="${base//$OLD/$NEW}"

        if [ "$base" != "$newbase" ]; then
            newpath="$dir/$newbase"
            mv "$path" "$newpath"
            echo -e "  ${GREEN}✓${NC} Renamed: $path → $newpath"
        fi
    done

echo -e "${GREEN}  ✓ Path renaming complete${NC}"

# Summary
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN} ✓ Renaming Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "  1. Review changes: ${NC}git status${NC}"
echo -e "  2. Test build: ${NC}dotnet build && cd apps/web && npm run build${NC}"
echo -e "  3. Commit changes: ${NC}git add -A && git commit -m 'chore: rename ${OLD} → ${NEW}'${NC}"
echo ""
