---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title:
date: 08/10/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages:
---
# Preparazione Open Source

Piano completo e metodico per preparare MdExplorer alla pubblicazione come progetto open source su GitHub, garantendo sicurezza, professionalità e assenza di problemi legali o di privacy.

## Panoramica Situazione Attuale

### Repository Analizzato
- **Dimensione git**: 818MB (.git directory), 388MB pack size
- **Progetti**: Soluzione .NET multi-progetto con frontend Angular 11 e Electron
- **Submodules**: 2 repository già su GitHub (website ed electron)
- **Remote attuale**: `https://github.com/salaroglio/MdExplorer.git`

### Dipendenze e Licenze Verificate
Tutte le dipendenze sono compatibili con open source:
- **MIT**: 83% delle librerie
- **Apache-2.0**: 11% delle librerie
- **0BSD**: 6% delle librerie

Nessuna dipendenza richiede pagamento o ha vincoli incompatibili con pubblicazione open source.

## FASE 1: Sicurezza e Rimozione Dati Sensibili (CRITICA)

### 1.1 Configurazioni con Dati Aziendali/Personali

#### File `MdExplorer/appsettings.json`
**PROBLEMA CRITICO**: Contiene riferimenti interni aziendali

```json
"PlantumlServer": "172.19.243.45",  // IP INTERNO - RIMUOVERE
"JiraServer": "http://jira.swarco.com"  // SERVER AZIENDALE - RIMUOVERE
```

**AZIONE**:
1. Sostituire con valori placeholder:
   ```json
   "PlantumlServer": "localhost",
   "JiraServer": "http://your-jira-server.com"
   ```
2. Creare file `appsettings.example.json` con configurazione esempio
3. Aggiungere `appsettings.Development.json` a `.gitignore` se contiene dati locali

#### File `CLAUDE.md`
**PROBLEMA**: Header YAML contiene email personale

Attuale:
```yaml
author: Carlo Salaroglio
email: salaroglio@hotmail.com
```

**AZIONE**:
1. Rimuovere header YAML (è per uso personale con editor)
2. Oppure spostare file in `.claude/DEVELOPMENT.md` (directory già in .gitignore)
3. Creare versione pubblica senza email personale

### 1.2 Email Personale nei File

**TROVATO IN**: 39 file (principalmente in cartella `sprints/`, documentazione, website)

**AZIONE**:
- Sostituire `salaroglio@hotmail.com` con email progetto (es: `mdexplorer@example.com`)
- Oppure rimuovere header YAML dai file sprint (sono documentazione interna)
- **File Electron `package.json`**: già corretto con `salaroglio@example.com` ✓

### 1.3 Database e File Generati

**PROBLEMA**: File `.db` non sono in `.gitignore`

**File trovati**:
- `MdExplorer/MdEngine.db`
- `MdExplorer/MdExplorer.db`
- `.md/MdProject_*.db`
- Database in directory `bin/` e `obj/`

**AZIONE**:
Aggiornare `.gitignore`:
```gitignore
# Database files
*.db
*.sqlite
*.sqlite3

# Claude Code local settings
.claude/settings.local.json

# Development metadata
.md/

# VS Copilot indices
.vs/**/CopilotIndices/
```

### 1.4 Verifica Git History

**SITUAZIONE**: Repository di 388MB - necessario verificare non ci siano credenziali in history

**AZIONE**:
1. Eseguire scan con strumenti:
   ```bash
   git log --all --full-history --pretty=format:"%H" -- "**/*password*" "**/*secret*" "**/*token*"
   ```
2. Se trovate credenziali in history, usare:
   - **BFG Repo-Cleaner**: `bfg --delete-files credentials.json`
   - **git filter-repo**: più potente ma richiede riscrittura history
3. Considerare se fare `git clone --depth 1` per nuovo repository pulito (perde history)

### 1.5 File Claude Code Settings

**FILE**: `.claude/settings.local.json`

Contiene permessi specifici utente e path locali. **GIÀ IN .gitignore** ✓

## FASE 2: Licenza e Documentazione Legale

### 2.1 Scelta Licenza

**RACCOMANDAZIONE**: **MIT License**

**Motivazioni**:
- Compatibile al 100% con tutte le dipendenze esistenti
- Permissiva e business-friendly
- Più popolare per progetti .NET e JavaScript
- Permette uso commerciale senza restrizioni

**AZIONE**:
Creare file `LICENSE` nella root:

```
MIT License

Copyright (c) 2025 Carlo Salaroglio

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### 2.2 Copyright nei File Sorgente

**SITUAZIONE ATTUALE**: Solo 2 file `.cs` hanno header copyright

**OPZIONI**:
1. **Non aggiungere header** - file LICENSE è sufficiente (approccio moderno)
2. **Aggiungere header solo a file principali** - controller, servizi core
3. **Aggiungere header a tutti i file** - più formale ma pesante

**RACCOMANDAZIONE**: Opzione 1 - file LICENSE è sufficiente

### 2.3 Package.json Licenze

**Electron** (`ElectronMdExplorer/package.json`):
- Attualmente: `"license": "ISC"`
- **AZIONE**: Cambiare in `"license": "MIT"` per coerenza

**Angular** (`MdExplorer/client2/package.json`):
- Attualmente: `"private": true` (senza campo license)
- **OK**: è componente interno non pubblicato su npm

## FASE 3: Documentazione Essenziale

### 3.1 README.md Principale

**MANCANTE** - file critico per progetto open source

**STRUTTURA RACCOMANDATA**:

```markdown
# MdExplorer

> Professional Markdown editor and project management tool with Git integration, AI-powered features, and multi-platform support.

![MdExplorer Screenshot](docs/images/screenshot.png)

## Features

- 📝 Advanced Markdown editing with live preview
- 📂 Project-based file organization
- 🔄 Integrated Git workflow (commit, push, pull, branches)
- 🤖 AI-powered TOC generation and chat assistant
- 📊 PlantUML diagrams support
- 📄 Export to PDF and Word with custom templates
- 🔍 Full-text search across projects
- 🖥️ Cross-platform: Windows, macOS, Linux

## Technology Stack

- **Backend**: ASP.NET Core 8.0, NHibernate, FluentMigrator
- **Frontend**: Angular 11, Material Design
- **Editor**: React 19 with Milkdown
- **Desktop**: Electron 28
- **Database**: SQLite
- **Git**: LibGit2Sharp

## Prerequisites

- **.NET SDK 8.0** or later
- **Node.js 14.21.3** (required for Angular 11 build)
- **Windows**: Windows 10/11, **macOS**: 10.15+, **Linux**: Modern distro

## Quick Start

### 1. Clone Repository

```bash
git clone --recursive https://github.com/salaroglio/MdExplorer.git
cd MdExplorer
```

### 2. Build Backend

```bash
dotnet restore
dotnet build
```

### 3. Build Angular Frontend

```bash
cd MdExplorer/client2
nvm use 14.21.3  # or switch to Node 14.21.3
npm install
npm run build
```

### 4. Run Application

```bash
cd ../..
dotnet run --project MdExplorer/MdExplorer.Service.csproj
```

Open browser: `http://localhost:5000`

### 5. Build Electron Desktop App

```bash
cd ElectronMdExplorer
npm install
npm start  # Development
npm run build  # Production installer
```

## Configuration

Copy `appsettings.example.json` to `appsettings.json` and configure:

- **PlantUML Server**: Local or remote PlantUML service
- **Git Settings**: Authentication preferences
- **AI Features**: Model paths and GPU settings

See [Configuration Guide](docs/configuration.md) for details.

## Development

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

Key documents:
- [Workflow Dati Persistenti](workflow-sviluppo-dati-persistenti.md) - Database patterns
- [DAL Evolution](Ad.Tools.Dal/EvoluzioneDal.md) - Data access layer migration

## Architecture

```
MdExplorer/
├── MdExplorer/              # ASP.NET Core web service
├── MdExplorer.bll/          # Business logic & features
├── MdExplorer.Abstractions/ # Domain models & interfaces
├── MDExplorer.dal/          # Data access (legacy)
├── Ad.Tools.Dal.Evo/        # Modern data access layer
├── MdExplorer.Migrations/   # Database migrations
├── ElectronMdExplorer/      # Desktop app wrapper
└── MdEditor.React/          # React-based editor
```

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

## Support

- **Issues**: [GitHub Issues](https://github.com/salaroglio/MdExplorer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/salaroglio/MdExplorer/discussions)
- **Website**: https://www.mdexplorer.net

## Acknowledgments

Built with amazing open-source libraries - see [librerie-licenze.md](librerie-licenze.md) for full list.
```

### 3.2 CONTRIBUTING.md

**MANCANTE** - importante per progetti open source

**CONTENUTO BASE**:

```markdown
# Contributing to MdExplorer

Thank you for your interest in contributing!

## How to Contribute

### Reporting Bugs

Use GitHub Issues with the bug report template. Include:
- Operating system and version
- .NET SDK version
- Node.js version
- Steps to reproduce
- Expected vs actual behavior

### Suggesting Features

Use GitHub Issues with feature request template. Explain:
- Use case and problem solved
- Proposed solution
- Alternative solutions considered

### Pull Requests

1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Follow code style (see below)
4. Write tests if applicable
5. Update documentation
6. Commit: `git commit -m "feat: add amazing feature"`
7. Push: `git push origin feature/amazing-feature`
8. Open Pull Request

## Development Setup

See README.md Quick Start section.

### Important: Node.js Version

Angular build requires **Node.js 14.21.3**:

```bash
nvm use 14.21.3
cd MdExplorer/client2
npm install
npm run build
```

## Code Style

### .NET
- Follow Microsoft C# conventions
- Use meaningful names
- Add XML comments to public APIs

### TypeScript/Angular
- Follow Angular style guide
- Use strict typing
- Prefer reactive patterns (RxJS)

### Database Migrations

Always use FluentMigrator - see [workflow-sviluppo-dati-persistenti.md](workflow-sviluppo-dati-persistenti.md)

**Pattern**: `M{Anno}_{Mese}_{Giorno}_{Numero}`
Example: `M2025_10_08_001`

## Cross-Platform Compatibility

**CRITICAL**: Project must work on Windows, macOS, and Linux.

- Use `Path.Combine()` for file system paths
- Use `/` for Markdown/HTML/Pandoc paths
- Test on multiple platforms when possible

## Commit Messages

Follow Conventional Commits:
- `feat:` new feature
- `fix:` bug fix
- `docs:` documentation
- `refactor:` code refactoring
- `test:` adding tests
- `chore:` maintenance

## Questions?

Open a Discussion on GitHub or comment on related Issue.
```

### 3.3 CODE_OF_CONDUCT.md

**MANCANTE** - standard per progetti open source

**RACCOMANDAZIONE**: Usare Contributor Covenant standard

### 3.4 SECURITY.md

**MANCANTE** - importante per segnalazione vulnerabilità

```markdown
# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 2025.x  | :white_check_mark: |
| < 2025  | :x:                |

## Reporting a Vulnerability

**DO NOT** open public GitHub issue for security vulnerabilities.

Instead, email: security@mdexplorer.net (or private email)

Include:
- Description of vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

We will respond within 48 hours and provide updates every 7 days.

## Security Measures

- LibGit2Sharp for Git operations (no shell injection)
- Credential storage using OS-native stores
- Input validation on all user data
- SQLite with parameterized queries (NHibernate)
```

### 3.5 GitHub Templates

**MANCANTE** - migliorano qualità issues/PR

Creare directory `.github/` con:

**`.github/ISSUE_TEMPLATE/bug_report.md`**:
```markdown
---
name: Bug Report
about: Report a bug to help us improve
title: '[BUG] '
labels: bug
assignees: ''
---

**Describe the bug**
Clear description of what the bug is.

**To Reproduce**
Steps to reproduce:
1. Go to '...'
2. Click on '...'
3. See error

**Expected behavior**
What you expected to happen.

**Screenshots**
If applicable, add screenshots.

**Environment:**
- OS: [e.g. Windows 11, macOS 14, Ubuntu 22.04]
- MdExplorer Version: [e.g. 2025.10.02.1]
- .NET SDK: [e.g. 8.0.100]
- Node.js: [e.g. 14.21.3]

**Additional context**
Any other context about the problem.
```

**`.github/ISSUE_TEMPLATE/feature_request.md`**:
```markdown
---
name: Feature Request
about: Suggest an idea for MdExplorer
title: '[FEATURE] '
labels: enhancement
assignees: ''
---

**Is your feature request related to a problem?**
Clear description of the problem. Ex. I'm always frustrated when [...]

**Describe the solution you'd like**
Clear description of what you want to happen.

**Describe alternatives you've considered**
Other solutions or features you've considered.

**Additional context**
Any other context, screenshots, or examples.
```

**`.github/pull_request_template.md`**:
```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Tests added/updated
- [ ] Manual testing completed
- [ ] Tested on multiple platforms (if applicable)

## Checklist
- [ ] Code follows project style
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings
- [ ] Cross-platform compatible

## Related Issues
Closes #(issue number)
```

## FASE 4: Configurazione e Build

### 4.1 File Esempio per Configurazioni

**AZIONE**: Creare `MdExplorer/appsettings.example.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "MdExplorer": "Information"
    },
    "File": {
      "Path": "Logs/mdexplorer-{Date}.log",
      "LogLevel": {
        "Default": "Debug",
        "Microsoft": "Warning"
      }
    }
  },
  "AllowedHosts": "*",
  "MdExplorer": {
    "CreateStartupLog": false,
    "PlantumlServer": "localhost",
    "JiraServer": "http://your-jira-server.com"
  },
  "Git": {
    "Authentication": {
      "PreferredMethods": ["SSH", "GitCredentialHelper", "SystemCredentialStore"],
      "AllowUserPrompt": false,
      "SSHKeySearchPaths": [
        "~/.ssh/id_ed25519",
        "~/.ssh/id_ecdsa",
        "~/.ssh/id_rsa"
      ],
      "CredentialHelperTimeoutSeconds": 30,
      "CacheCredentials": true,
      "CacheTimeoutMinutes": 15
    },
    "Operations": {
      "DefaultAuthor": {
        "UseGitConfig": true,
        "FallbackName": "MdExplorer User",
        "FallbackEmail": "user@mdexplorer.local"
      },
      "PullStrategy": "Merge",
      "PushStrategy": "Simple",
      "DefaultRemote": "origin",
      "OperationTimeoutSeconds": 300
    }
  },
  "TocGeneration": {
    "EnableAI": true,
    "BatchSize": 10,
    "TimeoutSeconds": 30,
    "MaxContentLength": 2000,
    "CacheDays": 30,
    "DefaultPrompt": "Analyze this markdown document and generate a concise description (max 50 words)."
  },
  "AiChat": {
    "Gpu": {
      "EnableAutoDetection": true,
      "PreferGpuAcceleration": true,
      "DefaultGpuLayerCount": -1,
      "ReserveGpuMemoryMB": 2048,
      "LogGpuDetails": true
    },
    "Models": {
      "DefaultContextSize": 4096,
      "MaxTokens": 512,
      "Temperature": 0.7
    }
  }
}
```

Aggiungere nota in README:
```markdown
## First Run Configuration

1. Copy `appsettings.example.json` to `appsettings.json`
2. Update settings for your environment:
   - PlantUML Server (use localhost or remote)
   - Jira integration (optional)
   - AI model paths (if using local AI features)
```

### 4.2 CI/CD con GitHub Actions

**OPZIONALE ma CONSIGLIATO** - automatizza build e test

**`.github/workflows/build.yml`**:

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-backend:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3
      with:
        submodules: recursive

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore --configuration Release

    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal

  build-frontend:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup Node.js 14.21.3
      uses: actions/setup-node@v3
      with:
        node-version: '14.21.3'

    - name: Build Angular
      working-directory: MdExplorer/client2
      run: |
        npm ci
        npm run build

    - name: Lint
      working-directory: MdExplorer/client2
      run: npm run lint

  build-electron:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]

    steps:
    - uses: actions/checkout@v3
      with:
        submodules: recursive

    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18.x'

    - name: Build Electron
      working-directory: ElectronMdExplorer
      run: |
        npm ci
        npm run build
```

### 4.3 Docker per Sviluppo

**OPZIONALE** - facilita setup ambiente sviluppo

**`Dockerfile.dev`**:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install Node.js 14.21.3 via nvm
RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash
ENV NVM_DIR=/root/.nvm
RUN . "$NVM_DIR/nvm.sh" && nvm install 14.21.3 && nvm use 14.21.3

# Install build tools
RUN apt-get update && apt-get install -y \
    git \
    sqlite3 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy solution
COPY . .

# Restore .NET dependencies
RUN dotnet restore

# Build Angular (requires Node 14.21.3)
WORKDIR /app/MdExplorer/client2
RUN . "$NVM_DIR/nvm.sh" && nvm use 14.21.3 && npm install && npm run build

WORKDIR /app
EXPOSE 5000

CMD ["dotnet", "run", "--project", "MdExplorer/MdExplorer.Service.csproj"]
```

**`docker-compose.yml`**:

```yaml
version: '3.8'

services:
  mdexplorer:
    build:
      context: .
      dockerfile: Dockerfile.dev
    ports:
      - "5000:5000"
    volumes:
      - .:/app
      - /app/MdExplorer/client2/node_modules
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

## FASE 5: Pulizia Codice

### 5.1 TODO/FIXME/HACK

**TROVATI**: 41 occorrenze in 12 file

**File con più TODO**:
- `MdExplorer/Services/Git/ModernGitService.cs`: 13 occorrenze
- `MdExplorer/Startup.cs`: 2 occorrenze
- Vari altri file: 1 occorrenza ciascuno

**AZIONE**:
1. **Revisionare ogni TODO**:
   - Se implementabile rapidamente → implementare
   - Se richiede lavoro → creare GitHub Issue
   - Se obsoleto → rimuovere
2. **Pattern consigliato**: Convertire in GitHub Issues con label `todo`
3. Lasciare solo TODO ben documentati con contesto

### 5.2 File Test/Debug Root

**PROBLEMI IDENTIFICATI**:

File nella root che sembrano test/debug:
- `WindowsCredentialStoreResolver_Enhanced.cs` - file test/esperimento
- `GitCredentialTest.cs` - file test

**AZIONE**:
1. **Spostare** in directory test appropriata o
2. **Rimuovere** se obsoleti
3. Verificare non siano referenziati in soluzione

### 5.3 Cartelle Documentazione Interna

**PROBLEMA**: Cartelle con documentazione sviluppo personale/interna:

- `sprints/` - documenti sprint interni (20 file)
- `analisi/` - analisi interne (4 file)
- `differenzeClaude-Copilot/` - note comparative personali (2 file)
- `migrazione_linux/` - note migrazione (2 file)

**OPZIONI**:

**Opzione A - Mantenere nel Repository**:
- PRO: Trasparenza completa, utile per contributori
- CONTRO: Contiene email personali, note informali
- AZIONE: Rimuovere email personali dai file

**Opzione B - Repository Separato** (CONSIGLIATO):
- Creare repository privato `MdExplorer-Internal-Docs`
- Spostare cartelle `sprints/`, `analisi/`, `differenzeClaude-Copilot/`
- Mantenere solo documentazione tecnica pubblica:
  - `workflow-sviluppo-dati-persistenti.md` ✓
  - `Ad.Tools.Dal/EvoluzioneDal.md` ✓
  - `librerie-licenze.md` ✓

**Opzione C - Aggiungere a .gitignore**:
```gitignore
# Internal development documentation
/sprints/
/analisi/
/differenzeClaude-Copilot/
/migrazione_linux/
```
- PRO: Mantiene files localmente per riferimento
- CONTRO: Perde storia git di questi file

**RACCOMANDAZIONE**: Opzione B per primo rilascio open source, poi Opzione A se vuoi condividere processo sviluppo.

### 5.4 File Backup in Soluzione

**TROVATI** in `.csproj`:
- `MdExplorer.bll/MdExplorer - Backup (1).Features.csproj`
- `MdExplorer.bll/MdExplorer - Backup.Features.csproj`

**AZIONE**: Rimuovere file backup dalla soluzione e filesystem

## FASE 6: Submodules e Dipendenze Esterne

### 6.1 Gestione Submodules

**SUBMODULES IDENTIFICATI**:

1. **mdExplorerWebSite/mdExplorerWebSite**
   - URL: `https://github.com/salaroglio/mdexplorer-website.git`
   - Scopo: Website ufficiale progetto
   - **AZIONE**: Verificare che repository sia pubblico e ready per open source

2. **ElectronMdExplorer**
   - URL: `https://github.com/salaroglio/electron-mdexplorer.git`
   - Scopo: App Electron desktop
   - **AZIONE**: Verificare licenza MIT e nessun dato sensibile

**CHECKLIST per ogni submodule**:
- [ ] Repository pubblico
- [ ] File LICENSE presente
- [ ] README.md presente
- [ ] Nessuna credenziale o dato sensibile
- [ ] Package.json con licenza corretta

### 6.2 Binari e Asset Esterni

**PlantUML JAR Files** in `MdExplorer/Binaries/`:

**VERIFICA NECESSARIA**:
- PlantUML è Apache License 2.0 - compatibile con MIT ✓
- Verificare se distribuire JAR o documentare download separato

**RACCOMANDAZIONE**:
- **Opzione 1**: Includere JAR (file binario, ma licenza permette)
- **Opzione 2**: Documentare download manuale in README
- **Opzione 3**: Script setup automatico che scarica JAR

### 6.3 WiX Installer Assets

**TROVATI**: `MdExplorer.Setup/` e `MdExplorer.Bootstrapper/`

**VERIFICA**:
- File RTF licenza
- Immagini installer
- Certificati di firma (NON devono essere in repository)

**AZIONE**:
- Verificare `.gitignore` esclude `*.pfx`, `*.p12` ✓ (già presente)
- Verificare nessun certificato committato (scan fatto - OK ✓)

## FASE 7: Repository GitHub

### 7.1 Impostazioni Repository GitHub

**Quando il repository diventa pubblico**:

**About Section**:
- Description: "Professional Markdown editor and project management tool with Git integration, AI features, and cross-platform support"
- Website: https://www.mdexplorer.net
- Topics: `markdown`, `editor`, `git`, `electron`, `dotnet`, `angular`, `project-management`, `ai`, `plantuml`

**Features da Abilitare**:
- [x] Wikis - per documentazione estesa
- [x] Issues - per bug tracking e feature requests
- [x] Discussions - per Q&A community
- [x] Projects - per roadmap pubblica
- [x] Preserve this repository - backup GitHub

### 7.2 Branch Protection

**Settings > Branches > Add branch protection rule**:

Per branch `main`:
- [x] Require pull request before merging
  - [x] Require approvals: 1
  - [x] Dismiss stale pull request approvals
- [x] Require status checks to pass
  - [x] Require branches to be up to date
  - Status checks: Build backend, Build frontend
- [x] Require conversation resolution before merging
- [x] Include administrators (opzionale)

### 7.3 Release Strategy

**Prima Release: v2025.10.02.1**

Preparare:
- [ ] Tag git: `git tag -a v2025.10.02.1 -m "First public release"`
- [ ] Release notes su GitHub
- [ ] Asset binari:
  - Windows installer (.exe)
  - Linux AppImage / .deb
  - macOS .dmg (se disponibile)
- [ ] Changelog completo

**Release Notes Template**:

```markdown
# MdExplorer v2025.10.02.1 - First Public Release

We're excited to release MdExplorer as an open-source project!

## 🎉 What is MdExplorer?

Professional Markdown editor and project management tool with:
- Advanced Markdown editing with live preview
- Integrated Git workflow
- AI-powered features (TOC generation, chat assistant)
- PlantUML diagrams support
- Export to PDF/Word
- Cross-platform desktop app (Electron)

## ✨ Key Features in this Release

- Full Git integration (commit, push, pull, branches)
- Modern Angular 11 UI with Material Design
- React-based Milkdown editor
- AI chat with local models (GPU acceleration support)
- Project-based organization with SQLite
- Multi-platform: Windows, macOS, Linux

## 📦 Installation

See [README.md](README.md) for build instructions.

Pre-built binaries:
- **Windows**: MdExplorer-Setup-2025.10.02.1.exe
- **Linux**: MdExplorer-2025.10.02.1.AppImage
- **macOS**: Coming soon

## 🚀 Getting Started

1. Download installer for your platform
2. Run the application
3. Create or open a project
4. Start editing Markdown!

## 📄 License

MIT License - see [LICENSE](LICENSE)

## 🙏 Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md)

## 🐛 Known Issues

- Angular build requires specific Node.js 14.21.3
- PlantUML server needs manual configuration

## 📝 Full Changelog

Initial open-source release. For development history, see closed-source repository.
```

### 7.4 Social Preview

**GitHub Repository Settings > Social Preview**:

Creare immagine 1280x640px con:
- Logo/icona MdExplorer
- Tagline: "Professional Markdown Editor & Project Management"
- Screenshot applicazione

## FASE 8: Post-Pubblicazione

### 8.1 Community Setup

**GitHub Discussions**:
- Categorie:
  - 💬 General
  - 💡 Ideas
  - 🙏 Q&A
  - 📣 Announcements
  - 🎯 Show and tell

**Project Board**:
- Colonne:
  - 📋 Backlog
  - 🔜 To Do
  - 🏗️ In Progress
  - ✅ Done

### 8.2 Promozione

**Dove annunciare**:
- [ ] Reddit: r/dotnet, r/opensource, r/selfhosted
- [ ] Hacker News: Show HN
- [ ] Twitter/X: hashtag #opensource #markdown #dotnet
- [ ] Dev.to: articolo introduttivo
- [ ] LinkedIn: post personale
- [ ] Product Hunt (opzionale)

**Messaggio tipo**:
```
🎉 Excited to announce MdExplorer is now open source!

A professional Markdown editor with Git integration, AI features, and cross-platform support.

Built with .NET, Angular, and Electron.

⭐ Star on GitHub: github.com/salaroglio/MdExplorer
📖 Docs & downloads: mdexplorer.net

#opensource #markdown #dotnet #angular
```

### 8.3 Monitoring e Manutenzione

**Metriche da Monitorare**:
- GitHub Stars, Forks, Watchers
- Issues aperte/chiuse
- Pull requests
- Contributors
- Downloads release

**Impegno Tempo**:
- Rispondere issues: 2-3 volte/settimana
- Review PR: entro 3-5 giorni
- Release nuove versioni: mensile o per feature importanti

## Rischi e Mitigazioni

### Rischi Identificati

| Rischio | Probabilità | Impatto | Mitigazione |
|---------|-------------|---------|-------------|
| **Credenziali in git history** | Media | CRITICO | Scan completo con BFG, nuovo repository se necessario |
| **Email personale esposta** | Alta | Basso | Sostituzione automatica in tutti i file |
| **IP interno aziendale pubblico** | Alta | Medio | Rimozione da appsettings.json |
| **Riferimenti SWARCO** | Alta | Basso | Rimozione da configurazioni |
| **Dimensione repo 388MB** | Alta | Basso | Considerare GitHub LFS per binari grandi |
| **Supporto community richiedente** | Media | Basso | Templates chiari, FAQ, automazioni |
| **Contributori con stili diversi** | Media | Basso | CONTRIBUTING.md con linee guida chiare |
| **Breaking changes da contributori** | Bassa | Medio | Branch protection, code review obbligatoria |

### Mitigazioni Tecniche

**Scan Automatico Segreti**:

Aggiungere GitHub Action per scan segreti:

```yaml
# .github/workflows/secret-scan.yml
name: Secret Scanning

on: [push, pull_request]

jobs:
  scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: TruffleHog
        uses: trufflesecurity/trufflehog@main
        with:
          path: ./
          base: ${{ github.event.repository.default_branch }}
          head: HEAD
```

## Checklist Finale Pre-Pubblicazione

### Sicurezza
- [ ] Nessuna password/token in repository
- [ ] Nessuna credenziale in git history
- [ ] IP/hostname interni rimossi
- [ ] Email personali rimosse/sostituite
- [ ] `.gitignore` aggiornato per file sensibili
- [ ] Scan con git-secrets o simili
- [ ] Database `.db` non committati

### Legale
- [ ] File LICENSE creato (MIT)
- [ ] Package.json con licenza corretta
- [ ] Nessun codice proprietario terze parti
- [ ] Dipendenze tutte compatibili
- [ ] Copyright notices (se applicabile)

### Documentazione
- [ ] README.md completo e chiaro
- [ ] CONTRIBUTING.md con guidelines
- [ ] CODE_OF_CONDUCT.md
- [ ] SECURITY.md per vulnerabilità
- [ ] Issue templates
- [ ] PR template
- [ ] appsettings.example.json

### Codice
- [ ] TODO/FIXME revisionati
- [ ] File test/debug rimossi
- [ ] Build funziona su macchina pulita
- [ ] Test passano
- [ ] Nessun warning critico
- [ ] Cross-platform verificato

### Repository
- [ ] Branch protection configurata
- [ ] Description e topics impostati
- [ ] Social preview image
- [ ] Submodules verificati
- [ ] Prima release preparata
- [ ] CHANGELOG.md iniziale

### Build & Deploy
- [ ] CI/CD configurato (opzionale)
- [ ] Build instructions testate
- [ ] Installer funzionanti
- [ ] Auto-update configurato
- [ ] Docker setup (opzionale)

## Timeline Consigliata

### Settimana 1: Preparazione Sicurezza
- **Giorno 1-2**: Rimozione dati sensibili (email, IP, server aziendali)
- **Giorno 3**: Scan git history, pulizia se necessario
- **Giorno 4**: Aggiornamento .gitignore, verifica database
- **Giorno 5**: Review finale sicurezza

### Settimana 2: Documentazione e Licenza
- **Giorno 1**: Creazione LICENSE e aggiornamento package.json
- **Giorno 2-3**: Scrittura README.md completo
- **Giorno 4**: CONTRIBUTING.md, CODE_OF_CONDUCT.md, SECURITY.md
- **Giorno 5**: Issue/PR templates

### Settimana 3: Pulizia Codice e Testing
- **Giorno 1-2**: Gestione TODO/FIXME
- **Giorno 3**: Rimozione file debug/backup
- **Giorno 4**: Gestione documentazione interna
- **Giorno 5**: Test build completa su macchina pulita

### Settimana 4: Pubblicazione e Launch
- **Giorno 1**: Configurazione repository GitHub
- **Giorno 2**: Creazione prima release
- **Giorno 3**: Setup CI/CD (opzionale)
- **Giorno 4**: Verifica submodules
- **Giorno 5**: 🚀 PUBBLICAZIONE e annunci

**Tempo totale stimato**: 3-4 settimane part-time o 1-2 settimane full-time

## Risorse e Riferimenti

### Tool Utili
- **BFG Repo-Cleaner**: https://rtyley.github.io/bfg-repo-cleaner/
- **git-secrets**: https://github.com/awslabs/git-secrets
- **TruffleHog**: https://github.com/trufflesecurity/trufflehog
- **Choose a License**: https://choosealicense.com/
- **GitHub Badges**: https://shields.io/

### Guide Ufficiali
- GitHub Open Source Guide: https://opensource.guide/
- Microsoft Open Source Guide: https://docs.opensource.microsoft.com/
- Contributor Covenant: https://www.contributor-covenant.org/

### Community
- Awesome Open Source: https://awesomeopensource.com/
- First Timers Only: https://www.firsttimersonly.com/

## Note Finali

### Punti di Forza MdExplorer
- Stack tecnologico moderno e professionale
- Architettura ben organizzata
- Cross-platform reale
- Features uniche (Git + AI + Markdown)
- Documentazione tecnica già esistente di buona qualità

### Aree di Miglioramento per Open Source
- Mancanza README principale
- Documentazione setup per nuovi contributori
- Test coverage (non analizzato in dettaglio)
- Internazionalizzazione (UI principalmente in italiano?)

### Opportunità
- Community .NET/Angular numerosa
- Nicchia Markdown + Git poco coperta
- Features AI attraenti per developer
- Potenziale integrazioni (VS Code extension, CLI tool)

## Prossimi Passi Immediati

1. **CRITICO**: Rimuovere IP `172.19.243.45` e `jira.swarco.com` da appsettings.json
2. **CRITICO**: Scan git history per credenziali
3. Decidere strategia per cartella `sprints/` (mantenere/spostare/rimuovere)
4. Creare LICENSE file
5. Iniziare README.md

---

**Documento compilato**: 08/10/2025
**Versione**: 1.0
**Prossimo review**: Post-pubblicazione +1 mese
