#!/usr/bin/env bash
# Raccoglitore di «Invia diagnostica» (sprint docs-internal/Sprints/2026-11-06-Invia-Diagnostica.md, D4).
# Gira sulla VM di sviluppo con un timer systemd utente ogni 5 minuti: guarda sul relay (errantia) se sono arrivati
# pacchetti, li copia in ~/diagnostics/<data>_<codice>/, li scompatta, li CANCELLA dal relay, avvisa sul desktop.
# Allo stesso giro toglie da ~/diagnostics i pacchetti più vecchi di 10 giorni (decisione dell'utente, 06/11/2026).
#
# Variabili (con i valori di produzione):
#   DIAG_REMOTE=root@errantia.net      chi ospita i pacchetti; vuoto = cartella locale (per le prove)
#   DIAG_REMOTE_DIR=/var/lib/mdexplorer-diag
#   DIAG_LOCAL=$HOME/diagnostics
#   DIAG_KEEP_DAYS=10
#   DIAG_DISPLAY=:10                   il desktop della sessione xrdp
set -euo pipefail

REMOTE="${DIAG_REMOTE-root@errantia.net}"
REMOTE_DIR="${DIAG_REMOTE_DIR:-/var/lib/mdexplorer-diag}"
LOCAL="${DIAG_LOCAL:-$HOME/diagnostics}"
KEEP_DAYS="${DIAG_KEEP_DAYS:-10}"
DISPLAY_FOR_NOTICE="${DIAG_DISPLAY:-:10}"
LOG="$LOCAL/raccolti.log"

mkdir -p "$LOCAL"
log() { printf '[%s] %s\n' "$(date '+%Y-%m-%d %H:%M:%S')" "$*" >> "$LOG"; }

# Comandi verso il relay: via SSH, o sul disco se DIAG_REMOTE è vuoto.
on_remote() {
  if [ -z "$REMOTE" ]; then bash -c "$1"; else ssh -o BatchMode=yes -o ConnectTimeout=15 "$REMOTE" "$1"; fi
}
copy_from_remote() { # $1 = codice, $2 = cartella di destinazione
  if [ -z "$REMOTE" ]; then cp -r "$REMOTE_DIR/$1/." "$2/"; else scp -q -o BatchMode=yes -r "$REMOTE:$REMOTE_DIR/$1/." "$2/"; fi
}

notify() {
  # Il bus della sessione grafica: dal timer systemd non è nell'ambiente.
  DISPLAY="$DISPLAY_FOR_NOTICE" DBUS_SESSION_BUS_ADDRESS="${DBUS_SESSION_BUS_ADDRESS:-unix:path=/run/user/$(id -u)/bus}" \
    notify-send --app-name="MdExplorer" --icon=dialog-information "$1" "$2" 2>/dev/null || log "avviso sul desktop non riuscito: $1"
}

codes=$(on_remote "ls -1 '$REMOTE_DIR' 2>/dev/null || true" | grep -E '^[A-Z2-9]{4}-[A-Z2-9]{2}$' || true)
for code in $codes; do
  tmp=$(mktemp -d "$LOCAL/.in-arrivo-XXXXXX")
  if ! copy_from_remote "$code" "$tmp" || [ ! -s "$tmp/package.zip" ]; then
    log "ERRORE: $code non copiato, resta sul relay"; rm -rf "$tmp"; continue
  fi
  remote_size=$(on_remote "stat -c %s '$REMOTE_DIR/$code/package.zip'" || echo "?")
  local_size=$(stat -c %s "$tmp/package.zip")
  if [ "$remote_size" != "$local_size" ]; then
    log "ERRORE: $code copiato a metà ($local_size di $remote_size byte), resta sul relay"; rm -rf "$tmp"; continue
  fi
  received=$(python3 -c "import json,sys; print(json.load(open(sys.argv[1])).get('receivedAt','')[:10])" "$tmp/meta.json" 2>/dev/null || date +%F)
  dest="$LOCAL/${received:-$(date +%F)}_$code"
  rm -rf "$dest"; mv "$tmp" "$dest"
  (cd "$dest" && unzip -q -o package.zip -d contenuto) || log "ATTENZIONE: $code non si scompatta"
  on_remote "rm -rf '$REMOTE_DIR/$code'"
  info=$(python3 -c "import json,sys; m=json.load(open(sys.argv[1])); print(f\"MDE {m.get('version') or '?'}, {m.get('os') or '?'}, {round(m.get('bytes',0)/1024)} KB\")" "$dest/meta.json" 2>/dev/null || echo "")
  log "raccolto $code in $dest ($info), tolto dal relay"
  notify "Diagnostica $code arrivata" "$info — $dest"
done

# I pacchetti raccolti restano 10 giorni.
find "$LOCAL" -mindepth 1 -maxdepth 1 -type d -name '20*_*' -mtime +"$KEEP_DAYS" -print0 2>/dev/null | while IFS= read -r -d '' old; do
  rm -rf "$old"; log "tolto $(basename "$old"): più vecchio di $KEEP_DAYS giorni"
done
