# MdExplorer

> Professional Markdown editor and project management tool with Git integration, AI-powered features, and multi-platform support.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-11-DD0031)](https://angular.io/)
[![Electron](https://img.shields.io/badge/Electron-28-47848F)](https://www.electronjs.org/)

## Features

- 📝 **Advanced Markdown Editing** - Live preview with React-based Milkdown editor
- 📂 **Project-Based Organization** - Manage multiple documentation projects with SQLite backend
- 🔄 **Integrated Git Workflow** - Commit, push, pull, branches, and tags management
- 🤖 **AI-Powered Features** - TOC generation and chat assistant with local model support
- 📊 **PlantUML Diagrams** - Create and preview diagrams directly in your documents
- 📄 **Professional Export** - Generate PDF and Word documents with custom templates
- 🔍 **Full-Text Search** - Fast search across all your markdown files
- 🖥️ **Cross-Platform** - Works on Windows, macOS, and Linux
- 🚀 **Desktop App** - Native Electron wrapper with auto-update support

## Technology Stack

- **Backend**: ASP.NET Core 8.0, NHibernate ORM, FluentMigrator
- **Frontend**: Angular 11 with Material Design
- **Editor**: React 19 with Milkdown
- **Desktop**: Electron 28
- **Database**: SQLite with automatic migrations
- **Git**: LibGit2Sharp
- **Export**: Pandoc (PDF), custom DOCX templates (Word)

## Prerequisites

- **.NET SDK 8.0** or later - [Download](https://dotnet.microsoft.com/download)
- **Node.js 14.21.3** (required for Angular 11 build) - [Download with nvm](https://github.com/nvm-sh/nvm)
- **Windows**: Windows 10/11, **macOS**: 10.15+, **Linux**: Modern distribution

## Quick Start

### 1. Clone Repository

```bash
git clone --recursive https://github.com/salaroglio/MdExplorer.git
cd MdExplorer
```

Note: Use `--recursive` to include submodules (Electron app and website).

### 2. Build Backend

```bash
dotnet restore
dotnet build
```

### 3. Build Angular Frontend

**Important**: Angular 11 requires Node.js 14.21.3

```bash
cd MdExplorer/client2

# Switch to Node 14.21.3 (using nvm)
nvm use 14.21.3

npm install
npm run build
```

### 4. Run Application

```bash
cd ../..
dotnet run --project MdExplorer/MdExplorer.Service.csproj
```

Open your browser at: `http://localhost:5000`

### 5. Build Electron Desktop App (Optional)

```bash
cd ElectronMdExplorer
npm install
npm start           # Development mode
npm run build       # Production installer
```

## Configuration

On first run, the application creates default configuration. To customize:

1. Copy `MdExplorer/appsettings.example.json` to `MdExplorer/appsettings.json`
2. Configure settings:
   - **PlantUML Server**: URL of PlantUML service (local or remote)
   - **Git Authentication**: Preferred methods and SSH key paths
   - **AI Features**: Model paths and GPU acceleration settings
   - **Jira Integration**: Optional Jira server URL

See [Configuration Guide](docs/configuration.md) for detailed options.

## Development

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines and setup instructions.

### Key Development Documents

- [Workflow Dati Persistenti](workflow-sviluppo-dati-persistenti.md) - Database patterns and migrations
- [DAL Evolution](Ad.Tools.Dal/EvoluzioneDal.md) - Data access layer migration guide
- [CLAUDE.md](CLAUDE.md) - AI-assisted development guidelines

### Project Structure

```
MdExplorer/
├── MdExplorer/                  # ASP.NET Core web service
│   ├── client2/                 # Angular 11 frontend
│   ├── Controllers/             # REST API controllers
│   ├── Services/                # Backend services
│   └── appsettings.json         # Configuration
├── MdExplorer.Features/         # Business logic & features
├── MdExplorer.Abstractions/     # Domain models & interfaces
├── MDExplorer.dal/              # Data access (legacy)
├── Ad.Tools.Dal.Evo/            # Modern data access layer
├── MdExplorer.Migrations/       # Database migrations (User DB)
├── MdExplorer.Migrations.EngineDb/   # Engine DB migrations
├── MdExplorer.Migrations.ProjectDb/  # Project DB migrations
├── ElectronMdExplorer/          # Desktop app wrapper
└── MdEditor.React/              # React-based Milkdown editor
```

### Running Tests

```bash
# Backend tests
dotnet test

# Angular tests
cd MdExplorer/client2
npm test

# Angular linting
npm run lint
```

## Architecture Highlights

- **Multi-Database**: Separate SQLite databases for user settings, engine data, and projects
- **FluentMigrator**: Automatic database schema migrations on startup
- **NHibernate**: ORM with FluentNHibernate mappings
- **SignalR**: Real-time file monitoring and updates
- **Command Pattern**: Extensible markdown transformation pipeline
- **Strangler Fig Pattern**: Gradual migration from legacy DAL to modern repository pattern

## Export Features

### PDF Export
- Uses Pandoc with eisvogel template
- Custom styling and formatting
- Automatic table of contents
- Code syntax highlighting

### Word Export
- Custom DOCX templates
- Header/footer support
- Predefined page templates
- Style preservation

## AI Features

### TOC Generation
- Automatic description generation for markdown files
- Batch processing support
- Configurable prompts
- GPU acceleration support

### Chat Assistant
- Local model support (no cloud required)
- Context-aware responses
- Project-specific knowledge
- Configurable model parameters

## Git Integration

- **Authentication**: SSH keys, Git credential helper, system credential store
- **Operations**: Commit, push, pull, fetch, branches, tags
- **Visual Diff**: See changes before committing
- **Remote Management**: Add, remove, and configure remotes
- **Credential Caching**: Secure credential storage

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md).

## Security

Please report security vulnerabilities to security@mdexplorer.net. See [SECURITY.md](SECURITY.md) for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/salaroglio/MdExplorer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/salaroglio/MdExplorer/discussions)
- **Website**: https://www.mdexplorer.net
- **Email**: developer@mdexplorer.net

## Acknowledgments

Built with amazing open-source libraries. See [librerie-licenze.md](librerie-licenze.md) for the complete list of dependencies and their licenses.

Special thanks to all contributors and the open-source community.

---

Made with ❤️ by Carlo Salaroglio and contributors
