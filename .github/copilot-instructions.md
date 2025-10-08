# mdExplorer Project - Copilot Instructions

## Project Overview
mdExplorer is a comprehensive markdown documentation explorer and editor built with a multi-layer architecture. The project enables advanced markdown document management, editing, and publishing with version control integration.

## Architecture Components

### 1. Backend API Server (.NET Core)
- **Main Project**: `MdExplorer.Service` (ASP.NET Core 3.1)
- **Framework**: .NET Core 3.1 with C#
- **Architecture**: Web API serving both Angular and React frontends
- **Key Features**:
  - RESTful API endpoints (`/api/MdProjects/*`)
  - SignalR real-time communication
  - File system monitoring and management
  - Markdown processing with Markdig
  - PDF/Word export capabilities
  - Git integration

### 2. Angular Frontend
- **Location**: `MdExplorer/client2/`
- **Framework**: Angular 11 with Material Design
- **Key Dependencies**:
  - Angular Material UI components
  - SignalR client (`@microsoft/signalr`)
  - RxJS for reactive programming
- **Main Components**:
  - Project management interface
  - File tree navigation
  - Document viewer/editor
  - Settings and configuration dialogs
  - Git integration UI

### 3. React Editor
- **Location**: `MdEditor.React/`
- **Framework**: React 19 with Vite build system
- **Key Dependencies**:
  - Milkdown editor (`@milkdown/core`, `@milkdown/crepe`)
  - Styled Components for styling
  - TypeScript support
- **Purpose**: Advanced markdown editing with rich text capabilities

### 4. Electron Desktop Application
- **Location**: `ElectronMdExplorer/`
- **Framework**: Electron (v28.1.4)
- **Purpose**: Desktop wrapper providing native OS integration
- **Key Features**:
  - Hosts the Angular application
  - Communicates with local API server
  - Cross-platform desktop experience
  - Native file system access

## Data Layer Architecture

### Current Migration (Strangler Fig Pattern)
- **Legacy**: `Ad.Tools.Dal` (being phased out)
- **New**: `Ad.Tools.Dal.Evo` (target architecture)
- **ORM**: NHibernate with FluentNHibernate mappings
- **Migration Tool**: FluentMigrator for schema changes

### Database Structure
- **SQLite Databases**:
  - `MdExplorer.db` - User settings and preferences
  - `MdEngine.db` - Engine configuration and metadata
  - Project-specific databases for document management

### Data Access Projects
- `MdExplorer.DataAccess.Engine` - Engine data operations
- `MdExplorer.DataAccess.Project` - Project-specific data
- `MDExplorer.DataAccess` - Legacy data access layer
- `Ad.Tools.Dal.Evo` - New data access framework
- `Ad.Tools.Dal.Evo.Abstractions` - Interfaces and contracts

## Key Business Logic

### Core Features (`MdExplorer.Features`)
- Document processing and transformation
- Export functionality (PDF, Word, HTML)
- File system operations and monitoring
- Git version control integration
- Project management and organization
- Real-time collaboration via SignalR

### Migration Management
- `MdExplorer.Migrations` - Core database migrations
- `MdExplorer.Migrations.EngineDb` - Engine database schema
- `MdExplorer.Migrations.ProjectDb` - Project database schema

## Development Workflow

### Backend Development
```powershell
# Build entire solution
dotnet build

# Run the main API service
dotnet run --project MdExplorer/MdExplorer.Service.csproj

# Run specific tests
dotnet test MdExplorer.Features.Tests/MdExplorer.Features.Tests.csproj
```

### Frontend Development
```powershell
# Angular Client
cd MdExplorer/client2
npm install
npm run build    # Production build with base href
npm start        # Development server

# React Editor
cd MdEditor.React
npm install
npm run build    # Production build with Vite
npm run dev      # Development server
```

### Desktop Application
```powershell
cd ElectronMdExplorer
npm install
npm start        # Run Electron app
npm run build    # Build distributables
```

## Technology Stack Summary

### Backend Technologies
- **.NET Core 3.1** - Web API framework
- **NHibernate** - ORM for data access
- **FluentMigrator** - Database migrations
- **SQLite** - Database engine
- **Markdig** - Markdown processing
- **SignalR** - Real-time communication
- **AutoMapper** - Object mapping

### Frontend Technologies
- **Angular 11** - Main UI framework
- **Angular Material** - UI component library
- **React 19** - Editor framework
- **Milkdown** - Rich markdown editor
- **TypeScript** - Type-safe JavaScript
- **RxJS** - Reactive programming
- **Vite** - Build tool for React

### Desktop & Infrastructure
- **Electron** - Desktop application framework
- **Node.js** - JavaScript runtime
- **Git** - Version control integration
- **WiX Toolset** - Windows installer (`MdExplorer.Setup`)

## Project Structure Highlights

### Main Solution Projects
- `MdExplorer.Service` - Main API server
- `MdExplorer.Features` - Business logic layer
- `MdExplorer.Abstractions` - Shared interfaces and models
- `MdExplorer.Features.Tests` - Unit tests

### Development Tools
- `MdImageNumbering` - Internal tooling for image management
- Various analysis and refactoring documentation in `/analisi/`
- PowerShell scripts for credential and system analysis

## Key Integration Points

### API-Frontend Communication
- RESTful endpoints for CRUD operations
- SignalR hubs for real-time updates
- File system watching for automatic updates

### Electron-Web Integration
- Electron hosts Angular application
- IPC communication for native features
- Shared configuration between web and desktop

### Editor Integration
- React editor embedded in Angular application
- Seamless switching between view and edit modes
- Consistent styling and theming

## Development Guidelines

### Code Organization
- Follow established folder structure
- Maintain separation of concerns
- Use dependency injection throughout
- Implement proper error handling

### Data Access Patterns
- Prefer new `Ad.Tools.Dal.Evo` for new features
- Use repository pattern for data operations
- Implement unit of work for transactions
- Apply migrations for schema changes

### Frontend Best Practices
- Use Angular Material components consistently
- Implement reactive patterns with RxJS
- Follow TypeScript strict mode guidelines
- Maintain component hierarchy and services

This project represents a sophisticated document management ecosystem with modern web technologies, desktop integration, and robust data management capabilities.