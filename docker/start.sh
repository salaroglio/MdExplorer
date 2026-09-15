#!/usr/bin/env bash
# MdExplorer — start container + open browser (macOS / Linux)
# Usage: ./start.sh

set -e

URL="http://127.0.0.1:5000/client2/index.html"
PROBE="http://127.0.0.1:5000/"
TIMEOUT_SEC=90

echo "Starting MdExplorer container..."
docker compose up -d

echo "Waiting for MdExplorer to respond on $PROBE (timeout: ${TIMEOUT_SEC}s)..."
elapsed=0
while [ "$elapsed" -lt "$TIMEOUT_SEC" ]; do
    # Any HTTP code (200/404/...) means Kestrel is alive; only exit code 7
    # (connection refused) keeps us polling.
    code=$(curl -s -o /dev/null -w "%{http_code}" --max-time 2 "$PROBE" 2>/dev/null || echo "000")
    if [ "$code" != "000" ]; then
        break
    fi
    sleep 2
    elapsed=$((elapsed + 2))
    printf "."
done
echo

if [ "$elapsed" -ge "$TIMEOUT_SEC" ]; then
    echo "WARNING: MdExplorer didn't respond within ${TIMEOUT_SEC}s. Opening URL anyway." >&2
else
    echo "MdExplorer ready (${elapsed}s)."
fi

case "$(uname -s)" in
    Darwin*) open "$URL" ;;
    Linux*)
        if   command -v xdg-open >/dev/null 2>&1; then xdg-open "$URL"
        elif command -v gio      >/dev/null 2>&1; then gio open  "$URL"
        else echo "No browser launcher found. Open manually: $URL"; fi
        ;;
    *) echo "Open manually: $URL" ;;
esac
