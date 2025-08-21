---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 21/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# 2025-07-31 Implementazione TitleBar Electron

## Obiettivo

Rimuovere la barra del titolo nativa di Electron e implementare una titlebar custom che funzioni sia in ambiente web che desktop, mantenendo un'esperienza utente consistente.

## Soluzione Architetturale

### 1. Componente TitleBarComponent

* Nuovo componente Angular a livello root (`app.component.html`)
* Posizionato sopra `<router-outlet>`
* Altezza fissa (\~30px) per tutta la larghezza dell'applicazione
* Sempre visibile in entrambi gli ambienti (web e Electron)

### 2. Struttura Layout

```
AppComponent
├── TitleBarComponent (barra superiore fissa)
│   ├── Logo + Titolo (sinistra)
│   ├── Area Drag (centro)
│   └── Window Controls (destra - solo Electron)
└── <div class="app-content"> (margin-top: 30px)
    └── <router-outlet>
```

### 3. Comportamento Differenziato

#### Ambiente Web

* Mostra: Logo + Titolo "MdExplorer"
* Non mostra: Controlli finestra
* Funzione: Header branding dell'applicazione

#### Ambiente Electron

* Mostra: Logo + Titolo + Controlli finestra (minimize/maximize/close)
* Area centrale draggable per spostare la finestra
* Finestra configurata come `frameless`

### 4. Rilevamento Ambiente

```TypeScript
isElectron = !!(window && window.electronAPI);
```

### 5. Comunicazione IPC (solo Electron)

#### Flusso:

1. **Angular**: Click su controlli → `window.electronAPI.minimizeWindow()`
2. **Preload.js**: Espone API sicura tramite `contextBridge`
3. **Electron main**: Gestisce comandi IPC → `mainWindow.minimize()`

#### Canali IPC:

* `minimize-window`
* `maximize-window`
* `close-window`

### 6. Modifiche Richieste

#### index.js (Electron)

* Aggiungere `frame: false` nella creazione BrowserWindow
* Implementare handler IPC per controlli finestra

#### preload.js

* Esporre API per controlli finestra via `contextBridge`

#### Angular

* Creare TitleBarComponent
* Modificare app.component.html per includere titlebar
* Aggiungere margine superiore al contenuto principale

### 7. Vantaggi

* **UI Consistente**: Stesso layout in web e desktop
* **Single Codebase**: Nessuna duplicazione di codice
* **Sicurezza**: Comunicazione IPC isolata tramite preload
* **UX Professionale**: Look moderno e unificato
* **Estensibilità**: Spazio per future features nella titlebar

### 8. CSS Chiave

```CSS
.title-bar {
  position: fixed;
  top: 0;
  width: 100%;
  height: 30px;
  z-index: 1000;
}

.drag-region {
  -webkit-app-region: drag;
  flex: 1;
}

.window-controls button {
  -webkit-app-region: no-drag;
}
```

## Note Implementative

* La titlebar deve integrarsi visivamente con il tema Material Design esistente
* I controlli finestra useranno icone Material Icons
* L'area drag non deve interferire con elementi cliccabili
* Gestire correttamente lo stato maximized/restored del pulsante maximize

<br />
