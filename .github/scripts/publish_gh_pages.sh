#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

REMOTE_URL="$(git remote get-url origin)"
REGISTRY_URL="${1:-https://tonicatfelidae.github.io/ETPlugin}"
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

EXISTING_DIR="$TMP_DIR/existing"
DIST_DIR="$TMP_DIR/dist"

echo "==> Checking for existing gh-pages branch on origin..."
if git ls-remote --exit-code --heads origin gh-pages >/dev/null 2>&1; then
  git clone --depth 1 --branch gh-pages "$REMOTE_URL" "$EXISTING_DIR"
else
  mkdir -p "$EXISTING_DIR"
fi

echo "==> Building static UPM Scoped Registry..."
python3 "$REPO_ROOT/.github/scripts/build_upm_registry.py" \
  --packages-root "$REPO_ROOT/Packages" \
  --output-dir "$DIST_DIR" \
  --registry-url "$REGISTRY_URL" \
  --existing-dir "$EXISTING_DIR"

echo "==> Pushing static UPM Scoped Registry to origin/gh-pages..."
cd "$DIST_DIR"
git init
git checkout -b gh-pages
git add -A
git commit -m "chore(registry): publish UPM scoped registry"
git remote add origin "$REMOTE_URL"
git push -f origin gh-pages

echo "==> Done! Scoped Registry published to branch 'gh-pages' (${REGISTRY_URL})"
