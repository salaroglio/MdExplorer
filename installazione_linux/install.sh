#!/bin/bash

# MdExplorer Universal Linux Installer
# Supports: Ubuntu, Debian, Fedora, openSUSE, Arch Linux

set -e

# Script version
VERSION="1.0.0"

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

# Detect Linux distribution
detect_distro() {
    if [ -f /etc/os-release ]; then
        . /etc/os-release
        DISTRO=$ID
        DISTRO_FAMILY=$ID_LIKE
        DISTRO_VERSION=$VERSION_ID
        DISTRO_NAME=$PRETTY_NAME
    else
        print_error "Cannot detect Linux distribution"
        exit 1
    fi
    
    # Determine package manager
    case "$DISTRO" in
        ubuntu|debian|linuxmint|pop)
            PKG_MANAGER="apt"
            PKG_UPDATE="apt update"
            PKG_INSTALL="apt install -y"
            JAVA_PACKAGE="default-jre"
            ;;
        fedora|rhel|centos)
            PKG_MANAGER="dnf"
            PKG_UPDATE="dnf check-update"
            PKG_INSTALL="dnf install -y"
            JAVA_PACKAGE="java-11-openjdk"
            ;;
        opensuse*|sles)
            PKG_MANAGER="zypper"
            PKG_UPDATE="zypper refresh"
            PKG_INSTALL="zypper install -y"
            JAVA_PACKAGE="java-11-openjdk"
            ;;
        arch|manjaro)
            PKG_MANAGER="pacman"
            PKG_UPDATE="pacman -Sy"
            PKG_INSTALL="pacman -S --noconfirm"
            JAVA_PACKAGE="jre-openjdk"
            ;;
        *)
            print_error "Unsupported distribution: $DISTRO"
            print_info "You may need to install dependencies manually"
            MANUAL_MODE=true
            ;;
    esac
}

# Check if running with proper permissions
check_permissions() {
    if [ "$EUID" -eq 0 ]; then
        print_info "Running as root"
        SUDO=""
    else
        print_info "Running as user, will use sudo when needed"
        SUDO="sudo"
        
        # Test sudo access
        if ! $SUDO -n true 2>/dev/null; then
            print_warning "This installer needs sudo privileges to install system dependencies"
            print_info "You will be prompted for your password"
            $SUDO true || {
                print_error "Failed to obtain sudo privileges"
                exit 1
            }
        fi
    fi
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check and install dependency
check_dependency() {
    local command=$1
    local package=$2
    local description=$3
    
    if command_exists "$command"; then
        print_success "$description is already installed"
        return 0
    else
        print_warning "$description is not installed"
        
        if [ "$MANUAL_MODE" = true ]; then
            print_info "Please install $description manually"
            return 1
        fi
        
        read -p "Do you want to install $description? (y/n) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            print_info "Installing $description..."
            $SUDO $PKG_INSTALL $package || {
                print_error "Failed to install $description"
                return 1
            }
            print_success "$description installed successfully"
        else
            print_warning "Skipping $description installation"
            return 1
        fi
    fi
}

# Install AppImage
install_appimage() {
    local appimage_file=$(find . -maxdepth 1 -name "*.AppImage" -type f | head -n 1)
    
    if [ -z "$appimage_file" ]; then
        print_error "No AppImage file found in current directory"
        exit 1
    fi
    
    print_info "Found AppImage: $(basename "$appimage_file")"
    
    # Create installation directory
    print_info "Creating installation directory..."
    $SUDO mkdir -p "$INSTALL_DIR"
    
    # Copy AppImage
    print_info "Installing MdExplorer..."
    $SUDO cp "$appimage_file" "$INSTALL_DIR/mdexplorer.AppImage"
    $SUDO chmod +x "$INSTALL_DIR/mdexplorer.AppImage"
    
    # Copy icon if exists
    if [ -f "mdexplorer.png" ]; then
        print_info "Installing icon..."
        $SUDO mkdir -p "$(dirname "$ICON_PATH")"
        $SUDO cp "mdexplorer.png" "$ICON_PATH"
    fi
    
    # Create symlink in PATH
    print_info "Creating command line launcher..."
    $SUDO ln -sf "$INSTALL_DIR/mdexplorer.AppImage" "/usr/local/bin/mdexplorer"
    
    print_success "MdExplorer installed to $INSTALL_DIR"
}

# Create desktop entry
create_desktop_entry() {
    print_info "Creating application menu entry..."
    
    $SUDO tee "$DESKTOP_ENTRY_PATH" > /dev/null << EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=MdExplorer
Comment=Markdown file management and editing
Exec=$INSTALL_DIR/mdexplorer.AppImage %U
Icon=$ICON_PATH
Terminal=false
Categories=Development;Office;TextEditor;
MimeType=text/markdown;text/x-markdown;
StartupNotify=true
EOF
    
    # Update desktop database
    if command_exists "update-desktop-database"; then
        $SUDO update-desktop-database 2>/dev/null || true
    fi
    
    print_success "Application menu entry created"
}

# Create desktop shortcut
create_desktop_shortcut() {
    print_info "Creating desktop shortcut..."
    
    # Find desktop directory
    if command_exists "xdg-user-dir"; then
        DESKTOP_DIR=$(xdg-user-dir DESKTOP 2>/dev/null)
    fi
    
    if [ -z "$DESKTOP_DIR" ] || [ ! -d "$DESKTOP_DIR" ]; then
        DESKTOP_DIR="$HOME/Desktop"
    fi
    
    if [ ! -d "$DESKTOP_DIR" ]; then
        print_warning "Desktop directory not found, skipping desktop shortcut"
        return
    fi
    
    # Create desktop file
    cat > "$DESKTOP_DIR/MdExplorer.desktop" << EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=MdExplorer
Comment=Markdown file management and editing
Exec=$INSTALL_DIR/mdexplorer.AppImage %U
Icon=$ICON_PATH
Terminal=false
Categories=Development;Office;TextEditor;
MimeType=text/markdown;text/x-markdown;
StartupNotify=true
EOF
    
    # Make it executable
    chmod +x "$DESKTOP_DIR/MdExplorer.desktop"
    
    # Mark as trusted (for GNOME)
    if command_exists "gio"; then
        gio set "$DESKTOP_DIR/MdExplorer.desktop" metadata::trusted true 2>/dev/null || true
    fi
    
    print_success "Desktop shortcut created"
}

# Main installation
main() {
    echo "============================================"
    echo "MdExplorer Linux Installer v$VERSION"
    echo "============================================"
    echo
    
    # Detect distribution
    print_info "Detecting Linux distribution..."
    detect_distro
    print_success "Detected: $DISTRO_NAME"
    
    # Check permissions
    check_permissions
    
    # Update package lists
    if [ "$MANUAL_MODE" != true ]; then
        print_info "Updating package lists..."
        $SUDO $PKG_UPDATE 2>/dev/null || true
    fi
    
    echo
    echo "Checking system dependencies..."
    echo "==============================="
    
    # Check Java
    JAVA_OK=true
    if ! check_dependency "java" "$JAVA_PACKAGE" "Java Runtime Environment"; then
        JAVA_OK=false
    fi
    
    # Check Git
    GIT_OK=true
    if ! check_dependency "git" "git" "Git version control"; then
        GIT_OK=false
    fi
    
    # Check Pandoc
    PANDOC_OK=true
    if ! check_dependency "pandoc" "pandoc" "Pandoc document converter"; then
        PANDOC_OK=false
    fi
    
    echo
    echo "Installing MdExplorer..."
    echo "======================="
    
    # Install AppImage
    install_appimage
    
    # Create desktop entry
    create_desktop_entry
    
    # Create desktop shortcut
    create_desktop_shortcut
    
    echo
    echo "============================================"
    print_success "Installation Complete!"
    echo "============================================"
    echo
    echo "MdExplorer has been installed successfully."
    echo
    echo "You can now:"
    echo "  • Launch from Applications menu"
    echo "  • Use desktop shortcut"
    echo "  • Run from terminal: mdexplorer"
    echo
    
    # Show warnings for missing dependencies
    if [ "$JAVA_OK" = false ]; then
        print_warning "Java is not installed - PlantUML features will not work"
    fi
    if [ "$GIT_OK" = false ]; then
        print_warning "Git is not installed - Version control features will not work"
    fi
    if [ "$PANDOC_OK" = false ]; then
        print_warning "Pandoc is not installed - Document export features will be limited"
    fi
    
    # LaTeX reminder
    echo
    print_info "For full PDF export support, you may also want to install LaTeX:"
    case "$PKG_MANAGER" in
        apt)
            echo "  sudo apt install texlive-xetex texlive-fonts-recommended"
            ;;
        dnf)
            echo "  sudo dnf install texlive-xetex texlive-collection-fontsrecommended"
            ;;
        zypper)
            echo "  sudo zypper install texlive-xetex"
            ;;
        pacman)
            echo "  sudo pacman -S texlive-core texlive-fontsextra"
            ;;
    esac
    echo
}

# Run main function
main "$@"