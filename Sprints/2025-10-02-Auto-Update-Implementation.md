---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 03/10/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Auto-Update Implementation

## Overview

Implementazione sistema di aggiornamento automatico per l'applicazione Electron utilizzando `electron-updater`. La funzionalità permetterà agli utenti di ricevere notifiche di nuove versioni disponibili e scaricare/installare aggiornamenti direttamente dalla tray bar.

### Obiettivi
* Check automatico versione all'avvio e periodico
* Download sicuro con verifica integrità
* Notifica utente tramite tray icon
* Installazione guidata con opzioni flessibili
* Sicurezza garantita tramite firma digitale e checksum

## Strategia di Implementazione Progressiva

### Fase 1: Codice Pronto (Implementazione Immediata)
* Implementare electron-updater nel progetto
* Configurare endpoint futuro: `https://www.mdexplorer.net/updates/latest.yml`
* Gestione errori silenziosa quando endpoint non disponibile
* Menu tray "Verifica Aggiornamenti" con messaggio "Servizio non disponibile"
* App continua a funzionare normalmente senza notifiche errore

### Fase 2: Attivazione Automatica (Quando Sito Ready)
* Deploy endpoint server con file `latest.yml`
* Upload installer firmati
* Auto-updater inizia a funzionare automaticamente
* Nessuna modifica codice necessaria

**Vantaggio**: distribuire app già predisposta, attivazione trasparente quando infrastruttura pronta

## Prerequisiti

### Certificato Code-Signing
* **Necessario per**: evitare warning Windows, abilitare auto-update sicuro
* **Costo**: €150-300/anno (Sectigo, DigiCert, GlobalSign)
* **Quando**: obbligatorio prima della Fase 2 (release pubblica)
* **Fase 1**: può funzionare senza, con warning accettabili per beta/testing

### Endpoint Server
* **URL**: `https://www.mdexplorer.net/updates/`
* **File richiesto**: `latest.yml` (metadata versione e download)
* **Hosting**: VPS esistente o static hosting
* **HTTPS**: obbligatorio per sicurezza

## Architettura Tecnica

### Flusso Auto-Update

```
[Electron App]
    ↓ (check all'avvio + ogni 4h)
[https://mdexplorer.net/updates/latest.yml]
    ↓ (parsing versione)
[Confronto versione locale vs remota]
    ↓ (se nuova disponibile)
[Notifica tray icon]
    ↓ (user click)
[Download installer in background]
    ↓ (verifica checksum SHA512)
[Verifica firma digitale]
    ↓ (se ok)
[Opzioni: installa ora / al prossimo avvio / salta]
```

### Componenti Sicurezza

1. **HTTPS obbligatorio**: comunicazioni cifrate
2. **Checksum SHA512**: integrità file verificata automaticamente
3. **Firma digitale**: validazione publisher con certificato code-signing
4. **Anti-downgrade**: controllo versione semantica impedisce rollback
5. **Sandbox download**: file temporanei in directory isolata

## Step di Implementazione

### 1. Setup Dipendenze

**File**: `ElectronMdExplorer/package.json`

Aggiungere:
```json
{
  "dependencies": {
    "electron-updater": "^6.x.x"
  }
}
```

### 2. Configurazione electron-builder

**File**: `ElectronMdExplorer/electron-builder.yml` o sezione in `package.json`

```yaml
publish:
  - provider: generic
    url: https://www.mdexplorer.net/updates/
    channel: latest
```

Configurazione firma Windows:
```yaml
win:
  certificateFile: path/to/certificate.pfx
  certificatePassword: ${env.CERT_PASSWORD}
  sign: true
  publisherName: "Your Company Name"
```

### 3. Logica Check Versione (Main Process)

**File**: `ElectronMdExplorer/main.js` (o file principale Electron)

Implementare:
* Import `electron-updater`
* Inizializzazione `autoUpdater`
* Event handlers: `update-available`, `update-downloaded`, `error`
* Configurazione intervallo check (es: 4 ore)
* Gestione errori silenziosa per Fase 1

Funzionalità:
* Check all'avvio app (dopo 30 sec di delay)
* Check periodico automatico
* Log interni per debugging
* Comunicazione IPC con renderer per UI

### 4. UI Tray Menu

**File**: Modulo gestione tray (da identificare nel progetto)

Aggiungere voci menu:
* "Verifica Aggiornamenti..." → trigger check manuale
* Badge/icona notifica quando update disponibile
* Dialogo download progress
* Opzioni installazione:
  - Installa ora (quit & install)
  - Installa al prossimo avvio
  - Salta questa versione

Messaggi UI:
* Fase 1: "Servizio aggiornamenti non disponibile" se errore network
* Fase 2: notifiche normali update disponibile

### 5. Testing Locale

**Mock Server per sviluppo**:
* Creare file `latest.yml` locale di test
* Server HTTP locale (es: Python `http.server`)
* Simulare versioni diverse per testare flusso completo
* Testare gestione errori (timeout, 404, etc.)

Scenari da testare:
* Nessun update disponibile
* Update disponibile (download + install)
* Errore network (endpoint non raggiungibile)
* File corrotto (checksum mismatch)
* Firma invalida

### 6. Setup Server www.mdexplorer.net

**Endpoint**: `/updates/`

**File `latest.yml` esempio**:
```yaml
version: 1.2.3
releaseDate: 2025-10-02
files:
  - url: mdexplorer-setup-1.2.3.exe
    sha512: <checksum-sha512>
    size: 45678901
path: mdexplorer-setup-1.2.3.exe
sha512: <checksum-sha512>
releaseDate: 2025-10-02T10:00:00.000Z
```

**Struttura directory**:
```
/updates/
  ├── latest.yml
  ├── mdexplorer-setup-1.2.3.exe
  ├── mdexplorer-setup-1.2.3.dmg (Mac)
  └── mdexplorer-setup-1.2.3.AppImage (Linux)
```

**Automazione**:
* Script build che genera `latest.yml` automaticamente
* Upload automatico a server dopo build
* Backup versioni precedenti

### 7. Processo Firma Installer

**Windows**:
```bash
# Con electron-builder (automatico se configurato)
npm run build

# Manuale con signtool
signtool sign /f certificate.pfx /p password /tr http://timestamp.digicert.com installer.exe
```

**macOS**:
* Apple Developer ID certificate
* Notarizzazione con `notarytool`
* Stapling per uso offline

**Linux**:
* Firma GPG per AppImage (opzionale)
* Checksum sempre necessario

### 8. Deploy e Test Completo

**Checklist pre-release**:
- [ ] Certificato code-signing configurato e valido
- [ ] Endpoint server accessibile (HTTPS)
- [ ] File `latest.yml` corretto e validato
- [ ] Installer firmato e caricato su server
- [ ] Checksum SHA512 verificato manualmente
- [ ] Test download da app con versione precedente
- [ ] Verifica firma digitale funzionante
- [ ] Test installazione completa
- [ ] Rollback plan in caso problemi

**Monitoring post-deploy**:
* Log server per download (analytics)
* Error tracking client-side
* Feedback utenti su processo update

## Gestione Errori

### Errori Network (Fase 1)
* Timeout request (30 sec default)
* Nessuna notifica utente se fail silenzioso
* Log interno: `[AutoUpdate] Check failed: ECONNREFUSED`
* Retry al prossimo intervallo automatico

### Errori Download
* Checksum mismatch → cancella file, notifica errore
* Firma invalida → blocco installazione, alert sicurezza
* Spazio disco insufficiente → notifica utente

### Fallback Strategy
* Se update fallisce 3 volte consecutive → disabilita check automatico
* Menu manuale sempre disponibile
* Possibilità reset tramite setting app

## Configurazioni Specifiche Progetto

### Variabili Ambiente

**Development**:
```env
AUTO_UPDATE_ENABLED=false
UPDATE_SERVER_URL=http://localhost:3000/updates/
```

**Production**:
```env
AUTO_UPDATE_ENABLED=true
UPDATE_SERVER_URL=https://www.mdexplorer.net/updates/
```

### Feature Flag

Possibilità disabilitare feature via config:
```json
{
  "features": {
    "autoUpdate": {
      "enabled": false,
      "checkInterval": 14400000
    }
  }
}
```

## Checklist Sicurezza Finale

Prima del rilascio pubblico verificare:

- [ ] **HTTPS**: tutte le comunicazioni cifrate
- [ ] **Firma digitale**: installer firmato con certificato valido
- [ ] **Checksum**: SHA512 verificato automaticamente
- [ ] **Anti-downgrade**: controllo versione semantica attivo
- [ ] **Sandbox**: download in directory temporanea isolata
- [ ] **Validazione input**: parsing `latest.yml` con schema validation
- [ ] **Timeout**: limiti temporali per network requests
- [ ] **Permissions**: app richiede solo permessi necessari
- [ ] **Logging**: nessun dato sensibile nei log
- [ ] **Rollback**: possibilità tornare a versione precedente

## Risorse e Riferimenti

### Documentazione
* [electron-updater docs](https://www.electron.build/auto-update)
* [Code signing guide](https://www.electron.build/code-signing)
* [Certificati code-signing](https://docs.digicert.com/en/software-trust-manager/ci-cd-integrations/plugins/electron-builder-integration.html)

### Alternative Valutate
* **Squirrel.Windows**: più controllo, setup complesso
* **AppCenter**: servizio Microsoft, costi mensili
* **Sistema custom**: sconsigliato per complessità sicurezza

### Motivazione Scelta electron-updater
* Libreria ufficiale Electron ecosystem
* Sicurezza integrata (firma + checksum)
* Supporto multi-piattaforma
* Aggiornamenti differenziali (delta)
* Community attiva, manutenzione continua

## Timeline Stimata

### Fase 1 (Codice Pronto)
* Setup dipendenze: 1h
* Implementazione logica: 3-4h
* UI tray integration: 2h
* Testing locale: 2h
* **Totale**: ~1 giorno

### Fase 2 (Attivazione)
* Acquisto certificato: 1-2 giorni (processo verifica)
* Setup server endpoint: 2h
* Processo firma e deploy: 1h
* Testing completo: 2h
* **Totale**: 2-3 giorni (attesa certificato inclusa)

## Note Implementazione

### Compatibilità
* Windows: NSIS installer con firma Authenticode
* macOS: DMG con Apple Developer ID + notarizzazione
* Linux: AppImage con checksum (firma opzionale)

### Banda Server
* Installer size: ~50-100MB tipico
* Con 100 utenti: ~5-10GB/mese
* Delta updates riducono consumo ~80%

### Manutenzione
* Ogni release: aggiornare `latest.yml`
* Automatizzare con CI/CD (GitHub Actions, GitLab CI)
* Script post-build per upload automatico

## Follow-up Futuro

Possibili miglioramenti:
* **Canali update**: stable, beta, nightly
* **Partial updates**: solo file modificati (delta)
* **Analytics**: tracking adoption rate nuove versioni
* **Rollback automatico**: se crash rate aumenta post-update
* **A/B testing**: rollout graduale (10% → 50% → 100%)
