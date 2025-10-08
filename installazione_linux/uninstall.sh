#!/bin/bash

# MdExplorer Uninstaller for Linux

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Installation paths
INSTALL_DIR="/opt/mdexplorer"
DESKTOP_ENTRY_PATH="/usr/share/applications/mdexplorer.desktop"
ICON_PATH="/usr/share/icons/hicolor/256x256/apps/mdexplorer.png"
SYMLINK_PATH="/usr/local/bin/mdexplorer"

# Functions for colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Check permissions
check_permissions() {
    if [ "$EUID" -eq 0 ]; then
        SUDO=""
    else
        print_info "This uninstaller needs sudo privileges"
        SUDO="sudo"
        $SUDO true || {
            print_error "Failed to obtain sudo privileges"
            exit 1
        }
    fi
}

# Remove desktop shortcut
remove_desktop_shortcut() {
    # Find desktop directory
    if command -v xdg-user-dir >/dev/null 2>&1; then
        DESKTOP_DIR=$(xdg-user-dir DESKTOP 2>/dev/null)
    fi
    
    if [ -z "$DESKTOP_DIR" ] || [ ! -d "$DESKTOP_DIR" ]; then
        DESKTOP_DIR="$HOME/Desktop"
    fi
    
    DESKTOP_FILE="$DESKTOP_DIR/MdExplorer.desktop"
    
    if [ -f "$DESKTOP_FILE" ]; then
        print_info "Removing desktop shortcut..."
        rm -f "$DESKTOP_FILE"
        print_success "Desktop shortcut removed"
    fi
}

# Main uninstall function
main() {
    echo "============================================"
    echo "MdExplorer Uninstaller"
    echo "============================================"
    echo
    
    print_warning "This will remove MdExplorer from your system"
    read -p "Are you sure you want to continue? (y/n) " -n 1 -r
    echo
    
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "Uninstallation cancelled"
        exit 0
    fi
    
    # Check permissions
    check_permissions
    
    echo
    print_info "Removing MdExplorer..."
    
    # Remove symlink
    if [ -L "$SYMLINK_PATH" ]; then
        print_info "Removing command line launcher..."
        $SUDO rm -f "$SYMLINK_PATH"
        print_success "Command line launcher removed"
    fi
    
    # Remove desktop entry
    if [ -f "$DESKTOP_ENTRY_PATH" ]; then
        print_info "Removing application menu entry..."
        $SUDO rm -f "$DESKTOP_ENTRY_PATH"
        
        # Update desktop database
        if command -v update-desktop-database >/dev/null 2>&1; then
            $SUDO update-desktop-database 2>/dev/null || true
        fi
        
        print_success "Application menu entry removed"
    fi
    
    # Remove icon
    if [ -f "$ICON_PATH" ]; then
        print_info "Removing icon..."
        $SUDO rm -f "$ICON_PATH"
        print_success "Icon removed"
    fi
    
    # Remove desktop shortcut (user-specific)
    remove_desktop_shortcut
    
    # Remove installation directory
    if [ -d "$INSTALL_DIR" ]; then
        print_info "Removing application files..."
        $SUDO rm -rf "$INSTALL_DIR"
        print_success "Application files removed"
    fi
    
    echo
    echo "============================================"
    print_success "MdExplorer has been uninstalled"
    echo "============================================"
    echo
    print_info "Note: System dependencies (Java, Git, Pandoc) were not removed"
    print_info "as they may be used by other applications."
    echo
}

# Run main function
main "$@"