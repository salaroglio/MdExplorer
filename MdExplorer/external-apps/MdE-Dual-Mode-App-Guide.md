# Guida: App Electron Dual-Mode per MdExplorer

Guida completa per creare applicazioni Electron che funzionano sia come app desktop standalone sia embedded dentro MdExplorer (iframe).

---

## Il Problema

Quando un'app Electron gira **standalone**, il frontend comunica col main process via IPC (preload.js + contextBridge → `window.electronAPI`). Questo permette di usare file dialog nativi, notifiche, clipboard, shell, ecc.

Quando la **stessa app** gira **embedded in MdExplorer**, MdE la lancia con `--mde-embedded --port <PORT>` e carica `http://localhost:<PORT>/` in un `<iframe>`. In questo contesto:

- **`window.electronAPI` non esiste** — l'iframe non ha preload Electron
- **Le API Electron IPC sono inaccessibili** dal frontend
- **Il main process Electron è comunque attivo** — ha pieno accesso alle API Electron

Il backend HTTP dell'app deve quindi esporre via REST le stesse funzionalità che normalmente passano per IPC.

---

## Architettura Dual-Mode

```
┌─────────────────────────────────────────┐
│           Frontend (HTML/JS)            │
│                                         │
│    const bridge = createMdeBridge()     │
│    // auto-detect: Electron IPC o HTTP  │
│                                         │
│    bridge.showOpenDialog(options)        │
│    bridge.readFile(path)                │
│    bridge.openExternal(url)             │
│    bridge.showNotification(title, body) │
└──────────┬─────────────┬───────────────┘
           │             │
     ┌─────▼─────┐  ┌───▼────────────┐
     │ Electron  │  │  HTTP/fetch    │
     │ IPC mode  │  │  to localhost  │
     │ (preload) │  │  (iframe mode) │
     └─────┬─────┘  └───┬────────────┘
           │             │
           ▼             ▼
┌────────────────────────────────────────┐
│       Electron Main Process            │
│       (attivo in ENTRAMBE le modalità) │
│                                        │
│  Standalone: BrowserWindow + IPC       │
│  Embedded:   HTTP Server + REST API    │
│                                        │
│  Electron APIs sempre disponibili:     │
│  dialog, shell, clipboard, app, etc.   │
└────────────────────────────────────────┘
```

**Insight chiave**: il main process Electron è SEMPRE in esecuzione. In modalità embedded non crea la finestra ma ha comunque accesso a tutte le API Electron. L'HTTP server è solo un ponte di comunicazione alternativo.

---

## Struttura File Consigliata

```
my-app/
├── main.js              # Electron main process (IPC + HTTP server)
├── preload.js           # contextBridge per modalità standalone
├── package.json
├── src/
│   ├── index.html       # Frontend entry point
│   ├── bridge.js        # Layer di astrazione (auto-detect mode)
│   ├── app.js           # Logica applicativa (usa solo bridge.*)
│   └── styles.css
└── server/
    ├── routes.js         # REST endpoints che wrappano le API Electron
    └── cors.js           # Middleware CORS per iframe
```

---

## Step 1: Main Process — Dual Mode

```javascript
// main.js
const { app, BrowserWindow, dialog, shell, clipboard, Notification } = require('electron');
const path = require('path');

// Parse arguments
const args = process.argv.slice(2);
const isMdeEmbedded = args.includes('--mde-embedded');
const mdePort = isMdeEmbedded
  ? parseInt(args[args.indexOf('--port') + 1])
  : null;
const mdeHost = args.includes('--mde-host')
  ? args[args.indexOf('--mde-host') + 1]
  : null;
const workspace = args.includes('--workspace')
  ? args[args.indexOf('--workspace') + 1]
  : null;

let mainWindow = null;

app.whenReady().then(() => {
  if (isMdeEmbedded) {
    // EMBEDDED MODE: avvia solo l'HTTP server, nessuna finestra
    startHttpServer(mdePort);
  } else {
    // STANDALONE MODE: finestra + IPC handlers normali
    mainWindow = new BrowserWindow({
      width: 1200,
      height: 800,
      webPreferences: {
        preload: path.join(__dirname, 'preload.js'),
        contextIsolation: true,
        nodeIntegration: false
      }
    });
    mainWindow.loadFile('src/index.html');
    registerIpcHandlers();
  }
});

// ============================================================
// IPC Handlers (usati in standalone mode)
// ============================================================
function registerIpcHandlers() {
  const { ipcMain } = require('electron');

  ipcMain.handle('dialog:open', async (event, options) => {
    return dialog.showOpenDialog(mainWindow, options);
  });

  ipcMain.handle('dialog:save', async (event, options) => {
    return dialog.showSaveDialog(mainWindow, options);
  });

  ipcMain.handle('shell:open-external', async (event, url) => {
    return shell.openExternal(url);
  });

  ipcMain.handle('shell:open-path', async (event, filePath) => {
    return shell.openPath(filePath);
  });

  ipcMain.handle('clipboard:read', async () => {
    return clipboard.readText();
  });

  ipcMain.handle('clipboard:write', async (event, text) => {
    clipboard.writeText(text);
  });

  ipcMain.handle('notification:show', async (event, { title, body }) => {
    new Notification({ title, body }).show();
  });

  ipcMain.handle('fs:read-file', async (event, filePath) => {
    const fs = require('fs').promises;
    return fs.readFile(filePath, 'utf-8');
  });

  ipcMain.handle('fs:write-file', async (event, { filePath, content }) => {
    const fs = require('fs').promises;
    await fs.writeFile(filePath, content, 'utf-8');
  });

  ipcMain.handle('fs:list-dir', async (event, dirPath) => {
    const fs = require('fs').promises;
    const entries = await fs.readdir(dirPath, { withFileTypes: true });
    return entries.map(e => ({
      name: e.name,
      isDirectory: e.isDirectory()
    }));
  });

  ipcMain.handle('app:get-info', async () => {
    return {
      version: app.getVersion(),
      name: app.getName(),
      userDataPath: app.getPath('userData'),
      workspace: workspace
    };
  });
}

// ============================================================
// HTTP Server (usato in embedded/iframe mode)
// ============================================================
function startHttpServer(port) {
  const express = require('express');
  const server = express();

  // CORS: OBBLIGATORIO per iframe embedding
  server.use((req, res, next) => {
    res.header('Access-Control-Allow-Origin', '*');
    res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
    res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization');
    if (req.method === 'OPTIONS') return res.sendStatus(204);
    next();
  });

  server.use(express.json());

  // Serve static files (il frontend dell'app)
  server.use(express.static(path.join(__dirname, 'src')));

  // --- Bridge REST API ---

  // File dialog: apri
  server.post('/api/bridge/dialog/open', async (req, res) => {
    try {
      // dialog.showOpenDialog funziona anche senza BrowserWindow (passa null)
      const result = await dialog.showOpenDialog(null, req.body || {});
      res.json(result);
    } catch (err) {
      res.status(500).json({ error: err.message });
    }
  });

  // File dialog: salva
  server.post('/api/bridge/dialog/save', async (req, res) => {
    try {
      const result = await dialog.showSaveDialog(null, req.body || {});
      res.json(result);
    } catch (err) {
      res.status(500).json({ error: err.message });
    }
  });

  // Shell: apri URL nel browser di default
  server.post('/api/bridge/shell/open-external', async (req, res) => {
    try {
      await shell.openExternal(req.body.url);
      res.json({ success: true });
    } catch (err) {
      res.status(500).json({ error: err.message });
    }
  });

  // Shell: apri file/cartella con app di default
  server.post('/api/bridge/shell/open-path', async (req, res) => {
    try {
      const errorMsg = await shell.openPath(req.body.path);
      res.json({ success: !errorMsg, error: errorMsg || undefined });
    } catch (err) {
      res.status(500).json({ error: err.message });
    }
  });

  // Clipboard
  server.get('/api/bridge/clipboard', (req, res) => {
    res.json({ text: clipboard.readText() });
  });

  server.post('/api/bridge/clipboard', (req, res) => {
    clipboard.writeText(req.body.text);
    res.json({ success: true });
  });

  // Notifiche native
  server.post('/api/bridge/notification', (req, res) => {
    new Notification({ title: req.body.title, body: req.body.body }).show();
    res.json({ success: true });
  });

  // File system: leggi file
  server.get('/api/bridge/fs/read', async (req, res) => {
    try {
      const fs = require('fs').promises;
      const content = await fs.readFile(req.query.path, 'utf-8');
      res.json({ content });
    } catch (err) {
      res.status(500).json({ error: err.message });
    }
  });

  // File system: scrivi file
  server.post('/api/bridge/fs/write', async (req, res) => {
    try {
      const fs = require('fs').promises;
      await fs.writeFile(req.body.path, req.body.content, 'utf-8');
      res.json({ success: true });
    } catch (err) {
      res.status(500).json({ error: err.message });
    }
  });

  // File system: elenca directory
  server.get('/api/bridge/fs/list', async (req, res) => {
    try {
      const fs = require('fs').promises;
      const entries = await fs.readdir(req.query.path, { withFileTypes: true });
      res.json(entries.map(e => ({
        name: e.name,
        isDirectory: e.isDirectory()
      })));
    } catch (err) {
      res.status(500).json({ error: err.message });
    }
  });

  // App info
  server.get('/api/bridge/app-info', (req, res) => {
    res.json({
      version: app.getVersion(),
      name: app.getName(),
      userDataPath: app.getPath('userData'),
      workspace: workspace,
      mdeHost: mdeHost,
      embeddedMode: true
    });
  });

  // Health check (OBBLIGATORIO per il polling MdE)
  server.get('/', (req, res, next) => {
    // Se la richiesta accetta HTML, serve index.html (static middleware).
    // Altrimenti risponde 200 per il health check.
    if (req.accepts('html')) return next();
    res.sendStatus(200);
  });

  server.listen(port, '127.0.0.1', () => {
    console.log(`[MDE-EMBEDDED] Server avviato su http://127.0.0.1:${port}`);
  });
}
```

---

## Step 2: Preload (solo per standalone mode)

```javascript
// preload.js
const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electronAPI', {
  dialog: {
    open: (options) => ipcRenderer.invoke('dialog:open', options),
    save: (options) => ipcRenderer.invoke('dialog:save', options)
  },
  shell: {
    openExternal: (url) => ipcRenderer.invoke('shell:open-external', url),
    openPath: (path) => ipcRenderer.invoke('shell:open-path', path)
  },
  clipboard: {
    read: () => ipcRenderer.invoke('clipboard:read'),
    write: (text) => ipcRenderer.invoke('clipboard:write', text)
  },
  notification: {
    show: (title, body) => ipcRenderer.invoke('notification:show', { title, body })
  },
  fs: {
    readFile: (path) => ipcRenderer.invoke('fs:read-file', path),
    writeFile: (path, content) => ipcRenderer.invoke('fs:write-file', { filePath: path, content }),
    listDir: (path) => ipcRenderer.invoke('fs:list-dir', path)
  },
  app: {
    getInfo: () => ipcRenderer.invoke('app:get-info')
  }
});
```

---

## Step 3: Bridge — Layer di Astrazione (IL CUORE DELLA SOLUZIONE)

Il bridge auto-rileva la modalità e instrada le chiamate nel canale corretto.

```javascript
// src/bridge.js

/**
 * Crea il bridge per comunicare con il backend.
 * - Se `window.electronAPI` esiste → usa IPC Electron (standalone)
 * - Altrimenti → usa fetch HTTP verso il proprio server (iframe/embedded)
 *
 * Il codice applicativo usa SOLO bridge.* e non tocca mai
 * direttamente window.electronAPI o fetch.
 */
function createMdeBridge() {
  const isElectron = !!(window.electronAPI);

  // Base URL per HTTP mode: stessa origin (l'iframe carica da localhost:PORT)
  const baseUrl = window.location.origin;

  // ---------- Helper HTTP ----------
  async function httpGet(path) {
    const res = await fetch(`${baseUrl}${path}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}: ${await res.text()}`);
    return res.json();
  }

  async function httpPost(path, body) {
    const res = await fetch(`${baseUrl}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}: ${await res.text()}`);
    return res.json();
  }

  // ---------- Bridge API ----------
  return {
    /** Modalità corrente */
    mode: isElectron ? 'electron' : 'http',

    /** True se gira embedded in MdExplorer */
    get isEmbedded() {
      return !isElectron;
    },

    // --- Dialog ---
    async showOpenDialog(options = {}) {
      if (isElectron) {
        return window.electronAPI.dialog.open(options);
      }
      return httpPost('/api/bridge/dialog/open', options);
    },

    async showSaveDialog(options = {}) {
      if (isElectron) {
        return window.electronAPI.dialog.save(options);
      }
      return httpPost('/api/bridge/dialog/save', options);
    },

    // --- Shell ---
    async openExternal(url) {
      if (isElectron) {
        return window.electronAPI.shell.openExternal(url);
      }
      return httpPost('/api/bridge/shell/open-external', { url });
    },

    async openPath(filePath) {
      if (isElectron) {
        return window.electronAPI.shell.openPath(filePath);
      }
      return httpPost('/api/bridge/shell/open-path', { path: filePath });
    },

    // --- Clipboard ---
    async readClipboard() {
      if (isElectron) {
        return window.electronAPI.clipboard.read();
      }
      const result = await httpGet('/api/bridge/clipboard');
      return result.text;
    },

    async writeClipboard(text) {
      if (isElectron) {
        return window.electronAPI.clipboard.write(text);
      }
      return httpPost('/api/bridge/clipboard', { text });
    },

    // --- Notifications ---
    async showNotification(title, body) {
      if (isElectron) {
        return window.electronAPI.notification.show(title, body);
      }
      return httpPost('/api/bridge/notification', { title, body });
    },

    // --- File System ---
    async readFile(filePath) {
      if (isElectron) {
        return window.electronAPI.fs.readFile(filePath);
      }
      const result = await httpGet(`/api/bridge/fs/read?path=${encodeURIComponent(filePath)}`);
      return result.content;
    },

    async writeFile(filePath, content) {
      if (isElectron) {
        return window.electronAPI.fs.writeFile(filePath, content);
      }
      return httpPost('/api/bridge/fs/write', { path: filePath, content });
    },

    async listDir(dirPath) {
      if (isElectron) {
        return window.electronAPI.fs.listDir(dirPath);
      }
      return httpGet(`/api/bridge/fs/list?path=${encodeURIComponent(dirPath)}`);
    },

    // --- App Info ---
    async getAppInfo() {
      if (isElectron) {
        return window.electronAPI.app.getInfo();
      }
      return httpGet('/api/bridge/app-info');
    },

    // --- MdExplorer Host API ---
    /**
     * Chiama un endpoint dell'API di MdExplorer host.
     * Disponibile SOLO in modalità embedded (--mde-host viene passato all'avvio).
     * In standalone ritorna null.
     *
     * Esempio: bridge.callMdeApi('/api/mdfiles/GetShallowStructure')
     */
    async callMdeApi(endpoint, options = {}) {
      if (isElectron) return null;
      const info = await this.getAppInfo();
      if (!info.mdeHost) return null;
      const url = `${info.mdeHost}${endpoint}`;
      const res = await fetch(url, options);
      if (!res.ok) throw new Error(`MDE API ${res.status}: ${await res.text()}`);
      return res.json();
    }
  };
}

// Singleton globale
window.bridge = createMdeBridge();
```

---

## Step 4: Codice Applicativo — Usa Solo il Bridge

```javascript
// src/app.js
// Il codice applicativo NON usa mai window.electronAPI o fetch direttamente.
// Usa SOLO window.bridge.

async function init() {
  const info = await bridge.getAppInfo();
  document.getElementById('version').textContent = `v${info.version}`;
  document.getElementById('workspace').textContent = info.workspace || 'N/A';

  if (bridge.isEmbedded) {
    // Nascondi elementi non rilevanti in modalità embedded
    // (es. barra titolo custom, menu, controlli finestra)
    document.body.classList.add('mde-embedded');
  }
}

async function onOpenFile() {
  const result = await bridge.showOpenDialog({
    properties: ['openFile'],
    filters: [{ name: 'Markdown', extensions: ['md'] }]
  });
  if (!result.canceled && result.filePaths.length > 0) {
    const content = await bridge.readFile(result.filePaths[0]);
    document.getElementById('editor').value = content;
  }
}

async function onSaveFile() {
  const result = await bridge.showSaveDialog({
    filters: [{ name: 'Markdown', extensions: ['md'] }]
  });
  if (!result.canceled && result.filePath) {
    const content = document.getElementById('editor').value;
    await bridge.writeFile(result.filePath, content);
    bridge.showNotification('Salvato', `File salvato: ${result.filePath}`);
  }
}

// Inizializza
init();
```

---

## Step 5: HTML con Adattamento Automatico

```html
<!-- src/index.html -->
<!DOCTYPE html>
<html lang="it">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>My App</title>
  <link rel="stylesheet" href="styles.css">
</head>
<body>
  <header class="titlebar">
    <!-- Nascosta automaticamente in embedded mode via CSS -->
    <h1>My App <span id="version"></span></h1>
  </header>

  <main>
    <p>Workspace: <code id="workspace"></code></p>
    <button onclick="onOpenFile()">Apri File</button>
    <button onclick="onSaveFile()">Salva File</button>
    <textarea id="editor" rows="20"></textarea>
  </main>

  <!-- ORDINE IMPORTANTE: bridge.js PRIMA di app.js -->
  <script src="bridge.js"></script>
  <script src="app.js"></script>
</body>
</html>
```

```css
/* src/styles.css */

/* In embedded mode: nascondi barra titolo e usa tutto lo spazio */
body.mde-embedded .titlebar {
  display: none;
}

body.mde-embedded {
  margin: 0;
  padding: 0;
}

/* Rimuovi bordi e ombre quando embedded */
body.mde-embedded main {
  border: none;
  box-shadow: none;
  border-radius: 0;
}
```

---

## Step 6: package.json

```json
{
  "name": "my-mde-app",
  "version": "1.0.0",
  "main": "main.js",
  "dependencies": {
    "express": "^4.18.0"
  },
  "devDependencies": {
    "electron": "^35.0.0"
  },
  "scripts": {
    "start": "electron .",
    "start:embedded": "electron . --mde-embedded --port 9100 --workspace ."
  }
}
```

> **Nota**: `express` è una dipendenza runtime necessaria per la modalità embedded. Non serve in standalone ma è sempre installata per semplicità.

---

## Riepilogo Endpoints Bridge

| Endpoint REST | Metodo | Equivalente Electron | Descrizione |
|---------------|--------|---------------------|-------------|
| `/api/bridge/dialog/open` | POST | `dialog.showOpenDialog()` | Dialog apri file/cartella |
| `/api/bridge/dialog/save` | POST | `dialog.showSaveDialog()` | Dialog salva file |
| `/api/bridge/shell/open-external` | POST | `shell.openExternal()` | Apri URL nel browser |
| `/api/bridge/shell/open-path` | POST | `shell.openPath()` | Apri file con app di default |
| `/api/bridge/clipboard` | GET | `clipboard.readText()` | Leggi clipboard |
| `/api/bridge/clipboard` | POST | `clipboard.writeText()` | Scrivi clipboard |
| `/api/bridge/notification` | POST | `new Notification()` | Mostra notifica nativa |
| `/api/bridge/fs/read?path=` | GET | `fs.readFile()` | Leggi file |
| `/api/bridge/fs/write` | POST | `fs.writeFile()` | Scrivi file |
| `/api/bridge/fs/list?path=` | GET | `fs.readdir()` | Lista directory |
| `/api/bridge/app-info` | GET | `app.getVersion()` etc. | Info sull'app e contesto |

---

## CORS — Configurazione Obbligatoria

Quando l'app gira in un iframe di MdExplorer, il browser applica le regole CORS.
L'HTTP server dell'app DEVE includere gli header CORS su TUTTE le risposte:

```javascript
server.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization');
  if (req.method === 'OPTIONS') return res.sendStatus(204);
  next();
});
```

> **Sicurezza**: `*` è accettabile perché sia MdE che l'app girano su `127.0.0.1`. In produzione si potrebbe restringere all'origin di MdE (`http://localhost:48123`).

---

## Integrazione con MdExplorer Host

L'app embedded riceve `--mde-host <URL>` all'avvio. Questo è l'URL del backend MdExplorer e permette all'app di chiamare le API di MdE.

### Endpoint MdE Utili

| Endpoint MdE | Metodo | Descrizione |
|--------------|--------|-------------|
| `/api/MdProjects/GetProjects` | GET | Lista progetti |
| `/api/mdfiles/GetShallowStructure` | GET | Albero file del progetto |
| `/api/mdfiles/getFile?filePath=...` | GET | Contenuto di un file markdown |
| `/api/MdExternalApps` | GET | Lista app esterne configurate |

### Esempio: Leggere un file dal progetto MdE

```javascript
// Dal codice applicativo
const files = await bridge.callMdeApi('/api/mdfiles/GetShallowStructure');
console.log('File nel progetto:', files);
```

### CORS per le chiamate a MdE Host

Le chiamate dall'iframe (`localhost:PORT`) verso MdE (`localhost:48123`) sono **cross-origin**. Due soluzioni:

**Soluzione A (consigliata): Proxy nel backend dell'app**

```javascript
// Nel server Express dell'app (main.js, dentro startHttpServer)
server.all('/api/mde-proxy/*', async (req, res) => {
  const targetPath = req.params[0];
  const targetUrl = `${mdeHost}/api/${targetPath}`;
  try {
    const response = await fetch(targetUrl, {
      method: req.method,
      headers: { 'Content-Type': 'application/json' },
      body: ['POST', 'PUT'].includes(req.method) ? JSON.stringify(req.body) : undefined
    });
    const data = await response.json();
    res.json(data);
  } catch (err) {
    res.status(502).json({ error: err.message });
  }
});
```

Poi nel bridge:
```javascript
// Invece di chiamare MdE direttamente
const files = await fetch('/api/mde-proxy/mdfiles/GetShallowStructure').then(r => r.json());
```

**Soluzione B: Chiamata diretta (richiede CORS su MdE)**

MdExplorer ha già CORS abilitato nel suo backend ASP.NET per le richieste da localhost.

---

## Checklist per App Dual-Mode

### Obbligatorio

- [ ] Parse `--mde-embedded`, `--port`, `--mde-host`, `--workspace` dagli argomenti
- [ ] Se `--mde-embedded`: NON creare BrowserWindow, avviare HTTP server sulla porta indicata
- [ ] HTTP server risponde `200` a `GET /` (health check per polling MdE)
- [ ] CORS headers su tutte le risposte HTTP
- [ ] REST endpoints `/api/bridge/*` per ogni operazione Electron usata dal frontend
- [ ] `bridge.js` nel frontend che auto-rileva la modalità
- [ ] Il codice applicativo usa SOLO `bridge.*`, mai `window.electronAPI` direttamente
- [ ] `express` nelle dipendenze di produzione

### Consigliato

- [ ] CSS class `mde-embedded` sul body per adattare il layout (nascondere titlebar, ecc.)
- [ ] Proxy `/api/mde-proxy/*` per chiamate all'host MdE senza problemi CORS
- [ ] Script npm `start:embedded` per test locale della modalità embedded
- [ ] Gestione graceful shutdown (`process.on('SIGTERM', ...)`)

### Da NON Fare

- **NON** usare `window.electronAPI` nel codice applicativo — sempre il bridge
- **NON** creare BrowserWindow in modalità embedded
- **NON** dimenticare CORS — l'iframe non funzionerà senza
- **NON** usare porte hardcoded — usare sempre `--port`
- **NON** ascoltare su `0.0.0.0` — usare `127.0.0.1` per sicurezza
- **NON** esporre endpoint FS senza validazione del path (limitare al workspace)

---

## Sicurezza degli Endpoint FS

Gli endpoint `/api/bridge/fs/*` danno accesso al filesystem. Limitare al workspace:

```javascript
const path = require('path');

function validatePath(requestedPath) {
  const resolved = path.resolve(requestedPath);
  if (!workspace) return resolved; // nessuna restrizione se no workspace
  const workspaceResolved = path.resolve(workspace);
  if (!resolved.startsWith(workspaceResolved)) {
    throw new Error('Path outside workspace');
  }
  return resolved;
}

// Esempio d'uso
server.get('/api/bridge/fs/read', async (req, res) => {
  try {
    const safePath = validatePath(req.query.path);
    const content = await require('fs').promises.readFile(safePath, 'utf-8');
    res.json({ content });
  } catch (err) {
    res.status(err.message === 'Path outside workspace' ? 403 : 500)
       .json({ error: err.message });
  }
});
```

---

## Testing Locale della Modalità Embedded

Per testare senza MdExplorer:

```bash
# Avvia in modalità embedded sulla porta 9100
npm run start:embedded

# Oppure manualmente
npx electron . --mde-embedded --port 9100 --mde-host http://localhost:48123 --workspace "C:\mio-progetto"
```

Poi apri `http://localhost:9100` in un browser normale. Il bridge rileverà che `window.electronAPI` non esiste e userà HTTP automaticamente.

---

## Migrazione di un'App Electron Esistente

Per convertire un'app Electron esistente al dual-mode:

### 1. Identifica tutti gli usi di `window.electronAPI` / IPC

Cerca nel codice frontend:
```
window.electronAPI
ipcRenderer.invoke
ipcRenderer.send
ipcRenderer.on
```

### 2. Crea gli endpoint REST corrispondenti

Per ogni `ipcMain.handle('channel', handler)` nel main process, crea un endpoint REST equivalente nel server Express.

### 3. Crea il bridge

Implementa `bridge.js` con un metodo per ogni operazione, che sceglie IPC o HTTP.

### 4. Sostituisci tutte le chiamate dirette

Sostituisci ogni `window.electronAPI.xxx()` con `bridge.xxx()` nel codice applicativo.

### 5. Aggiungi la logica dual-mode al main process

Modifica `main.js` per parsare `--mde-embedded` e avviare HTTP server invece di BrowserWindow.

### 6. Testa entrambe le modalità

```bash
npm start              # Standalone: deve funzionare come prima
npm run start:embedded # Embedded: apri in browser, tutto deve funzionare via HTTP
```

---

## Esempio Completo Minimale

Un'app dual-mode che legge e mostra file markdown dal workspace:

### `.mdeapps.json` (nella root del progetto MdE)

```json
{
  "version": "2",
  "apps": [
    {
      "id": "md-viewer",
      "name": "MD Viewer",
      "description": "Visualizzatore markdown semplice",
      "icon": "description",
      "executable": ".mde/apps/md-viewer/md-viewer.exe",
      "singleton": true
    }
  ],
  "tree": [
    { "type": "app", "appId": "md-viewer" }
  ]
}
```

### Flusso operativo

1. L'utente clicca "MD Viewer" nel tree di MdExplorer
2. MdE lancia: `md-viewer.exe --mde-embedded --port 52341 --mde-host http://localhost:48123 --workspace C:\docs`
3. L'app avvia Express su porta 52341, MdE fa polling finché risponde
4. MdE carica `http://localhost:52341/` nell'iframe
5. L'app mostra la lista file e l'utente può navigarli
6. Quando l'utente clicca "Apri file", il bridge chiama `POST /api/bridge/dialog/open` → il main process Electron mostra il dialog nativo → restituisce il path → l'app lo legge via `GET /api/bridge/fs/read`

---

## FAQ

**D: Perché non usare solo `<input type="file">` per i dialog?**
R: `<input type="file">` funziona per aprire file ma non supporta: apertura cartelle, salvataggio (solo upload), filtri personalizzati avanzati. Il dialog nativo Electron è più completo e coerente con l'esperienza desktop.

**D: Le notifiche native funzionano senza finestra?**
R: Sì. `new Notification({ title, body }).show()` in Electron funziona anche senza BrowserWindow. Il main process è comunque registrato come app nel sistema operativo.

**D: Posso usare Socket.IO/WebSocket invece di REST?**
R: Sì, per comunicazione bidirezionale (es. aggiornamenti in tempo reale dal backend). Aggiungi Socket.IO al server Express e un equivalente nel bridge. REST è sufficiente per operazioni request/response.

**D: L'app deve per forza essere Electron?**
R: No. Per un'app **pure Node.js** (senza Electron), gli endpoint REST usano le API Node.js native (`fs`, `child_process`, ecc.) ma i **dialog nativi non sono disponibili**. Alternative: `<input type="file">` nel browser, o comunicare con MdE host per usare i suoi dialog.

**D: Come gestisco temi/dark mode quando sono embedded?**
R: Leggi il tema di MdE via `bridge.callMdeApi()` o accetta un parametro `--theme dark|light` negli args. Adatta i CSS di conseguenza.
