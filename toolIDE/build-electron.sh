#!/bin/bash
# Build ElectronMdExplorer — Linux/macOS equivalent of build-electron.ps1.
# Prefers Node 23 from nvm (then 22, then 20); falls back to the system node.
# On Linux electron-builder produces AppImage and deb packages in Binaries/.

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ELECTRON_PATH="$SCRIPT_DIR/../ElectronMdExplorer"

# Find Node via nvm (prefer 23, then 22, then 20), fallback to system node
NODE_DIR=""
NVM_DIR="${NVM_DIR:-$HOME/.nvm}"
if [ -d "$NVM_DIR/versions/node" ]; then
    for ver in v23 v22 v20; do
        CANDIDATE=$(ls -d "$NVM_DIR/versions/node/$ver"* 2>/dev/null | sort -V | tail -1)
        if [ -n "$CANDIDATE" ] && [ -x "$CANDIDATE/bin/node" ]; then
            NODE_DIR="$CANDIDATE/bin"
            break
        fi
    done
fi

if [ -n "$NODE_DIR" ]; then
    export PATH="$NODE_DIR:$PATH"
fi

if ! command -v node >/dev/null 2>&1; then
    echo "ERROR: Node.js not found. Install with: nvm install 23" >&2
    exit 1
fi

echo "=== Building ElectronMdExplorer with Node.js $(node --version) ==="

cd "$ELECTRON_PATH"

# Ensure node_modules and patch-package patches (postinstall hook) are present
if [ ! -d node_modules ]; then
    echo
    echo "Installing dependencies (npm install)..."
    npm install
fi

# Run prebuild scripts
echo
echo "Running prebuild scripts..."
node scripts/check-go-binaries.js
node scripts/check-release-state.js

# Run electron-builder
echo
echo "Running: electron-builder"
node node_modules/electron-builder/cli.js

# Run postbuild (no-op outside Windows: website deployment ships the NSIS installer)
echo
echo "Running postbuild script..."
node scripts/post-build.js

echo
echo "=== Build completed successfully! ==="
