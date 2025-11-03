#!/usr/bin/env bash
set -euo pipefail

# ---- Config (env overrides OK) ----
REPO="${REPO:-$(gh repo view --json nameWithOwner -q .nameWithOwner)}"
MERGE_METHOD="${MERGE_METHOD:-squash}"      # merge|squash|rebase
CLOSE_LABELS="${CLOSE_LABELS:-obsolete,wontfix,invalid}"
SKIP_DRAFTS="${SKIP_DRAFTS:-true}"
DRY_RUN="${DRY_RUN:-false}"

echo "Repo: $REPO"
gh auth status >/dev/null

# 1) Auto-merge or queue all eligible open PRs
echo ">>> Enabling auto-merge (or queue) on eligible PRs..."
mapfile -t PRS < <(gh pr list -R "$REPO" --state open --json number,isDraft,url \
  --jq '.[] | select(.isDraft==false or "'$SKIP_DRAFTS'"!="true") | "\(.number) \(.url)"')

for row in "${PRS[@]}"; do
  num="${row%% *}"
  url="${row#* }"
  echo "  - PR #$num $url"
  if [[ "$DRY_RUN" == "true" ]]; then
    continue
  fi
  gh pr merge -R "$REPO" "$num" --$MERGE_METHOD --delete-branch --auto
done

# 2) Close obviously stale/obsolete PRs by label (optional)
IFS=',' read -r -a labels <<<"$CLOSE_LABELS"
if [[ ${#labels[@]} -gt 0 ]]; then
  echo ">>> Closing PRs with labels: ${labels[*]}"
  for label in "${labels[@]}"; do
    mapfile -t TO_CLOSE < <(gh pr list -R "$REPO" --state open --label "$label" --json number,url \
      --jq '.[] | "\(.number) \(.url)"')
    for row in "${TO_CLOSE[@]}"; do
      num="${row%% *}"
      url="${row#* }"
      echo "  - Closing #$num $url (label: $label)"
      if [[ "$DRY_RUN" != "true" ]]; then
        gh pr close -R "$REPO" "$num" -c "Closing as $label after maintenance sweep."
      fi
    done
  done
fi

# 3) Summary
echo ">>> Open PRs summary:"
gh pr list -R "$REPO" --state open --json number,title,isDraft,mergeStateStatus,url \
  --jq '.[] | {number,title,isDraft,mergeStateStatus,url}'
