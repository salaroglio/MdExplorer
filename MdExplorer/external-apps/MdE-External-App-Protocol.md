# MdE External App Protocol

Specifica per sviluppare applicazioni esterne che possono essere ospitate nel pannello destro di MdExplorer.

---

## Come funziona

MdExplorer legge il file `.mdeapps.json` nella root del progetto, crea nodi speciali nel tree con icona configurabile, e quando l'utente clicca il nodo:

1. Avvia l'eseguibile dell'app esterna con argomenti specifici
2. Fa polling su `http://localhost:<PORT>/` finché risponde 200
3. Carica la risposta in un `WebContentsView` sovrapposto al pannello destro
4. Quando l'utente naviga altrove, nasconde la view (il processo rimane vivo)
5. Quando l'utente torna sul nodo, mostra di nuovo la view senza rispawnare

---

## Argomenti passati all'app esterna

```
myapp.exe --mde-embedded --port <PORT> --mde-host <MDE_URL> [args personalizzati]
```

| Argomento | Tipo | Descrizione |
|-----------|------|-------------|
| `--mde-embedded` | flag | Indica che l'app gira embedded in MdE |
| `--port <PORT>` | number | Porta su cui avviare l'HTTP server |
| `--mde-host <URL>` | string | URL base di MdExplorer (es. `http://localhost:48123`) |

---

## Cosa deve fare l'app esterna

1. **Rilevare `--mde-embedded`** negli argomenti → non aprire una finestra **visibile** (creare comunque una `BrowserWindow` nascosta con `show: false` come parent per eventuali dialog nativi)
2. **Avviare un HTTP server** sulla `<PORT>` indicata
3. **Rispondere `200 OK`** a `GET /` con l'HTML dell'interfaccia
4. **Terminare pulitamente** quando il processo riceve un segnale di kill

---

## Esempio minimo — Node.js

```javascript
// server.js
const http = require('http');
const args = process.argv.slice(2);
const port = parseInt(args[args.indexOf('--port') + 1]);

http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
  res.end('<h1>La mia app</h1>');
}).listen(port, '127.0.0.1', () => {
  console.log(`Avviato su porta ${port}`);
});
```

## Esempio minimo — Electron

```javascript
// main.js
const { app, BrowserWindow } = require('electron');
const http = require('http');

const args = process.argv.slice(2);
const isMdeEmbedded = args.includes('--mde-embedded');
const port = isMdeEmbedded ? parseInt(args[args.indexOf('--port') + 1]) : 3000;

app.whenReady().then(() => {
  if (isMdeEmbedded) {
    // Nessuna finestra visibile, ma BrowserWindow nascosta per dialog nativi
    new BrowserWindow({ show: false, width: 1, height: 1 });
    startHttpServer(port);
  } else {
    // Avvia normalmente con BrowserWindow
    const win = new BrowserWindow({ width: 1200, height: 800 });
    win.loadFile('index.html');
  }
});

function startHttpServer(port) {
  http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'text/html' });
    res.end('<h1>La mia app embedded</h1>');
  }).listen(port, '127.0.0.1');
}
```

---

## Schema `.mdeapps.json`

```json
{
  "version": "1",
  "apps": [
    {
      "id": "my-app",
      "name": "My App",
      "description": "Tooltip opzionale nel tree",
      "icon": "dashboard",
      "executable": ".mde/apps/myapp.exe",
      "args": ["--extra-arg"],
      "treePosition": "bottom",
      "singleton": true
    }
  ]
}
```

| Campo | Tipo | Obbligatorio | Descrizione |
|-------|------|:---:|-------------|
| `id` | string | ✓ | Identificatore univoco (no spazi) |
| `name` | string | ✓ | Nome visualizzato nel tree |
| `executable` | string | ✓ | Path assoluto o relativo alla root del progetto |
| `args` | string[] | | Argomenti extra (MdE aggiunge automaticamente `--mde-embedded --port --mde-host`) |
| `icon` | string | | Nome icona Material (default: `launch`) → [lista icone](https://fonts.google.com/icons) |
| `description` | string | | Tooltip nel tree |
| `treePosition` | `"top"` \| `"bottom"` | | Posizione nel tree (default: `"bottom"`) |
| `singleton` | boolean | | `true` = riusa il processo (default: `true`) |

### Path relativi

`executable` può essere relativo alla root del progetto:

```json
"executable": ".mde/apps/myapp.exe"
```

Viene risolto in `<ProjectRoot>/.mde/apps/myapp.exe`.

---

## Gestione del lifecycle

| Evento | Comportamento MdE |
|--------|-------------------|
| Clic sul nodo | Spawn processo + poll HTTP (max 10s) |
| Navigazione ad altro nodo | View nascosta, processo **rimane vivo** |
| Ritorno al nodo | View mostrata, **nessun nuovo spawn** |
| Quit MdE | `taskkill /F /T /PID` → processo terminato |

---

## Icone Material disponibili (esempi)

`dashboard`, `code`, `science`, `terminal`, `analytics`, `bug_report`, `build`, `cloud`, `database`, `extension`, `integration_instructions`, `launch`, `memory`, `query_stats`, `school`, `settings`, `speed`, `storage`, `tune`

Elenco completo: https://fonts.google.com/icons
