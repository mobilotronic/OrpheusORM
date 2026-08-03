#!/usr/bin/env bash
#
# Generates release notes for a tag from the commits that landed on master
# between it and the previous tag, grouped by conventional-commit type.
#
#   .github/scripts/generate-release-notes.sh <tag> [previous-tag]
#
# Previous tag is worked out automatically when omitted. Writes markdown to
# stdout, so it can be previewed locally before a release:
#
#   .github/scripts/generate-release-notes.sh v2.0.0
#
set -euo pipefail

TAG="${1:?usage: generate-release-notes.sh <tag> [previous-tag]}"
PREV="${2:-}"

REPO_URL="https://github.com/mobilotronic/OrpheusORM"

# The ref actually walked. Normally the tag itself, but when the tag does not
# exist yet this falls back to HEAD, so the notes for an upcoming release can be
# previewed before tagging it — which is the usual reason to run this by hand.
if git rev-parse -q --verify "${TAG}^{commit}" >/dev/null 2>&1; then
    TARGET="$TAG"
    PREVIEW=false
else
    if ! git rev-parse -q --verify "HEAD^{commit}" >/dev/null 2>&1; then
        echo "error: '$TAG' is not a tag or commit, and HEAD is unusable." >&2
        exit 1
    fi
    TARGET="HEAD"
    PREVIEW=true
    # Notice goes to stderr so it never contaminates the markdown on stdout.
    echo "note: tag '$TAG' does not exist yet — previewing the commits currently on HEAD." >&2
fi

if [ -z "$PREV" ]; then
    if [ "$PREVIEW" = true ]; then
        # Nearest tag reachable from HEAD; HEAD itself is not tagged.
        PREV="$(git describe --tags --abbrev=0 "$TARGET" 2>/dev/null || true)"
    else
        # Nearest tag reachable from the commit before this one.
        PREV="$(git describe --tags --abbrev=0 "${TARGET}^" 2>/dev/null || true)"
    fi
fi

if [ -n "$PREV" ]; then
    RANGE="${PREV}..${TARGET}"
else
    RANGE="$TARGET"
fi

breaking=() features=() fixes=() perf=() refactor=() docs=() tests=() maint=() other=()

# --first-parent is what makes this "only the commits on master": one entry per
# squashed PR, and no feature-branch commits leaking in from any merge that was
# not squashed.
while IFS= read -r sha; do
    subject="$(git log -1 --format='%s' "$sha")"
    body="$(git log -1 --format='%b' "$sha")"

    # Conventional prefix: type, optional (scope), optional ! for breaking.
    prefix="${subject%%:*}"
    type=""
    if [ "$prefix" != "$subject" ]; then
        type="$(printf '%s' "$prefix" | sed -E 's/\(.*\)//; s/!$//' | tr '[:upper:]' '[:lower:]')"
    fi

    # Description without the prefix, first letter left as authored.
    if [ -n "$type" ]; then
        text="$(printf '%s' "${subject#*:}" | sed -E 's/^[[:space:]]+//')"
    else
        text="$subject"
    fi

    entry="- ${text}"

    # Breaking is a cross-cutting flag: the ! marker, or the footer. The commit
    # still appears under its own type below; what goes in the breaking section
    # is the BREAKING CHANGE footer, because that is the part that tells a
    # reader what to change. Falls back to the subject when there is no footer.
    if printf '%s' "$prefix" | grep -q '!$' || printf '%s' "$body" | grep -q '^BREAKING CHANGE:'; then
        note="$(printf '%s' "$body" \
            | sed -n '/^BREAKING CHANGE:/,$p' \
            | sed -E '1s/^BREAKING CHANGE:[[:space:]]*//' \
            | tr '\n' ' ' \
            | sed -E 's/[[:space:]]+/ /g; s/[[:space:]]+$//')"
        [ -z "$note" ] && note="$text"
        breaking+=("- ${note}")
    fi

    case "$type" in
        feat)              features+=("$entry") ;;
        fix)               fixes+=("$entry") ;;
        perf)              perf+=("$entry") ;;
        refactor)          refactor+=("$entry") ;;
        docs)              docs+=("$entry") ;;
        test|tests)        tests+=("$entry") ;;
        build|ci|chore)    maint+=("$entry") ;;
        *)                 other+=("$entry") ;;
    esac
done < <(git log --first-parent --format='%H' "$RANGE")

section() {
    local title="$1"; shift
    [ "$#" -eq 0 ] && return 0
    printf '### %s\n\n' "$title"
    printf '%s\n' "$@"
    printf '\n'
}

VERSION="${TAG#v}"
DATE="$(git log -1 --format=%cs "$TARGET")"

printf '## [%s] - %s\n\n' "$VERSION" "$DATE"

section 'Breaking Changes' ${breaking[@]+"${breaking[@]}"}
section 'Features'         ${features[@]+"${features[@]}"}
section 'Bug Fixes'        ${fixes[@]+"${fixes[@]}"}
section 'Performance'      ${perf[@]+"${perf[@]}"}
section 'Refactoring'      ${refactor[@]+"${refactor[@]}"}
section 'Documentation'    ${docs[@]+"${docs[@]}"}
section 'Tests'            ${tests[@]+"${tests[@]}"}
section 'Maintenance'      ${maint[@]+"${maint[@]}"}
section 'Other Changes'    ${other[@]+"${other[@]}"}

total=$(( ${#breaking[@]} + ${#features[@]} + ${#fixes[@]} + ${#perf[@]} \
        + ${#refactor[@]} + ${#docs[@]} + ${#tests[@]} + ${#maint[@]} + ${#other[@]} ))
if [ "$total" -eq 0 ]; then
    printf 'No changes recorded on master for this release.\n\n'
fi

if [ -n "$PREV" ]; then
    printf '**Full Changelog**: %s/compare/%s...%s\n' "$REPO_URL" "$PREV" "$TAG"
else
    printf '**Full Changelog**: %s/commits/%s\n' "$REPO_URL" "$TAG"
fi
