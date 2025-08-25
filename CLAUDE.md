---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 03/07/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# CLAUDE

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## IMPORTANTE: Confermare lettura di questo file
All'inizio di ogni sessione, confermare all'utente di aver letto e compreso le regole in questo file CLAUDE.md.

## IMPORTANTE: Ambiente di Sviluppo
* **All'inizio di ogni nuova sessione/contesto, SEMPRE chiedere all'utente se sta sviluppando in Windows con PowerShell oppure in Linux con Bash**
* Questa informazione è essenziale per fornire i comandi corretti per l'ambiente specifico
* Non assumere mai l'ambiente, chiedere sempre

## Development Guidance

### Requisiti Node.js per Angular
* **IMPORTANTE: Per compilare il frontend Angular è necessario usare Node.js versione 14.21.3**
  - Il progetto Angular 11 richiede questa versione specifica
  - Usare nvm per switchare alla versione corretta: `nvm use 14.21.3`
  - NON tentare di compilare con versioni più recenti di Node.js
  - **Comando completo per compilare Angular dalla directory client2**:
    ```bash
    source ~/.nvm/nvm.sh && nvm use 14.21.3 && npm run build
    ```

### Compatibilità Multi-piattaforma
* **IMPORTANTE: Il progetto è multipiattaforma e DEVE funzionare su Windows, Mac e Linux**
  - Usare sempre `Path.Combine()` per costruire percorsi file system
  - Usare `Path.DirectorySeparatorChar` quando necessario per il file system
  - Per i percorsi in Markdown/HTML/Pandoc usare sempre forward slash `/`
  - Verificare sempre che le modifiche siano cross-platform
  - Evitare assunzioni specifiche del sistema operativo

* Always explain the reasoning behind code implementation before writing the actual code
  - Provide a clear breakdown of the approach
  - Discuss potential alternative solutions
  - Outline the specific steps that will be taken
* Le compilazioni di Angular e dotnet, le faccio io
* Prima di cancellare qualsiasi cosa, controlla se esisteva già. Non cancellare mai file che non hai creato tu in questa sessione
* Sii veloce, ma non avere fretta. Lavora in modo tranquillo, rilassato e concentrato
* Verifica sempre con log o altre evidenze tutte le ipotesi prima di agire. Non assumere, verifica
* **Prima di proporre soluzioni, è necessario comprendere completamente la causa del problema** attraverso un'analisi approfondita del codice e del flusso di esecuzione

## Scrittura Documentazione

### REGOLA FONDAMENTALE PER LA DOCUMENTAZIONE
**NON SCRIVERE MAI CODICE NEI DOCUMENTI** a meno che non sia esplicitamente richiesto dall'utente nel prompt. Quando si scrive documentazione:
- Descrivere concetti, architetture e soluzioni in modo narrativo
- Usare diagrammi, tabelle e liste per organizzare le informazioni
- Spiegare il "cosa" e il "perché", non il "come" implementativo
- Se è necessario fare riferimento a codice, indicare solo il nome del file e la posizione, senza includerlo

### Naming Convention per Sprint e Documenti
**IMPORTANTE: Seguire sempre queste regole per i file markdown nella cartella `/sprints` e simili**:
- **Formato nome file**: `Anno-mese-giorno-NomeDescrittivo.md` (es: `2025-08-21-Sprint-AI-RAG-Implementation.md`)
- **Prefisso data sempre**: Utilizzare formato `YYYY-MM-DD` come prefisso per ordinamento cronologico
- **Niente emoji nei nomi file**: Rimuovere emoji come 🚀, 🔄, etc. dai nomi dei file
- **Titolo documento = nome file**: Il titolo H1 nel documento deve corrispondere al nome del file (senza il prefisso data)
- **Esempi corretti**:
  - `2025-06-01-Sprint.md` con titolo `# Sprint`
  - `2025-08-10-Sprint-Chat-AI.md` con titolo `# Sprint Chat AI`
  - `2025-12-06-Git-Authentication-Refactoring.md` con titolo `# Git Authentication Refactoring`

## Pattern di Debug tramite Log Strutturati

**REGOLA FONDAMENTALE**: Quando si verifica un problema di comunicazione tra componenti o servizi, implementare SEMPRE log strutturati per verificare le ipotesi prima di proporre soluzioni.

### Struttura dei Log di Debug

1. **Identificare i punti critici del flusso**:
   - Punto di origine (dove parte l'azione)
   - Punto di transito (servizi/metodi intermedi)
   - Punto di destinazione (dove dovrebbe arrivare l'azione)

2. **Implementare log dettagliati in ogni punto** per tracciare:
   - Chiamate ai metodi
   - Stato prima e dopo le operazioni
   - Parametri passati
   - Risultati ottenuti

3. **Per i servizi con Observable/BehaviorSubject** verificare:
   - Valore corrente dell'Observable
   - Numero di observers registrati
   - Cambiamenti di valore

4. **Analizzare i log per identificare**:
   - Se il flusso si interrompe in qualche punto
   - Se ci sono istanze multiple di servizi (observers count diversi)
   - Se i valori cambiano correttamente
   - Se gli eventi vengono emessi e ricevuti

### Esempio Pratico

Il caso dei pulsanti backward/forward ha dimostrato l'efficacia di questo approccio:
- Log iniziali hanno mostrato che il servizio veniva chiamato correttamente
- Log del servizio hanno rivelato 0 observers quando chiamato da TitleBar vs 3 observers quando chiamato da altri componenti
- Questo ha portato alla scoperta di istanze multiple del servizio causate da provider duplicati
* **Quando lavori con dati persistenti**: Consulta sempre il documento [workflow-sviluppo-dati-persistenti.md](./workflow-sviluppo-dati-persistenti.md) per seguire il processo corretto di:
  - Creazione migrations con FluentMigrator
  - Mapping entità con NHibernate
  - Implementazione API e servizi Angular
  - Questo documento contiene esempi pratici e best practices specifiche del progetto

## IMPORTANTE: Pattern Obbligatori per Dati Persistenti

**SEMPRE seguire questi pattern quando si lavora con database o configurazioni:**

1. **Migrations Database**: SEMPRE usare FluentMigrator
   - MAI script SQL diretti
   - Percorsi: `MdExplorer.Migrations/` (User DB), `MdExplorer.Migrations.EngineDb/`, `MdExplorer.Migrations.ProjectDb/`
   - Naming: `M{Anno}_{Mese}_{Giorno}_{Numero}` es: `M2024_08_24_001`

2. **ORM e Mapping**: SEMPRE usare NHibernate con FluentNHibernate
   - Mappings in: `MdExplorer.Abstractions/Mappings/`
   - Entità in: `MdExplorer.Abstractions/Entities/UserDB/` o `EngineDB/`
   - Pattern ClassMap\<T> per i mapping

3. **Configurazioni Applicazione**: SEMPRE usare tabella Setting esistente
   - Per configurazioni utente modificabili
   - Struttura: Name (string), ValueString, ValueInt, ValueBool, etc.
   - Accesso via: `IUserSettingsDB.GetDal<Setting>()`

4. **Data Access Layer**: 
   - Codice esistente: usa `Ad.Tools.Dal` con pattern DAL
   - Nuovo codice: preferire `Ad.Tools.Dal.Evo` con `IRepository<T>` e `IUnitOfWork`
   - Entrambi coesistono durante la migrazione (Strangler Fig Pattern)

5. **Per dettagli completi**: vedere sempre [workflow-sviluppo-dati-persistenti.md](./workflow-sviluppo-dati-persistenti.md)

## Ambiente di Sviluppo - Bash su Linux

* **IMPORTANTE**: L'ambiente di sviluppo è Bash su Linux
* Quando si usa la modalità agentica (Task tool), utilizzare comandi bash/shell Unix
* Esempi di comandi da usare:
  - `ls` per listare file e directory
  - `grep` per cercare nei file
  - `cat` per visualizzare contenuti
  - `cd` per cambiare directory
  - `test` o `[ -e file ]` per verificare l'esistenza di file/directory

## Git Commit Rules
* NON inserire MAI riferimenti a Claude, Anthropic o AI nei messaggi di commit
* NON usare "Co-Authored-By: Claude" o simili
* I commit devono essere scritti come se fossero stati fatti da uno sviluppatore umano

## Architecture Overview

MdExplorer is a multi-project .NET solution for Markdown file management and editing with the following architecture:

### Core Services

* **MdExplorer.Service** - ASP.NET Core 3.1 web API serving Angular and React frontends
* **MdExplorer.Features** - Business logic layer containing commands, exports (PDF/Word), and feature implementations
* **Data Access Layer** - Currently undergoing migration from `Ad.Tools.Dal` to `Ad.Tools.Dal.Evo` using Strangler Fig Pattern
  * Uses NHibernate ORM with FluentNHibernate mappings
  * SQLite databases: `MdExplorer.db` (user data), `MdEngine.db` (engine data), project-specific DBs
  * FluentMigrator for database schema migrations

### Frontend Applications

* **Angular Client** (`MdExplorer/client2`) - Main UI built with Angular 11 and Material Design
* **React Editor** (`MdEditor.React`) - Milkdown-based markdown editor using React 19
* **Electron Desktop** (`ElectronMdExplorer`) - Desktop wrapper for the web application

### Key Patterns

* Command Pattern for markdown transformations (see `Commands/` folders)
* Repository/Unit of Work pattern (being introduced in new DAL)
* SignalR for real-time file monitoring
* Dependency Injection throughout

## Common Development Commands

### Backend Development

```Shell
# Build entire solution
dotnet build

# Run the main service
dotnet run --project MdExplorer/MdExplorer.Service.csproj

# Run specific tests
dotnet test MdExplorer.Features.Tests/MdExplorer.Features.Tests.csproj
```

### Frontend Development

#### Angular Client

```Shell
cd MdExplorer/client2
npm install
npm run build    # Production build with correct base href
npm start        # Development server
npm run lint     # Run linter
npm test         # Run tests
```

#### React Editor

```Shell
cd MdEditor.React
npm install
npm run build    # Production build
npm run dev      # Development server with Vite
npm run lint     # Run ESLint
```

#### Electron Desktop

```Shell
cd ElectronMdExplorer
npm install
npm start        # Run Electron app
npm run build    # Build distributables
```

## Database Migrations

The project uses FluentMigrator for database schema management:

* User DB migrations: `MdExplorer.Migrations/`
* Engine DB migrations: `MdExplorer.Migrations.EngineDb/`
* Project DB migrations: `MdExplorer.Migrations.ProjectDb/`

Migrations run automatically on startup.

## Key Features & Integration Points

### PlantUML Integration

* Server configured in `appsettings.json` under `MdExplorer.PlantumlServer`
* JAR files located in `MdExplorer/Binaries/`
* Conversion handled by `FromPlantumlToPng` and `FromPlantumlToSvg` commands

### Git Integration

* Uses LibGit2Sharp for Git operations
* Controllers: `BranchesController`, `TagsController`, `GitFeatureController`
* Service: `GitService` in Features project

### Document Export

* PDF export uses Pandoc with eisvogel template
* Word export uses custom DOCX templates in `MdExplorer/templates/word/`
* Implementation in `PdfFeatures` and `WordFeatures` classes

### Real-time Features

* SignalR hub: `MonitorMDHub`
* File monitoring: `MonitorMDHostedService`
* Refactoring support with file tracking

## Testing Approach

* Test files organized by feature in test projects
* Test data includes sample Markdown, HTML, DOCX, and YAML files
* Angular tests use Karma/Jasmine
* .NET tests use MSTest framework

## Active Refactoring

The DAL is being migrated from `Ad.Tools.Dal` to `Ad.Tools.Dal.Evo` following the Strangler Fig Pattern. See `Ad.Tools.Dal/EvoluzionDal.md` for details. When working with data access:

* New features should use `IUnitOfWork` and `IRepository<T>` from `Ad.Tools.Dal.Evo.Abstractions`
* Existing code may still use the old DAL interfaces
* Both DALs currently coexist in the application

<br />