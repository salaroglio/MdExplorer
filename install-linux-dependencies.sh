#!/bin/bash

# MdExplorer Linux Dependencies Installation Script
# Author: MdExplorer Team
# Date: 2025-08-04
# Description: Installs all required dependencies for running MdExplorer on Linux

set -e  # Exit on error

echo "================================================"
echo "MdExplorer Linux Dependencies Installation"
echo "================================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

# Detect Linux distribution
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$NAME
    VER=$VERSION_ID
else
    print_error "Cannot detect Linux distribution"
    exit 1
fi

echo "Detected: $OS $VER"
echo ""

# Check architecture
ARCH=$(uname -m)
echo "Architecture: $ARCH"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    print_warning "This script needs sudo privileges to install packages"
    print_warning "Please run: sudo $0"
    echo ""
fi

echo "Installing dependencies..."
echo ""

# 1. Install libssl 1.1 for .NET Core 3.1
echo "1. Installing libssl 1.1 (required for .NET Core 3.1)..."
if [[ "$OS" == *"Ubuntu"* ]] || [[ "$OS" == *"Debian"* ]]; then
    
    # Check if libssl1.1 is already installed
    if dpkg -l | grep -q libssl1.1; then
        print_status "libssl1.1 is already installed"
    else
        print_warning "libssl1.1 not found, downloading..."
        
        # Download appropriate version based on architecture
        if [ "$ARCH" = "x86_64" ]; then
            LIBSSL_URL="http://archive.ubuntu.com/ubuntu/pool/main/o/openssl/libssl1.1_1.1.1f-1ubuntu2_amd64.deb"
            LIBSSL_FILE="libssl1.1_amd64.deb"
        elif [ "$ARCH" = "aarch64" ]; then
            LIBSSL_URL="http://ports.ubuntu.com/pool/main/o/openssl/libssl1.1_1.1.1f-1ubuntu2_arm64.deb"
            LIBSSL_FILE="libssl1.1_arm64.deb"
        else
            print_error "Unsupported architecture: $ARCH"
            exit 1
        fi
        
        # Download and install
        wget -O "/tmp/$LIBSSL_FILE" "$LIBSSL_URL"
        sudo dpkg -i "/tmp/$LIBSSL_FILE"
        rm "/tmp/$LIBSSL_FILE"
        print_status "libssl1.1 installed successfully"
    fi
    
elif [[ "$OS" == *"Fedora"* ]] || [[ "$OS" == *"Red Hat"* ]] || [[ "$OS" == *"CentOS"* ]]; then
    sudo dnf install -y compat-openssl10
    print_status "OpenSSL compatibility package installed"
    
elif [[ "$OS" == *"Arch"* ]]; then
    print_warning "For Arch Linux, you may need to install openssl-1.1 from AUR"
    echo "yay -S openssl-1.1"
    
else
    print_warning "Unknown distribution. Please install libssl 1.1 manually"
fi

echo ""

# 2. Install .NET Core 3.1 Runtime (if not present)
echo "2. Checking .NET Core 3.1..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_status ".NET SDK found: $DOTNET_VERSION"
    
    # Check for 3.1 runtime
    if dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 3.1"; then
        print_status ".NET Core 3.1 runtime is installed"
    else
        print_warning ".NET Core 3.1 runtime not found"
        echo "Install from: https://dotnet.microsoft.com/download/dotnet/3.1"
    fi
else
    print_error ".NET Core not found"
    echo "Please install from: https://dotnet.microsoft.com/download/dotnet/3.1"
fi

echo ""

# 3. Install GraphViz (for PlantUML diagrams)
echo "3. Installing GraphViz..."
if command -v dot &> /dev/null; then
    print_status "GraphViz is already installed"
else
    if [[ "$OS" == *"Ubuntu"* ]] || [[ "$OS" == *"Debian"* ]]; then
        sudo apt-get update
        sudo apt-get install -y graphviz
    elif [[ "$OS" == *"Fedora"* ]] || [[ "$OS" == *"Red Hat"* ]]; then
        sudo dnf install -y graphviz
    elif [[ "$OS" == *"Arch"* ]]; then
        sudo pacman -S --noconfirm graphviz
    fi
    print_status "GraphViz installed"
fi

echo ""

# 4. Install Java Runtime (for PlantUML)
echo "4. Installing Java Runtime..."
if command -v java &> /dev/null; then
    JAVA_VERSION=$(java -version 2>&1 | head -n 1)
    print_status "Java is already installed: $JAVA_VERSION"
else
    if [[ "$OS" == *"Ubuntu"* ]] || [[ "$OS" == *"Debian"* ]]; then
        sudo apt-get install -y default-jre
    elif [[ "$OS" == *"Fedora"* ]] || [[ "$OS" == *"Red Hat"* ]]; then
        sudo dnf install -y java-latest-openjdk
    elif [[ "$OS" == *"Arch"* ]]; then
        sudo pacman -S --noconfirm jre-openjdk
    fi
    print_status "Java Runtime installed"
fi

echo ""

# 5. Install Pandoc (for document export)
echo "5. Installing Pandoc..."
if command -v pandoc &> /dev/null; then
    print_status "Pandoc is already installed"
else
    if [[ "$OS" == *"Ubuntu"* ]] || [[ "$OS" == *"Debian"* ]]; then
        sudo apt-get install -y pandoc
    elif [[ "$OS" == *"Fedora"* ]] || [[ "$OS" == *"Red Hat"* ]]; then
        sudo dnf install -y pandoc
    elif [[ "$OS" == *"Arch"* ]]; then
        sudo pacman -S --noconfirm pandoc
    fi
    print_status "Pandoc installed"
fi

echo ""

# 6. Install additional libraries
echo "6. Installing additional libraries..."
if [[ "$OS" == *"Ubuntu"* ]] || [[ "$OS" == *"Debian"* ]]; then
    sudo apt-get install -y \
        libgdiplus \
        libc6-dev \
        libicu-dev \
        libssl-dev \
        ca-certificates \
        libsecret-1-0
    print_status "Additional libraries installed"
elif [[ "$OS" == *"Fedora"* ]] || [[ "$OS" == *"Red Hat"* ]]; then
    sudo dnf install -y \
        libgdiplus \
        glibc-devel \
        libicu \
        openssl-devel \
        ca-certificates \
        libsecret
    print_status "Additional libraries installed"
fi

echo ""

# 7. Install Node.js and npm (for frontend)
echo "7. Checking Node.js and npm..."
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    print_status "Node.js is already installed: $NODE_VERSION"
else
    print_warning "Node.js not found. Installing..."
    if [[ "$OS" == *"Ubuntu"* ]] || [[ "$OS" == *"Debian"* ]]; then
        curl -fsSL https://deb.nodesource.com/setup_lts.x | sudo -E bash -
        sudo apt-get install -y nodejs
    elif [[ "$OS" == *"Fedora"* ]] || [[ "$OS" == *"Red Hat"* ]]; then
        sudo dnf install -y nodejs npm
    elif [[ "$OS" == *"Arch"* ]]; then
        sudo pacman -S --noconfirm nodejs npm
    fi
    print_status "Node.js installed"
fi

if command -v npm &> /dev/null; then
    NPM_VERSION=$(npm --version)
    print_status "npm is already installed: $NPM_VERSION"
else
    print_error "npm not found"
fi

echo ""

# Summary
echo "================================================"
echo "Installation Summary"
echo "================================================"
echo ""

# Check all dependencies
echo "Checking all dependencies..."
echo ""

check_command() {
    if command -v $1 &> /dev/null; then
        print_status "$1 is available"
        return 0
    else
        print_error "$1 is NOT available"
        return 1
    fi
}

check_library() {
    if ldconfig -p | grep -q $1; then
        print_status "$1 library is available"
        return 0
    else
        print_error "$1 library is NOT available"
        return 1
    fi
}

ALL_GOOD=true

check_command dotnet || ALL_GOOD=false
check_command java || ALL_GOOD=false
check_command dot || ALL_GOOD=false
check_command pandoc || ALL_GOOD=false
check_command node || ALL_GOOD=false
check_command npm || ALL_GOOD=false
check_library libssl || ALL_GOOD=false
check_library libgdiplus || ALL_GOOD=false

echo ""

if [ "$ALL_GOOD" = true ]; then
    print_status "All dependencies are installed successfully!"
    echo ""
    echo "You can now build and run MdExplorer:"
    echo "  cd MdExplorer"
    echo "  dotnet build"
    echo "  dotnet run --project MdExplorer.Service.csproj"
else
    print_warning "Some dependencies are missing. Please install them manually."
fi

echo ""
echo "================================================"
echo "Installation complete!"
echo "================================================"