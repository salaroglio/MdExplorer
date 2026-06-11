#!/bin/bash
# One-click release MdExplorer (Linux).
# Doppio click (o ./release-mdexplorer.sh): decide la versione via git,
# pubblica il servizio .NET in service_payload, builda l'installer Electron
# e pusha il tag di release. Logica in ElectronMdExplorer/scripts/release.js.

cd "$(dirname "$0")"

# Lanciato con doppio click senza terminale? Riapriti in un terminale visibile
# (solo se c'è una sessione grafica: senza display si prosegue inline).
if [ ! -t 1 ] && [ -n "$DISPLAY$WAYLAND_DISPLAY" ]; then
    for term in gnome-terminal konsole x-terminal-emulator xterm; do
        if command -v "$term" >/dev/null 2>&1; then
            case "$term" in
                gnome-terminal)
                    exec "$term" -- bash -c "\"$0\" $*; echo; read -rp 'Premi INVIO per chiudere...'" ;;
                *)
                    exec "$term" -e bash -c "\"$0\" $*; echo; read -rp 'Premi INVIO per chiudere...'" ;;
            esac
        fi
    done
fi

# Node via nvm (preferenza 23, 22, 20), fallback al node di sistema —
# stessa logica di toolIDE/build-electron.sh.
NVM_DIR="${NVM_DIR:-$HOME/.nvm}"
if [ -d "$NVM_DIR/versions/node" ]; then
    for ver in v23 v22 v20; do
        CANDIDATE=$(ls -d "$NVM_DIR/versions/node/$ver"* 2>/dev/null | sort -V | tail -1)
        if [ -n "$CANDIDATE" ] && [ -x "$CANDIDATE/bin/node" ]; then
            export PATH="$CANDIDATE/bin:$PATH"
            break
        fi
    done
fi

# dotnet installato in home (dotnet-install.sh) se non già nel PATH
if ! command -v dotnet >/dev/null 2>&1 && [ -x "$HOME/.dotnet/dotnet" ]; then
    export PATH="$HOME/.dotnet:$PATH"
fi

if ! command -v node >/dev/null 2>&1; then
    echo "ERRORE: Node.js non trovato. Installa con: nvm install 23" >&2
    exit 1
fi
if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERRORE: dotnet SDK non trovato nel PATH." >&2
    exit 1
fi

node ElectronMdExplorer/scripts/release.js "$@"
