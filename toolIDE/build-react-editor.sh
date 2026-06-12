#!/bin/bash
# Build and deploy the React Editor (DocsPilot) — Linux/macOS equivalent of build-react-editor.ps1.
# Builds the Milkdown-based React editor and copies it to MdExplorer/wwwroot/milk_react/.
# Requires Node.js 20+ (preferred via nvm).

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$SCRIPT_DIR/.."
REACT_EDITOR_PATH="$PROJECT_ROOT/MdEditor.React"
DIST_PATH="$REACT_EDITOR_PATH/dist"
TARGET_PATH="$PROJECT_ROOT/MdExplorer/wwwroot/milk_react"

echo
echo "=== Build React Editor (DocsPilot) ==="
echo "Source: $REACT_EDITOR_PATH"
echo "Target: $TARGET_PATH"

# Find Node 20+ via nvm (prefer 22, then 23, then 20), fallback to system node
NODE_DIR=""
NVM_DIR="${NVM_DIR:-$HOME/.nvm}"
if [ -d "$NVM_DIR/versions/node" ]; then
    for ver in v22 v23 v20; do
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
    echo "ERROR: Node.js 20+ not found. Install with: nvm install 22" >&2
    exit 1
fi

NODE_MAJOR=$(node --version | sed 's/^v\([0-9]*\).*/\1/')
if [ "$NODE_MAJOR" -lt 20 ]; then
    echo "ERROR: Node.js 20+ required, found $(node --version). Install with: nvm install 22" >&2
    exit 1
fi

echo "Using Node.js: $(node --version)"

cd "$REACT_EDITOR_PATH"

# Step 1: build
echo
echo "=== Building React Editor ==="
npm run build:vite

# Step 2: verify build output
echo
echo "=== Verifying build output ==="
if [ ! -d "$DIST_PATH" ]; then
    echo "ERROR: dist/ folder not found after build" >&2
    exit 1
fi
if [ ! -f "$DIST_PATH/docspilot.es.js" ]; then
    echo "ERROR: docspilot.es.js not found in dist/" >&2
    exit 1
fi
echo "docspilot.es.js found"

# Step 3: clean target directory (remove old JS files from root, keep structure)
echo
echo "=== Cleaning target directory ==="
if [ -d "$TARGET_PATH" ]; then
    rm -f "$TARGET_PATH"/*.js
    echo "Removed old .js files from root"
else
    mkdir -p "$TARGET_PATH"
    echo "Created target directory"
fi

# Step 4: copy build output
echo
echo "=== Copying build output to wwwroot/milk_react/ ==="
cp "$DIST_PATH"/*.js "$TARGET_PATH/" 2>/dev/null || true
echo "Copied JS files"

if [ -d "$DIST_PATH/assets" ]; then
    mkdir -p "$TARGET_PATH/assets/css"
    cp "$DIST_PATH"/assets/css/*.css "$TARGET_PATH/assets/css/" 2>/dev/null || true
    echo "Copied CSS files to assets/css/"
fi

if [ -d "$DIST_PATH/css" ]; then
    mkdir -p "$TARGET_PATH/css"
    cp -r "$DIST_PATH"/css/* "$TARGET_PATH/css/"
    echo "Copied css/ folder"
fi

# Step 5: copy KaTeX CSS (not included in the Vite build)
echo
echo "=== Copying KaTeX CSS ==="
KATEX_SOURCE="$REACT_EDITOR_PATH/node_modules/katex/dist/katex.min.css"
KATEX_TARGET="$TARGET_PATH/assets/css/katex/dist"
if [ -f "$KATEX_SOURCE" ]; then
    mkdir -p "$KATEX_TARGET"
    cp "$KATEX_SOURCE" "$KATEX_TARGET/"
    echo "Copied katex.min.css"
else
    echo "WARNING: KaTeX CSS not found at: $KATEX_SOURCE" >&2
fi

# Step 6: final verification
echo
echo "=== Final Verification ==="
ALL_OK=1
for file in "$TARGET_PATH/docspilot.es.js" "$TARGET_PATH/docspilot.umd.js" "$TARGET_PATH/assets/css/milkdown-all.css"; do
    if [ -f "$file" ]; then
        echo "[OK] $file"
    else
        echo "[MISSING] $file" >&2
        ALL_OK=0
    fi
done

if [ "$ALL_OK" -ne 1 ]; then
    echo "BUILD FAILED: some required files are missing!" >&2
    exit 1
fi

TOTAL_FILES=$(find "$TARGET_PATH" -type f | wc -l)
echo
echo "=== BUILD AND DEPLOY COMPLETED SUCCESSFULLY ==="
echo "Total files deployed: $TOTAL_FILES"
echo
echo "The React editor is now available at: /milk_react/docspilot.es.js"
