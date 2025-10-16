#!/bin/bash

# Simple build script for MdExplorer Angular frontend
# This script only builds the main application

set -e

echo "🚀 Building MdExplorer Angular Frontend"
echo "========================================"

cd MdExplorer/client2

# Use Node 14.21.3 if nvm is available
if command -v nvm &> /dev/null; then
    echo "📦 Using Node.js 14.21.3"
    source ~/.nvm/nvm.sh
    nvm use 14.21.3
fi

echo "📥 Installing dependencies..."
npm install

echo "🔨 Building Angular application..."
npm run build

echo "✅ Angular build completed successfully!"
echo ""
echo "Output: MdExplorer/wwwroot/client2/"
