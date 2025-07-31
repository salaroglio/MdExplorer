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

## Development Guidance

* Always explain the reasoning behind code implementation before writing the actual code
  - Provide a clear breakdown of the approach
  - Discuss potential alternative solutions
  - Outline the specific steps that will be taken
* Le compilazioni di Angular e dotnet, le faccio io
* Prima di cancellare qualsiasi cosa, controlla se esisteva già. Non cancellare mai file che non hai creato tu in questa sessione
* Sii veloce, ma non avere fretta. Lavora in modo tranquillo, rilassato e concentrato
* Verifica sempre con log o altre evidenze tutte le ipotesi prima di agire. Non assumere, verifica
* **Prima di proporre soluzioni, è necessario comprendere completamente la causa del problema** attraverso un'analisi approfondita del codice e del flusso di esecuzione
* **Quando lavori con dati persistenti**: Consulta sempre il documento [workflow-sviluppo-dati-persistenti.md](./workflow-sviluppo-dati-persistenti.md) per seguire il processo corretto di:
  - Creazione migrations con FluentMigrator
  - Mapping entità con NHibernate
  - Implementazione API e servizi Angular
  - Questo documento contiene esempi pratici e best practices specifiche del progetto

## Ambiente di Sviluppo - PowerShell su Windows

* **IMPORTANTE**: L'ambiente di sviluppo è PowerShell su Windows, NON WSL
* Quando si usa la modalità agentica (Task tool), utilizzare comandi PowerShell e non comandi bash/shell Unix
* Esempi di comandi PowerShell da usare:
  - `Get-ChildItem` o `dir` invece di `ls`
  - `Select-String` invece di `grep`
  - `Get-Content` invece di `cat`
  - `Set-Location` o `cd` per cambiare directory
  - `Test-Path` per verificare l'esistenza di file/directory

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