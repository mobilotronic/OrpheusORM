#!/usr/bin/env bash
#
# Inserts a generated release-notes section into CHANGELOG.md, directly beneath
# the insertion marker so the newest release is always first.
#
#   .github/scripts/update-changelog.sh <version> <notes-file>
#
# Re-running for a version that is already present rewrites that section rather
# than stacking a duplicate on top, so a re-issued release is safe.
#
set -euo pipefail

VERSION="${1:?usage: update-changelog.sh <version> <notes-file>}"
NOTES="${2:?usage: update-changelog.sh <version> <notes-file>}"
FILE="${3:-CHANGELOG.md}"
MARKER='<!-- new releases are inserted below this line -->'

[ -f "$NOTES" ] || { echo "error: notes file '$NOTES' not found." >&2; exit 1; }

if [ ! -f "$FILE" ]; then
    echo "error: $FILE is missing; it must be committed with the insertion marker." >&2
    exit 1
fi

if ! grep -qF "$MARKER" "$FILE"; then
    echo "error: $FILE has no insertion marker. Expected the line: $MARKER" >&2
    exit 1
fi

tmp="$(mktemp)"

# Drop an existing section for this version: everything from its heading up to
# the next version heading.
if grep -qF "## [$VERSION] - " "$FILE"; then
    awk -v ver="## [$VERSION] - " '
        index($0, ver) == 1 { skip = 1; next }
        skip && /^## \[/     { skip = 0 }
        !skip                { print }
    ' "$FILE" > "$tmp"
    mv "$tmp" "$FILE"
    tmp="$(mktemp)"
fi

awk -v marker="$MARKER" -v notes="$NOTES" '
    { print }
    index($0, marker) == 1 { print ""; while ((getline line < notes) > 0) print line }
' "$FILE" > "$tmp"
mv "$tmp" "$FILE"
