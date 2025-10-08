#!/bin/bash

# Build script to create the MdExplorer Linux installation package

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Configuration
PACKAGE_NAME="mdexplorer-1.0.0-linux"
BUILD_DIR="build"
ELECTRON_DIR="../ElectronMdExplorer"

# Clean previous builds
print_info "Cleaning previous builds..."
rm -rf "$BUILD_DIR"
rm -f "$PACKAGE_NAME.tar.gz"

# Create build directory
print_info "Creating build directory..."
mkdir -p "$BUILD_DIR/$PACKAGE_NAME"

# Find AppImage
print_info "Looking for AppImage..."
APPIMAGE=$(find "$ELECTRON_DIR/Binaries" -name "*.AppImage" -type f | head -n 1)

if [ -z "$APPIMAGE" ]; then
    print_error "No AppImage found in $ELECTRON_DIR/Binaries"
    print_info "Please build the AppImage first using electron-builder"
    exit 1
fi

print_info "Found AppImage: $(basename "$APPIMAGE")"

# Copy files
print_info "Copying files..."
cp "$APPIMAGE" "$BUILD_DIR/$PACKAGE_NAME/"
cp install.sh "$BUILD_DIR/$PACKAGE_NAME/"
cp uninstall.sh "$BUILD_DIR/$PACKAGE_NAME/"
cp README.txt "$BUILD_DIR/$PACKAGE_NAME/"

# Make scripts executable
chmod +x "$BUILD_DIR/$PACKAGE_NAME/install.sh"
chmod +x "$BUILD_DIR/$PACKAGE_NAME/uninstall.sh"
chmod +x "$BUILD_DIR/$PACKAGE_NAME/"*.AppImage

# Copy icon if exists
ICON_PATH="$ELECTRON_DIR/assets/icons/IconReady.png"
if [ -f "$ICON_PATH" ]; then
    print_info "Copying icon..."
    cp "$ICON_PATH" "$BUILD_DIR/$PACKAGE_NAME/mdexplorer.png"
else
    print_warning "Icon not found at $ICON_PATH"
    print_warning "Desktop shortcuts will not have an icon"
fi

# Create tarball
print_info "Creating package..."
cd "$BUILD_DIR"
tar -czf "../$PACKAGE_NAME.tar.gz" "$PACKAGE_NAME"
cd ..

# Calculate size
SIZE=$(du -h "$PACKAGE_NAME.tar.gz" | cut -f1)

# Cleanup
print_info "Cleaning up..."
rm -rf "$BUILD_DIR"

# Summary
echo
echo "============================================"
print_info "Package created successfully!"
echo "============================================"
echo
echo "Package: $PACKAGE_NAME.tar.gz"
echo "Size: $SIZE"
echo
echo "This package contains:"
echo "  - MdExplorer AppImage"
echo "  - Universal installer script"
echo "  - Uninstaller script"
echo "  - README documentation"
if [ -f "$ICON_PATH" ]; then
    echo "  - Application icon"
fi
echo
echo "To distribute:"
echo "1. Upload $PACKAGE_NAME.tar.gz to your website"
echo "2. Users download and extract: tar -xzf $PACKAGE_NAME.tar.gz"
echo "3. Users run: cd $PACKAGE_NAME && ./install.sh"
echo