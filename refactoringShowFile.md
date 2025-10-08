# Refactoring Show-File-System Component

## Obiettivo
Trasformare il componente `show-file-system.component` da un albero espandibile a una UI a due colonne simile a Windows Explorer:
- **Colonna sinistra**: Icone per navigazione rapida (C:, Desktop, Documents, Downloads)
- **Colonna destra**: Visualizzazione del contenuto della cartella selezionata

## Analisi Stato Attuale

### Componente Angular (`show-file-system.component.ts`)

#### Caratteristiche attuali:
1. **Tree view espandibile** con Angular CDK Tree
2. **Caricamento dinamico** dei contenuti delle cartelle
3. **Menu contestuale** per creare nuove cartelle
4. **Selezione cartella** con dialog per conferma

#### Servizi e dipendenze:
- `MdFileService`: Gestisce le chiamate API per il filesystem
- `DynamicDatabase`: Gestisce lo stato locale dell'albero
- `DynamicDataSource`: Data source per il tree component
- `MatDialog`: Per i dialoghi di creazione cartelle

### API Backend (`MdFilesController.cs`)

#### Endpoint esistenti:
1. **`GetDynFoldersDocument`** (GET)
   - Parametri: `path`, `level`
   - Restituisce: Solo cartelle
   - Path speciali supportati: `"root"` → C:\, `"project"` → project path, `"documents"` → MyDocuments

2. **`GetDynFoldersAndFilesDocument`** (GET)
   - Parametri: `path`, `level`
   - Restituisce: Cartelle e file
   - Stessi path speciali di sopra

3. **`CreateNewDirectory`** (POST)
   - Crea nuove cartelle nel filesystem

4. **`OpenFolderOnFileExplorer`** (POST)
   - Apre una cartella in Windows Explorer

#### API mancanti:
- **GetSpecialFolders**: Per ottenere Desktop, Downloads, etc.
- **GetDrives**: Per ottenere la lista dei drive (C:, D:, etc.)

## Piano di Refactoring

### Fase 1: Preparazione Backend
**Task 1.1**: Creare nuovo endpoint per special folders
```csharp
[HttpGet]
public IActionResult GetSpecialFolders()
{
    var folders = new[]
    {
        new { 
            name = "Desktop", 
            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            icon = "desktop_windows"
        },
        new { 
            name = "Documents", 
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            icon = "folder_shared"
        },
        new { 
            name = "Downloads", 
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            icon = "cloud_download"
        }
    };
    return Ok(folders);
}
```

**Task 1.2**: Creare endpoint per drive list
```csharp
[HttpGet]
public IActionResult GetDrives()
{
    var drives = DriveInfo.GetDrives()
        .Where(d => d.IsReady)
        .Select(d => new {
            name = d.Name,
            path = d.RootDirectory.FullName,
            icon = "storage",
            label = d.VolumeLabel,
            totalSize = d.TotalSize,
            freeSpace = d.AvailableFreeSpace
        });
    return Ok(drives);
}
```

### Fase 2: Servizio Angular
**Task 2.1**: Estendere `MdFileService` con nuovi metodi
```typescript
getSpecialFolders(): Observable<SpecialFolder[]> {
    return this.http.get<SpecialFolder[]>('../api/mdfiles/GetSpecialFolders');
}

getDrives(): Observable<Drive[]> {
    return this.http.get<Drive[]>('../api/mdfiles/GetDrives');
}
```

**Task 2.2**: Creare interfacce TypeScript
```typescript
interface SpecialFolder {
    name: string;
    path: string;
    icon: string;
}

interface Drive {
    name: string;
    path: string;
    icon: string;
    label: string;
    totalSize: number;
    freeSpace: number;
}
```

### Fase 3: Nuovo Componente UI
**Task 3.1**: Creare struttura HTML a due colonne
```html
<div class="file-explorer-container">
  <div class="left-panel">
    <!-- Quick access section -->
    <div class="quick-access-section">
      <h3>Quick Access</h3>
      <div class="quick-access-item" *ngFor="let folder of specialFolders" 
           (click)="navigateToFolder(folder.path)">
        <mat-icon>{{folder.icon}}</mat-icon>
        <span>{{folder.name}}</span>
      </div>
    </div>
    
    <!-- Drives section -->
    <div class="drives-section">
      <h3>This PC</h3>
      <div class="drive-item" *ngFor="let drive of drives"
           (click)="navigateToFolder(drive.path)">
        <mat-icon>{{drive.icon}}</mat-icon>
        <span>{{drive.name}}</span>
      </div>
    </div>
  </div>
  
  <div class="right-panel">
    <!-- Breadcrumb -->
    <div class="breadcrumb">
      <button mat-icon-button (click)="navigateUp()">
        <mat-icon>arrow_back</mat-icon>
      </button>
      <span>{{currentPath}}</span>
    </div>
    
    <!-- File/Folder list -->
    <div class="content-list">
      <div class="content-item" *ngFor="let item of currentItems"
           (click)="onItemClick(item)"
           (contextmenu)="onRightClick($event, item)">
        <mat-icon>{{item.type === 'folder' ? 'folder' : 'insert_drive_file'}}</mat-icon>
        <span>{{item.name}}</span>
      </div>
    </div>
  </div>
</div>
```

**Task 3.2**: Implementare logica componente
```typescript
export class ShowFileSystemComponent implements OnInit {
  specialFolders: SpecialFolder[] = [];
  drives: Drive[] = [];
  currentPath: string = '';
  currentItems: MdFile[] = [];
  
  ngOnInit() {
    // Load special folders and drives
    forkJoin({
      folders: this.mdFileService.getSpecialFolders(),
      drives: this.mdFileService.getDrives()
    }).subscribe(({folders, drives}) => {
      this.specialFolders = folders;
      this.drives = drives;
    });
  }
  
  navigateToFolder(path: string) {
    this.currentPath = path;
    this.loadFolderContent(path);
  }
  
  loadFolderContent(path: string) {
    this.mdFileService.loadDocumentFolder(path, 0, this.baseStart.typeOfSelection)
      .subscribe(items => {
        this.currentItems = items;
      });
  }
  
  onItemClick(item: MdFile) {
    if (item.type === 'folder') {
      this.navigateToFolder(item.path);
    } else {
      this.getFolder(item);
    }
  }
}
```

### Fase 4: Styling
**Task 4.1**: Creare CSS per layout a due colonne
```scss
.file-explorer-container {
  display: flex;
  height: 100%;
  
  .left-panel {
    width: 250px;
    background: #f5f5f5;
    border-right: 1px solid #ddd;
    padding: 16px;
    
    .quick-access-item, .drive-item {
      display: flex;
      align-items: center;
      padding: 8px;
      cursor: pointer;
      
      &:hover {
        background: #e0e0e0;
      }
      
      mat-icon {
        margin-right: 8px;
      }
    }
  }
  
  .right-panel {
    flex: 1;
    display: flex;
    flex-direction: column;
    
    .breadcrumb {
      padding: 8px 16px;
      border-bottom: 1px solid #ddd;
      display: flex;
      align-items: center;
    }
    
    .content-list {
      flex: 1;
      overflow-y: auto;
      padding: 16px;
      
      .content-item {
        display: flex;
        align-items: center;
        padding: 8px;
        cursor: pointer;
        
        &:hover {
          background: #f0f0f0;
        }
      }
    }
  }
}
```

### Fase 5: Gestione navigazione
**Task 5.1**: Implementare breadcrumb navigation
```typescript
navigationHistory: string[] = [];

navigateToFolder(path: string) {
  this.navigationHistory.push(this.currentPath);
  this.currentPath = path;
  this.loadFolderContent(path);
}

navigateUp() {
  if (this.navigationHistory.length > 0) {
    const previousPath = this.navigationHistory.pop();
    this.currentPath = previousPath;
    this.loadFolderContent(previousPath);
  }
}
```

**Task 5.2**: Mantenere funzionalità esistenti
- Menu contestuale per creare cartelle
- Selezione file/cartella per dialog
- Gestione errori per cartelle inaccessibili

### Fase 6: Testing e ottimizzazione
**Task 6.1**: Test delle nuove API
- Verificare accesso a special folders su diversi OS
- Gestire errori per drive non accessibili

**Task 6.2**: Ottimizzare performance
- Implementare caching per cartelle già visitate
- Lazy loading per contenuti di grandi cartelle

**Task 6.3**: Accessibilità
- Aggiungere supporto keyboard navigation
- ARIA labels per screen readers

## Riepilogo Task per Sonnet 4

1. **Backend Tasks** (2 task):
   - Task 1.1: Implementare GetSpecialFolders API
   - Task 1.2: Implementare GetDrives API

2. **Service Tasks** (2 task):
   - Task 2.1: Estendere MdFileService
   - Task 2.2: Creare interfacce TypeScript

3. **UI Component Tasks** (2 task):
   - Task 3.1: Creare HTML a due colonne
   - Task 3.2: Implementare logica navigazione

4. **Styling Task** (1 task):
   - Task 4.1: Implementare CSS responsive

5. **Navigation Tasks** (2 task):
   - Task 5.1: Breadcrumb navigation
   - Task 5.2: Preservare funzionalità esistenti

6. **Testing Tasks** (3 task):
   - Task 6.1: Test API cross-platform
   - Task 6.2: Ottimizzazione performance
   - Task 6.3: Accessibilità

Totale: **12 task modulari** facilmente eseguibili da Sonnet 4.