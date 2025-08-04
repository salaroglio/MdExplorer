# MdExplorer Linux Migration - Summary & Roadmap

## 📊 Executive Summary

Il progetto MdExplorer richiede modifiche significative per funzionare su Linux. L'analisi ha identificato **120+ problemi** distribuiti su tutti i componenti del sistema. Tuttavia, l'architettura base è solida e molti componenti sono già parzialmente predisposti per il cross-platform.

### Effort Totale Stimato: **40-55 giorni/persona**

## 🎯 Problemi Principali per Componente

### Backend .NET (11-16 giorni)
- **7 progetti** incompatibili con .NET Framework
- **42+ path** con drive letters e backslash hardcoded  
- **5+ processi** che usano cmd.exe
- **LocalAppData** environment variable Windows-specific

### Frontend (2-3 giorni)
- **Angular**: 35+ occorrenze di backslash hardcoded
- **React**: Già compatibile ✅
- **Karma**: Configurazione solo per Chrome

### Electron (5-8 giorni)
- **Build config** solo Windows
- **Executable paths** hardcoded con .exe
- **No packaging** per Linux

### Configuration (2-3 giorni)
- **RuntimeIdentifier** hardcoded win10-x64
- **UseWindowsForms** enabled
- **Path Windows** in migrations

## 🚦 Roadmap di Migrazione

### Phase 1: Foundation (Settimana 1-2)
**Obiettivo**: Rimuovere blockers critici

#### Sprint 1.1: Project Configuration
- [ ] Rimuovere RuntimeIdentifier hardcoded
- [ ] Disabilitare UseWindowsForms per Linux
- [ ] Configurare build condizionali per OS
- [ ] **Deliverable**: Build .NET funzionante su Linux

#### Sprint 1.2: Path Management
- [ ] Implementare DataDirectoryManager
- [ ] Sostituire LocalAppData con XDG paths
- [ ] Rimuovere drive letters dalle migrations
- [ ] **Deliverable**: Database creato correttamente su Linux

### Phase 2: Core Compatibility (Settimana 3-4)
**Obiettivo**: Funzionalità base operative

#### Sprint 2.1: Process & Shell
- [ ] Implementare CrossPlatformProcess helper
- [ ] Sostituire cmd.exe con shell wrapper
- [ ] Sostituire explorer.exe con xdg-open
- [ ] **Deliverable**: Processi esterni funzionanti

#### Sprint 2.2: Path Operations
- [ ] Sostituire tutti i backslash hardcoded
- [ ] Implementare path utilities
- [ ] Fix manipolazioni manuali dei path
- [ ] **Deliverable**: Operazioni file cross-platform

### Phase 3: Frontend Integration (Settimana 5)
**Obiettivo**: UI completamente funzionale

#### Sprint 3.1: Angular Fixes
- [ ] Implementare PathService
- [ ] Refactoring componenti con path Windows
- [ ] Configurare Karma per headless testing
- [ ] **Deliverable**: Angular app funzionante su Linux

#### Sprint 3.2: SignalR & Testing
- [ ] Normalizzare path in SignalR messages
- [ ] Configurare test cross-platform
- [ ] **Deliverable**: Real-time features operative

### Phase 4: Electron Desktop (Settimana 6-7)
**Obiettivo**: Applicazione desktop completa

#### Sprint 4.1: Electron Core
- [ ] Gestione executable paths cross-platform
- [ ] Configurazione build multi-platform
- [ ] Desktop integration files
- [ ] **Deliverable**: Electron app avviabile

#### Sprint 4.2: Packaging
- [ ] Configurare AppImage build
- [ ] Configurare deb/rpm packaging
- [ ] Test auto-update mechanism
- [ ] **Deliverable**: Installer Linux funzionanti

### Phase 5: Polish & Testing (Settimana 8)
**Obiettivo**: Production-ready

#### Sprint 5.1: Integration Testing
- [ ] Test su Ubuntu 20.04/22.04
- [ ] Test su Fedora/RHEL
- [ ] Test su Arch Linux
- [ ] **Deliverable**: Compatibility matrix

#### Sprint 5.2: Documentation & CI/CD
- [ ] Documentare setup Linux
- [ ] Configurare GitHub Actions per Linux
- [ ] Script installazione dipendenze
- [ ] **Deliverable**: Release Linux v1.0

## 📈 Metriche di Successo

### Technical KPIs
- ✅ **Zero Windows-specific code** in runtime path
- ✅ **100% unit tests** passing su Linux
- ✅ **< 60s build time** per piattaforma
- ✅ **< 5s startup time** su hardware medio
- ✅ **< 500MB RAM** usage in idle

### Functional KPIs
- ✅ Tutte le features core funzionanti
- ✅ Export PDF/Word operativo
- ✅ Git integration completa
- ✅ PlantUML rendering funzionante
- ✅ SignalR real-time updates

### Distribution KPIs
- ✅ AppImage < 100MB
- ✅ Deb/RPM packages installabili
- ✅ Auto-update funzionante
- ✅ Desktop integration completa

## 🔧 Tooling Requirements

### Development Environment
```bash
# Linux Development Setup
sudo apt-get install -y \
  dotnet-sdk-3.1 \
  nodejs \
  npm \
  git \
  build-essential \
  chromium-browser
```

### Runtime Dependencies
```bash
# Linux Runtime Dependencies
sudo apt-get install -y \
  graphviz \
  default-jre \
  pandoc \
  texlive-xetex \
  libgdiplus \
  libsecret-1-0
```

## 🚀 Quick Start Guide

### Per iniziare la migrazione:

1. **Setup Branch**
   ```bash
   git checkout -b feature/linux-support
   ```

2. **Priorità Immediate**
   ```bash
   # Fix 1: RuntimeIdentifier
   sed -i 's/<RuntimeIdentifier>win10-x64/<RuntimeIdentifier Condition="\$(OS)=='"'"'Windows_NT'"'"'">win10-x64/g' *.csproj
   
   # Fix 2: Remove UseWindowsForms
   sed -i '/<UseWindowsForms>/d' MdExplorer.Service.csproj
   ```

3. **Test Build Base**
   ```bash
   dotnet build --runtime linux-x64
   ```

## 📊 Risk Assessment

### High Risk Areas
1. **SQLite compatibility** - Potenziali problemi con System.Data.SQLite
2. **LibGit2Sharp** - Dipendenze native potrebbero fallire
3. **PlantUML/GraphViz** - Path e configurazioni da verificare
4. **Performance** - Possibile degrado su Linux

### Mitigation Strategies
1. Migrare a Microsoft.Data.Sqlite
2. Test estensivi LibGit2Sharp su varie distro
3. Docker container per PlantUML server
4. Profiling e ottimizzazione post-migrazione

## 📝 Definition of Done

### Per considerare la migrazione completa:

- [ ] **Code**: Zero riferimenti Windows-specific in runtime
- [ ] **Build**: CI/CD pipeline per Linux attiva
- [ ] **Test**: > 90% code coverage mantenuta
- [ ] **Package**: AppImage, deb, rpm disponibili
- [ ] **Docs**: Guida installazione Linux completa
- [ ] **Support**: Testato su top 3 distro Linux
- [ ] **Performance**: Metrics comparabili a Windows

## 🎉 Conclusioni

La migrazione a Linux è **fattibile e strategicamente importante**. Il progetto ha già buone fondamenta cross-platform nel backend .NET Core e alcune parti (come il progetto React) sono già pronte.

### Prossimi Passi Consigliati:

1. **Immediate** (questa settimana):
   - Creare branch dedicato
   - Fix configurazioni .csproj
   - Setup CI per Linux

2. **Short-term** (prossimo mese):
   - Implementare helper classes
   - Fix path critici
   - Test build base

3. **Long-term** (prossimo quarter):
   - Completare migrazione
   - Beta testing community
   - Release ufficiale Linux

### Benefici Attesi:

- 📈 **Aumento user base** del 30-40%
- 🌍 **Presenza nel mercato Linux** enterprise/developer
- 🔧 **Codebase più manutenibile** e testabile
- 🚀 **Foundation per future features** cross-platform

---

*Documento preparato per il team di sviluppo MdExplorer*
*Data: 2025-08-04*
*Versione: 1.0*