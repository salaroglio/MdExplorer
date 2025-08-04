# Migrazione MdExplorer a Linux - Overview

## Obiettivo
Analizzare e documentare tutte le modifiche necessarie per rendere MdExplorer completamente compatibile con Linux, mantenendo la compatibilità cross-platform.

## Struttura Analisi

### 1. Analisi Backend (.NET Core)
- **01-dotnet-compatibility.md** - Compatibilità framework e librerie
- **02-filesystem-paths.md** - Gestione path e filesystem
- **03-external-processes.md** - Processi esterni e shell commands
- **04-configuration.md** - Configurazioni e appsettings

### 2. Analisi Frontend
- **05-angular-react.md** - Compatibilità Angular e React
- **06-electron-linux.md** - Electron su Linux

### 3. Database e Storage
- **07-database-sqlite.md** - SQLite e percorsi database
- **08-file-storage.md** - Storage file e permessi

### 4. Integrazioni Esterne
- **09-plantuml-integration.md** - PlantUML su Linux
- **10-git-integration.md** - LibGit2Sharp su Linux
- **11-pandoc-export.md** - Pandoc e export PDF/Word

### 5. Build e Deploy
- **12-build-system.md** - Sistema di build per Linux
- **13-packaging.md** - Packaging e distribuzione

## Priorità Analisi

### Alta Priorità (Blockers)
1. Path del filesystem Windows-specific
2. Processi esterni e comandi shell
3. Configurazioni hardcoded per Windows
4. Librerie .NET non compatibili

### Media Priorità (Funzionalità)
1. Export PDF/Word con Pandoc
2. PlantUML server e JAR
3. Electron packaging per Linux
4. Git integration

### Bassa Priorità (Nice to have)
1. Ottimizzazioni specifiche Linux
2. Integrazione con package manager Linux
3. Systemd service configuration

## Progetti Analizzati

### Backend Core (.NET Core 3.1)
- ✅ MdExplorer.Service - Entry point principale
- ✅ MdExplorer.Features - Business logic
- ✅ MDExplorer.DataAccess - Data access layer
- ✅ Ad.Tools.Dal/Evo - Framework DAL

### Frontend
- ✅ client2 (Angular 11)
- ✅ MdEditor.React (React 19)
- ✅ ElectronMdExplorer

### Utility
- ✅ MdImageNumbering (.NET 6.0)
- ⚠️ WebViewControl (.NET Framework 4.7.2) - NON COMPATIBILE

## Problemi Identificati (Preview)

### Critici
1. WebViewControl usa .NET Framework 4.7.2 (Windows-only)
2. WiX installers sono Windows-specific
3. Possibili path hardcoded con backslash

### Da Verificare
1. LibGit2Sharp compatibility su Linux
2. PlantUML JAR execution
3. Pandoc path e templates
4. SignalR su Linux
5. SQLite path dei database

## Next Steps
1. Analisi dettagliata di ogni componente
2. Creazione checklist per ogni problema trovato
3. Proposta soluzioni per ogni incompatibilità
4. Piano di migrazione incrementale