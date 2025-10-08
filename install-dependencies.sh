#!/bin/bash

# Script per installare le dipendenze necessarie per MdExplorer con supporto AI
# Supporta distribuzioni basate su Debian/Ubuntu e RedHat/Fedora

echo "=== MdExplorer - Installazione Dipendenze ==="
echo

# Rileva il sistema operativo
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$ID
    VER=$VERSION_ID
else
    echo "Sistema operativo non riconosciuto"
    exit 1
fi

echo "Sistema rilevato: $OS $VER"
echo

# Funzione per verificare se un comando esiste
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Funzione per verificare se una libreria è installata
lib_exists() {
    ldconfig -p | grep -q "$1"
}

# Lista delle dipendenze richieste
REQUIRED_LIBS=(
    "libgomp.so.1"      # OpenMP support per LLamaSharp
    "libstdc++.so.6"    # C++ standard library
    "libm.so.6"         # Math library
    "libgcc_s.so.1"     # GCC support library
    "libpthread.so.0"   # POSIX threads
    "libdl.so.2"        # Dynamic linker
    "librt.so.1"        # Realtime extensions
)

# Pacchetti da installare in base al sistema
DEBIAN_PACKAGES=(
    "libgomp1"          # OpenMP
    "libstdc++6"        # C++ stdlib
    "libc6"             # glibc
    "build-essential"   # Compilatori e tools base
)

REDHAT_PACKAGES=(
    "libgomp"           # OpenMP
    "libstdc++"         # C++ stdlib
    "glibc"             # glibc
    "gcc"               # Compilatore
    "gcc-c++"           # C++ compiler
)

# Verifica dipendenze mancanti
echo "Verifica dipendenze..."
MISSING_LIBS=()

for lib in "${REQUIRED_LIBS[@]}"; do
    if lib_exists "$lib"; then
        echo "✓ $lib trovata"
    else
        echo "✗ $lib mancante"
        MISSING_LIBS+=("$lib")
    fi
done

echo

# Se ci sono librerie mancanti, installa i pacchetti necessari
if [ ${#MISSING_LIBS[@]} -gt 0 ]; then
    echo "Installazione pacchetti mancanti..."
    
    case "$OS" in
        ubuntu|debian|linuxmint|pop)
            echo "Aggiornamento repository..."
            sudo apt-get update
            
            echo "Installazione pacchetti..."
            sudo apt-get install -y "${DEBIAN_PACKAGES[@]}"
            ;;
            
        fedora|rhel|centos|rocky|almalinux)
            echo "Installazione pacchetti..."
            sudo dnf install -y "${REDHAT_PACKAGES[@]}"
            ;;
            
        arch|manjaro)
            echo "Installazione pacchetti..."
            sudo pacman -S --noconfirm base-devel gcc
            ;;
            
        opensuse*)
            echo "Installazione pacchetti..."
            sudo zypper install -y gcc gcc-c++ libgomp1
            ;;
            
        *)
            echo "Distribuzione $OS non supportata automaticamente."
            echo "Installa manualmente i seguenti pacchetti:"
            echo "- libgomp (OpenMP)"
            echo "- libstdc++ (C++ standard library)"
            echo "- build tools (gcc/g++)"
            exit 1
            ;;
    esac
    
    # Aggiorna la cache delle librerie
    sudo ldconfig
    
    echo
    echo "Verifica post-installazione..."
    for lib in "${REQUIRED_LIBS[@]}"; do
        if lib_exists "$lib"; then
            echo "✓ $lib OK"
        else
            echo "✗ $lib ancora mancante"
        fi
    done
else
    echo "Tutte le dipendenze sono già installate!"
fi

echo
echo "=== Configurazione percorso librerie native ==="

# Verifica se LD_LIBRARY_PATH è configurato per LLamaSharp
LLAMA_LIB_PATH="/home/carlo/Documents/sviluppo/MdExplorer/MdExplorer/bin/Debug/net8.0/linux-x64"

if [ -d "$LLAMA_LIB_PATH" ]; then
    echo "Directory librerie native trovata: $LLAMA_LIB_PATH"
    
    # Verifica le librerie LLamaSharp
    if [ -f "$LLAMA_LIB_PATH/libllama.so" ] && [ -f "$LLAMA_LIB_PATH/libggml.so" ]; then
        echo "✓ Librerie LLamaSharp presenti"
        
        # Test caricamento librerie
        echo
        echo "Test caricamento librerie..."
        export LD_LIBRARY_PATH="$LLAMA_LIB_PATH:$LD_LIBRARY_PATH"
        
        if ldd "$LLAMA_LIB_PATH/libllama.so" 2>&1 | grep -q "not found"; then
            echo "✗ Problemi nel caricamento di libllama.so:"
            ldd "$LLAMA_LIB_PATH/libllama.so" 2>&1 | grep "not found"
        else
            echo "✓ libllama.so caricabile correttamente"
        fi
        
        if ldd "$LLAMA_LIB_PATH/libggml.so" 2>&1 | grep -q "not found"; then
            echo "✗ Problemi nel caricamento di libggml.so:"
            ldd "$LLAMA_LIB_PATH/libggml.so" 2>&1 | grep "not found"
        else
            echo "✓ libggml.so caricabile correttamente"
        fi
    else
        echo "✗ Librerie LLamaSharp non trovate. Esegui prima 'dotnet build'"
    fi
else
    echo "✗ Directory librerie native non trovata. Esegui prima 'dotnet build'"
fi

echo
echo "=== Verifica .NET SDK ==="

if command_exists dotnet; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✓ .NET SDK installato: $DOTNET_VERSION"
    
    # Verifica che sia almeno .NET 8
    if [[ "$DOTNET_VERSION" =~ ^8\. ]]; then
        echo "✓ Versione .NET 8 corretta"
    else
        echo "⚠ Attenzione: Il progetto richiede .NET 8.0"
        echo "  Installa da: https://dotnet.microsoft.com/download/dotnet/8.0"
    fi
else
    echo "✗ .NET SDK non trovato"
    echo "  Installa da: https://dotnet.microsoft.com/download/dotnet/8.0"
fi

echo
echo "=== Riepilogo ==="
echo
echo "Per eseguire MdExplorer con supporto AI:"
echo "1. Usa lo script run-mdexplorer.sh che configura automaticamente LD_LIBRARY_PATH"
echo "2. Oppure esegui manualmente:"
echo "   export LD_LIBRARY_PATH=\"$LLAMA_LIB_PATH:\$LD_LIBRARY_PATH\""
echo "   dotnet run --project MdExplorer/MdExplorer.Service.csproj"
echo
echo "Script completato!"