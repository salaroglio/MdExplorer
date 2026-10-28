#!/usr/bin/env bash
# Compila mde-remote per i server Linux x64 dentro rockylinux:8 (glibc 2.28): il binario chiede al più GLIBC_2.17 e
# gira da CentOS 7 a Ubuntu 24.04 (sprint Cartella-Remota-Via-SSH, F0(a)). Compilato su una distribuzione recente
# chiederebbe la sua glibc (2.34 su Ubuntu 22.04) e non partirebbe sui server più vecchi.
# Uso: ./publish-linux-x64.sh [cartella di uscita]   (default: MdExplorer/Binaries/remote/linux-x64, che va su Git)
# Accanto al binario scrive mde-remote.sources-sha256, l'impronta dei sorgenti da cui è nato: il test
# RemoteAgentBinaryShould la ricalcola e diventa rosso se i sorgenti sono cambiati senza ricompilare.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(dirname "$here")"
out="${1:-$repo/MdExplorer/Binaries/remote/linux-x64}"
mkdir -p "$out"
docker run --rm \
  -v "$repo:/src:ro" -v "$out:/out" -v mde-remote-nuget:/root/.nuget \
  rockylinux:8 bash -c '
    set -e
    dnf install -y -q dotnet-sdk-8.0 clang zlib-devel >/dev/null
    cp -r /src/MdExplorer.Remote.Protocol /src/MdExplorer.RemoteAgent /tmp/
    rm -rf /tmp/MdExplorer.Remote.Protocol/bin /tmp/MdExplorer.Remote.Protocol/obj /tmp/MdExplorer.RemoteAgent/bin /tmp/MdExplorer.RemoteAgent/obj
    dotnet publish /tmp/MdExplorer.RemoteAgent/MdExplorer.RemoteAgent.csproj -c Release -r linux-x64 -o /tmp/out -nologo
    cp /tmp/out/mde-remote /out/
    chown '"$(id -u):$(id -g)"' /out/mde-remote'
# stessa ricetta di RemoteAgentSources.Fingerprint (C#): percorsi relativi ordinati, CR tolti
(cd "$repo" && find MdExplorer.Remote.Protocol MdExplorer.RemoteAgent -type f \( -name '*.cs' -o -name '*.csproj' \) \
   -not -path '*/bin/*' -not -path '*/obj/*' | LC_ALL=C sort | while read -r f; do printf '%s\n' "$f"; tr -d '\r' < "$f"; printf '\n'; done) \
  | sha256sum | cut -d' ' -f1 > "$out/mde-remote.sources-sha256"
glibc="$(objdump -T "$out/mde-remote" | grep -o 'GLIBC_[0-9.]*' | sort -uV | tail -1)"
echo "mde-remote: $(stat -c %s "$out/mde-remote") byte, richiede al più $glibc"
