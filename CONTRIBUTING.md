# Contributing to MdExplorer

Thank you for your interest in contributing to MdExplorer!

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
- [Development Setup](#development-setup)
- [Code Style Guidelines](#code-style-guidelines)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Pull Request Process](#pull-request-process)

## Code of Conduct

This project adheres to a Code of Conduct. By participating, you are expected to uphold this code. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

## How to Contribute

### Reporting Bugs

Use GitHub Issues with the bug report template. Please include:
- Operating system and version (Windows, macOS, Linux)
- .NET SDK version (`dotnet --version`)
- Node.js version (`node --version`)
- Steps to reproduce the issue
- Expected vs actual behavior
- Screenshots or error messages if applicable

### Suggesting Features

Use GitHub Issues with the feature request template. Please explain:
- The use case and problem your feature would solve
- Your proposed solution
- Alternative solutions you've considered
- Any additional context or examples

### Contributing Code

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes following our code style guidelines
4. Write or update tests if applicable
5. Update documentation as needed
6. Commit your changes using conventional commit messages
7. Push to your fork: `git push origin feature/amazing-feature`
8. Open a Pull Request with a clear description

## Development Setup

### Prerequisites

- **.NET SDK 8.0** or later
- **Node.js 14.21.3** (required for Angular 11 build - use nvm)
- **Git** for version control

### Clone and Setup

```bash
# Clone with submodules
git clone --recursive https://github.com/salaroglio/MdExplorer.git
cd MdExplorer

# Build backend
dotnet restore
dotnet build

# Build Angular frontend (IMPORTANT: Use Node 14.21.3)
cd MdExplorer/client2
nvm use 14.21.3  # or switch to Node 14.21.3 with your node manager
npm install
npm run build
cd ../..

# Run application in Debug mode (opens browser automatically)
dotnet run --project MdExplorer/MdExplorer.Service.csproj
```

**Note on Build Modes**:
- **Debug mode**: `dotnet run` - Opens browser automatically for development
- **Release mode**: `dotnet run --configuration Release` - No browser launch (used with Electron wrapper)

### Project Structure

```
MdExplorer/
├── MdExplorer/              # ASP.NET Core web service
├── MdExplorer.bll/          # Business logic & features
├── MdExplorer.Abstractions/ # Domain models & interfaces
├── MDExplorer.dal/          # Data access (legacy)
├── Ad.Tools.Dal.Evo/        # Modern data access layer
├── MdExplorer.Migrations/   # Database migrations
└── MdEditor.React/          # React-based Milkdown editor
```

**Note**: ElectronMdExplorer is a separate commercial product and not part of the open source project.

## Code Style Guidelines

### .NET / C#

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful, descriptive names for variables, methods, and classes
- Add XML documentation comments to public APIs
- Keep methods focused and concise
- Use async/await for I/O operations
- Handle exceptions appropriately

### TypeScript / Angular

- Follow [Angular Style Guide](https://angular.io/guide/styleguide)
- Use strict typing - avoid `any` where possible
- Prefer reactive patterns with RxJS
- Component files: `*.component.ts`, `*.component.html`, `*.component.scss`
- Service files: `*.service.ts`
- Use dependency injection for services

### Database Changes

**IMPORTANT**: Always use FluentMigrator for database changes. See [workflow-sviluppo-dati-persistenti.md](workflow-sviluppo-dati-persistenti.md) for complete workflow.

**Migration Naming Pattern**: `M{Year}_{Month}_{Day}_{Number}`

Example:
```csharp
[Migration(20251008001, "Add new feature table")]
public class M2025_10_08_001 : Migration
{
    public override void Up()
    {
        Create.Table("NewFeature")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Name").AsString(255).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("NewFeature");
    }
}
```

## Cross-Platform Compatibility

**CRITICAL**: MdExplorer must work on Windows, macOS, and Linux.

### Guidelines

- **File System Paths**: Always use `Path.Combine()` to construct file system paths
- **Path Separators**: Use `Path.DirectorySeparatorChar` when needed for file system operations
- **Markdown/HTML/Pandoc Paths**: Always use forward slash `/` in URLs and Markdown links
- **Platform-Specific Code**: Use `RuntimeInformation.IsOSPlatform()` to check platform
- **Testing**: Test on multiple platforms when possible

### Examples

```csharp
// ✅ GOOD - Cross-platform file paths
var path = Path.Combine(directory, "subfolder", "file.md");

// ❌ BAD - Windows-specific
var path = directory + "\\subfolder\\file.md";

// ✅ GOOD - Markdown links
var link = $"/{folderName}/{fileName}";

// ✅ GOOD - Platform check
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Windows-specific code
}
```

## Commit Message Guidelines

Follow [Conventional Commits](https://www.conventionalcommits.org/):

**Format**: `<type>: <description>`

**Types**:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `refactor:` Code refactoring (no functional changes)
- `test:` Adding or updating tests
- `chore:` Maintenance tasks, dependencies
- `perf:` Performance improvements
- `style:` Code style changes (formatting, missing semi-colons, etc.)
- `ci:` CI/CD configuration changes
- `build:` Build system or external dependencies

**Examples**:
```
feat: add export to markdown functionality
fix: resolve crash when opening large files
docs: update README with new setup instructions
refactor: extract file utility functions
test: add unit tests for markdown parser
chore: update Angular dependencies to v11.2.14
```

## Pull Request Process

### Before Submitting

1. **Update your branch**: Rebase on latest main
   ```bash
   git fetch origin
   git rebase origin/main
   ```

2. **Run tests**: Ensure all tests pass
   ```bash
   dotnet test
   cd MdExplorer/client2 && npm test
   ```

3. **Build check**: Verify clean build
   ```bash
   dotnet build --configuration Release
   ```

4. **Update documentation**: If your changes affect user-facing features or APIs

### Pull Request Template

When opening a PR, please include:

- **Description**: Clear description of what changes were made and why
- **Type of Change**: Bug fix / New feature / Breaking change / Documentation
- **Testing**: How you tested your changes
- **Screenshots**: For UI changes
- **Related Issues**: Link to related issues with "Closes #123"

### Review Process

- At least one maintainer will review your PR
- Address any feedback or requested changes
- Once approved, a maintainer will merge your PR
- Your contribution will be included in the next release!

## Development Workflow

### Data Access Layer

The project is migrating from `Ad.Tools.Dal` to `Ad.Tools.Dal.Evo` using the Strangler Fig Pattern:

- **New features**: Use `IUnitOfWork` and `IRepository<T>` from `Ad.Tools.Dal.Evo.Abstractions`
- **Existing code**: May still use the old DAL interfaces
- See [Ad.Tools.Dal/EvoluzioneDal.md](Ad.Tools.Dal/EvoluzioneDal.md) for migration details

### Working with Migrations

See [workflow-sviluppo-dati-persistenti.md](workflow-sviluppo-dati-persistenti.md) for complete guidance on:
- Adding new entities
- Modifying existing entities
- Creating mappings with NHibernate
- Working with Angular services and components

## Questions?

- Open a [GitHub Discussion](https://github.com/salaroglio/MdExplorer/discussions)
- Comment on a related issue
- Check existing documentation in the repository

Thank you for contributing to MdExplorer! 🎉
