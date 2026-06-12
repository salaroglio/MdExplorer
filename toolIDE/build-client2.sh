#!/bin/bash
# Build client2 (Angular 15) — Linux/macOS equivalent of build-client2.ps1.
# Prefers Node 16 from nvm (Angular 15 officially supports Node 14/16/18);
# falls back to the system node with a warning.

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
CLIENT2_DIR="$SCRIPT_DIR/../MdExplorer/client2"

# Find Node 16 via nvm
NODE_BIN=""
NVM_DIR="${NVM_DIR:-$HOME/.nvm}"
if [ -d "$NVM_DIR/versions/node" ]; then
    CANDIDATE=$(ls -d "$NVM_DIR/versions/node"/v16* 2>/dev/null | sort -V | tail -1)
    if [ -n "$CANDIDATE" ] && [ -x "$CANDIDATE/bin/node" ]; then
        NODE_BIN="$CANDIDATE/bin/node"
    fi
fi

if [ -z "$NODE_BIN" ]; then
    if command -v node >/dev/null 2>&1; then
        NODE_BIN="$(command -v node)"
        echo "WARNING: Node 16 not found via nvm, using system node $($NODE_BIN --version)." >&2
        echo "         Angular 15 officially supports Node 14/16/18; install with: nvm install 16" >&2
    else
        echo "ERROR: node not found. Install Node 16 (e.g. via nvm: nvm install 16)" >&2
        exit 1
    fi
fi

echo "=== Building client2 with Node.js $("$NODE_BIN" --version) ==="

cd "$CLIENT2_DIR"

echo
echo "Running: node update-version.js"
"$NODE_BIN" update-version.js

echo
echo "Running: ng build --base-href /client2/"
"$NODE_BIN" node_modules/@angular/cli/bin/ng build --base-href /client2/

echo
echo "=== Build completed successfully! ==="
