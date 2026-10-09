#!/usr/bin/env bash
# Prepara i pacchetti delle grammatiche tree-sitter da pubblicare su mdexplorer.net (grafo del codice, D8).
#
# Per ogni grammatica e piattaforma produce la cartella `<out>/grammar-<id>-<rid>/` con:
#   - `manifest`           il manifest JSON (stesso contratto degli add-on: name, version, downloadUrl, size, checksum)
#   - `grammar-<id>-<rid>.zip`   lo zip con il solo file della grammatica
# e si serve così: GET {GrammarApiUrl}/grammar-<id>-<rid>/manifest → downloadUrl (relativo alla base).
#
# Le grammatiche vengono dal NuGet TreeSitter.DotNet (MIT): la versione del pacchetto È la versione della grammatica,
# perché le `.so`/`.dll` sono compilate per l'ABI di quel core.
#
# Uso:  scripts/codegraph/pack-grammars.sh [out-dir] [nuget-version] [rid ...]
#       scripts/codegraph/pack-grammars.sh out/grammars 1.3.0 linux-x64 win-x64
set -euo pipefail
OUT="$(realpath -m "${1:-out/grammars}")"
VERSION="${2:-1.3.0}"
shift $(( $# >= 2 ? 2 : $# )) || true
RIDS=("$@"); [ ${#RIDS[@]} -eq 0 ] && RIDS=(linux-x64 win-x64)
GRAMMARS=(typescript javascript python java go rust)
PKG="${NUGET_PACKAGES:-$HOME/.nuget/packages}/treesitter.dotnet/$VERSION/runtimes"
[ -d "$PKG" ] || { echo "NuGet TreeSitter.DotNet $VERSION non trovato in $PKG: fai un restore di MdExplorer.Features prima." >&2; exit 1; }
command -v zip >/dev/null || { echo "serve 'zip'" >&2; exit 1; }

mkdir -p "$OUT"
for rid in "${RIDS[@]}"; do
  for g in "${GRAMMARS[@]}"; do
    case "$rid" in win-*) file="tree-sitter-$g.dll";; *) file="libtree-sitter-$g.so";; esac
    src="$PKG/$rid/native/$file"
    [ -f "$src" ] || { echo "manca $src: salto $g per $rid" >&2; continue; }
    id="grammar-$g-$rid"; dir="$OUT/$id"; mkdir -p "$dir"
    ( cd "$(dirname "$src")" && zip -q -j "$dir/$id.zip" "$file" )
    size=$(stat -c %s "$dir/$id.zip"); sum=$(sha256sum "$dir/$id.zip" | cut -d' ' -f1)
    cat > "$dir/manifest" <<JSON
{
  "name": "tree-sitter $g ($rid)",
  "version": "$VERSION",
  "minAppVersion": "3.15.0",
  "downloadUrl": "$id/$id.zip",
  "size": $size,
  "checksum": "$sum",
  "releaseNotes": "Grammatica tree-sitter '$g' dal NuGet TreeSitter.DotNet $VERSION (MIT)."
}
JSON
    echo "$id: $file ($size byte)"
  done
done
echo "pronto in $OUT — pubblica le cartelle sotto {GrammarApiUrl}/"
