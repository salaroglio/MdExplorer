---
author: Carlo Salaroglio
document_type: Document
email: developer@mdexplorer.net
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

## Skills Available

This project uses specialized skills for different development tasks. Use the appropriate skill based on your task:

* **developer** - General development, architecture, patterns, and implementation
* **bug-fixing** - Debugging, troubleshooting, and fixing issues
* **document-writer** - Writing sprint and technical documentation
* **solution-architect** - Architectural design, strategic technical decisions, system design, and pattern evaluation

## Understanding "Projects" in MdExplorer

### What is a Project?

A **Project** in MdExplorer is a folder on the file system that contains markdown files and is being actively managed and indexed by the application.

**Three Perspectives:**
- **File System**: A project is a directory path on disk containing `.md` files
- **Database**: A project is a record in the `Project` table within the SQLite user settings database
- **Application**: A project is the "current working context" - it defines what files the user can browse, edit, and work with

### Project Data Structure

**Frontend Model** (`client2/src/app/md-explorer/models/md-project.ts`):
```typescript
export class MdProject {
  id: string
  name: string
  path: string
  sidenavWidth: number
}
```

**Backend Entity** (`MdExplorer.Abstractions/Entities/UserDB/Project.cs`):
```csharp
public class Project {
    public virtual Guid Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Path { get; set; }
    public virtual DateTime LastUpdate { get; set; }
    public virtual IList<Bookmark> Bookmarks { get; set; }
    public virtual int? SidenavWidth { get; set; }
}
```

### Three-Level Database Structure

When a project is opened, **THREE separate SQLite databases** are initialized:

1. **User Settings Database** (Global)
   - Location: `%AppData%/MdExplorer.db`
   - Purpose: Stores projects list, user preferences, bookmarks, TOC cache
   - Tables: `Project`, `Bookmark`, etc.

2. **Engine Database** (Per Project Hash)
   - Location: `%AppData%/MdEngine_{hash}.db`
   - Purpose: Stores markdown file index and link relationships
   - Hash: Generated from project path using `Helper.HGetHashString()`
   - Tables: `MarkdownFile`, `LinkInsideMarkdown`

3. **Project Database** (Per Project)
   - Location: `{ProjectPath}/.md/MdProject_{hash}.db`
   - Purpose: Project-specific metadata and settings
   - Tables: `ProjectSetting`, `ProjectFileInfoNode`, etc.

### Opening a Project - Critical Flow

**What happens when a user opens a project:**

1. **Frontend**: User selects folder → `ProjectsService.createProjectWithConfig()`
2. **Backend**: `MdProjectsController.SetFolderProject()` is called:
   - Disables FileSystemWatcher temporarily
   - Updates FileSystemWatcher path to new project
   - Creates/updates Project record in database
   - Calls `ProjectsManager.SetNewProject()` to initialize structure
   - Re-enables FileSystemWatcher
3. **Frontend**: Navigates to `/main/navigation/document`
4. **File Loading**: Calls `GET /api/mdfiles/GetShallowStructure`:
   - Cleans up database (deletes old `LinkInsideMarkdown` and `MarkdownFile` records)
   - Indexes all `.md` files recursively → creates `MarkdownFile` records
   - Builds shallow folder structure (1st level only)
   - **Starts background task** `IndexLinksInBackground()` to parse all files for links
5. **Background Indexing**: Each file is parsed for:
   - Links to other markdown files
   - PlantUML diagrams
   - YAML front matter
   - Cross-file references

### Project Folder Structure

When a project is initialized, this structure is created:

```
ProjectRoot/
├── .md/
│   ├── templates/
│   │   ├── pdf/
│   │   │   └── eisvogel.tex
│   │   └── word/
│   │       ├── reference.docx
│   │       ├── minute.docx
│   │       └── pages/
│   ├── EmojiForPandoc/
│   └── MdProject_{hash}.db
├── .mdapplicationtoopen
├── .mdchangeignore
├── .mdFoldersIgnore
├── .github/
│   └── copilot-instructions.md (optional)
└── .git/ (optional)
```

### Key Files for Project Management

**Frontend:**
- `client2/src/app/md-explorer/services/projects.service.ts` - Project operations
- `client2/src/app/projects/projects.component.ts` - UI for project management
- `client2/src/app/md-explorer/models/md-project.ts` - Project model

**Backend:**
- `Controllers/MdProjects/MdProjectsController.cs` - Project API endpoints
- `ProjectsManager.cs` - Project initialization logic
- `MdExplorer.Abstractions/Entities/UserDB/Project.cs` - Project entity
- `Controllers/MdFiles/MdFilesController.cs` - File indexing endpoints (`GetShallowStructure`)

### Important API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/MdProjects/GetProjects` | GET | Fetch all projects list |
| `/api/MdProjects/SetFolderProject` | POST | Open/Create project |
| `/api/MdProjects/DeleteProject` | POST | Delete project from list |
| `/api/mdfiles/GetShallowStructure` | GET | Fetch file tree and trigger indexing |

### Relationship: Projects vs Files

- **One-to-Many**: One project contains many markdown files
- **Project is a Scope**: Not a database collection, but a file system boundary for indexing, bookmarks, search, and Git operations
- **Link Tracking**: `LinkInsideMarkdown` table tracks relationships between files within a project

<br />