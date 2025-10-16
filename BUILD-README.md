# MdExplorer - Build Instructions

## Quick Start

### Development

**Terminal 1 - Angular Dev Server:**
```bash
cd MdExplorer/client2
nvm use 14.21.3
npm install
npm start
# http://localhost:4200 with hot reload
```

**Terminal 2 - .NET Backend:**
```bash
cd MdExplorer
dotnet watch run
```

### Production Build

**Build Angular:**
```bash
./build-angular.sh    # Linux/macOS
# or
.\build-angular.ps1   # Windows PowerShell
```

**Build .NET:**
```bash
cd MdExplorer
dotnet build --configuration Release
```

**Run:**
```bash
cd MdExplorer
dotnet run --no-build --configuration Release
```

## Build Scripts

### `build-angular.sh` / `.ps1`

Simple script that builds the Angular frontend.

**What it does:**
1. Switches to Node.js 14.21.3 (if nvm available)
2. Installs npm dependencies
3. Builds Angular application
4. Output goes to `MdExplorer/wwwroot/client2/`

**Usage:**
```bash
# Linux/macOS
./build-angular.sh

# Windows PowerShell
.\build-angular.ps1
```

## Requirements

### Node.js

**IMPORTANT**: Angular 11 requires Node.js 14.21.3

Install with nvm:
```bash
# Install nvm first (if not already installed)
# Then:
nvm install 14.21.3
nvm use 14.21.3
```

### .NET

- .NET 8.0 SDK or later
- Compatible with Windows, macOS, and Linux

## Project Structure

```
MdExplorer/
├── client2/              # Angular 11 frontend
│   ├── src/
│   ├── package.json
│   └── angular.json
├── wwwroot/
│   └── client2/         # Built Angular assets (output)
├── Controllers/
├── Services/
└── MdExplorer.Service.csproj

MdExplorer.Features/      # Business logic
MdExplorer.Abstractions/  # Interfaces and entities
```

## Common Tasks

### Clean Build

```bash
# Clean Angular
cd MdExplorer/client2
rm -rf node_modules dist
npm install
npm run build

# Clean .NET
cd ..
dotnet clean
dotnet restore
dotnet build --configuration Release
```

### Run Tests

```bash
# Angular tests
cd MdExplorer/client2
npm test

# .NET tests
dotnet test
```

### Troubleshooting

#### Error: "npm: command not found"
**Fix**: Install Node.js 14.21.3 with nvm (see Requirements above)

#### Error: Angular compilation fails
**Fix**: Ensure you're using Node.js 14.21.3
```bash
node --version  # Should show v14.21.3
nvm use 14.21.3
```

#### Error: "Permission denied" (Linux/macOS)
**Fix**: Make script executable
```bash
chmod +x build-angular.sh
```

#### Error: PowerShell Execution Policy (Windows)
**Fix**: Enable script execution
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Architecture

MdExplorer is a multi-project .NET solution for Markdown file management:

- **ASP.NET Core 8.0** web API backend
- **Angular 11** frontend with Material Design
- **SQLite** databases (NHibernate ORM)
- **SignalR** for real-time file monitoring
- **Pandoc** for document export (PDF/Word)
- **PlantUML** for diagram rendering

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Build and test locally
5. Submit a pull request

## License

MIT License - See LICENSE file for details

## Support

- GitHub Issues: https://github.com/your-org/mdexplorer/issues
- Documentation: See docs/ folder
- Email: developer@mdexplorer.net
