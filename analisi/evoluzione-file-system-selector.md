# Evoluzione File System Selector

# Piano di Implementazione

## Panoramica

Questo documento descrive l'evoluzione del componente `ShowFileSystemComponent` per gestire meglio i diversi contesti di selezione (solo cartelle vs file e cartelle) con un approccio graduale da Opzione 1 a Opzione 3.

## Contesto Attuale

Il componente viene utilizzato in 4 punti diversi:

1. **ProjectsComponent** - Selezione cartella per nuovo progetto (`typeOfSelection: "Folders"`)
2. **CloneProjectComponent** - Selezione cartella per clone Git (`typeOfSelection: "Folders"`)
3. **MoveMdFileComponent** - Selezione cartella destinazione (`typeOfSelection: "Folders"`)
4. **AddNewFileToMDEComponent** - Selezione file esistente (`typeOfSelection: "FoldersAndFiles"`)

## Step 1: Implementazione Base (Opzione 1)

### Task 1.1: Adattare il testo del pulsante

**File**: `show-file-system.component.html`

* Modificare il pulsante "Select directory" per mostrare testo dinamico

```HTML
<!-- Da -->
<button mat-stroked-button color="primary" 
        (click)="closeDialog()" 
        [disabled]="!folder.path">
  Select directory
</button>

<!-- A -->
<button mat-stroked-button color="primary" 
        (click)="closeDialog()" 
        [disabled]="!folder.path">
  {{getSelectionButtonText()}}
</button>
```

### Task 1.2: Implementare logica selezione differenziata

**File**: `show-file-system.component.ts`

* Aggiungere metodo per testo pulsante dinamico
* Modificare validazione selezione basata su tipo

```TypeScript
// Nuovo metodo
public getSelectionButtonText(): string {
  return this.baseStart.typeOfSelection === 'FoldersAndFiles' 
    ? 'Select file' 
    : 'Select folder';
}

// Modificare onItemClick per validare selezione
public onItemClick(item: MdFile): void {
  if (this.baseStart.typeOfSelection === 'FoldersAndFiles' && item.type === 'folder') {
    // Per selezione file, le cartelle servono solo per navigare
    this.navigateToFolder(item.fullPath || item.path);
  } else {
    // Selezione normale
    this.activeNode = item;
    this.getFolder(item);
  }
}

// Modificare validazione pulsante
public canSelectItem(): boolean {
  if (!this.folder.path) return false;
  
  if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
    // Solo file possono essere selezionati
    return this.activeNode && this.activeNode.type !== 'folder';
  }
  
  // Solo cartelle possono essere selezionate
  return true;
}
```

### Task 1.3: Aggiornare template per validazione

**File**: `show-file-system.component.html`

```HTML
<!-- Modificare il pulsante con nuova validazione -->
<button mat-stroked-button color="primary" 
        (click)="closeDialog()" 
        [disabled]="!canSelectItem()">
  {{getSelectionButtonText()}}
</button>
```

### Task 1.4: Migliorare feedback visivo

**File**: `show-file-system.component.scss`

```SCSS
// Aggiungere stili per evidenziare elementi non selezionabili
.content-item {
  &.not-selectable {
    opacity: 0.6;
    cursor: default;
    
    &:hover {
      background: transparent;
    }
  }
}
```

**File**: `show-file-system.component.html`

```HTML
<!-- Aggiungere classe condizionale -->
<div class="content-item" 
     [class.not-selectable]="!isItemSelectable(item)"
     ...>
```

**File**: `show-file-system.component.ts`

```TypeScript
public isItemSelectable(item: MdFile): boolean {
  if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
    return item.type !== 'folder';
  }
  return item.type === 'folder';
}
```

## Step 2: Estensione ShowFileMetadata

### Task 2.1: Estendere l'interfaccia

**File**: `show-file-metadata.ts`

```TypeScript
export class ShowFileMetadata {
  constructor() {}
  public start: string;
  public title: string;
  public typeOfSelection: string;
  
  // Nuove proprietà opzionali
  public buttonText?: string;
  public fileExtensions?: string[];
  public showFileDetails?: boolean;
  public allowMultipleSelection?: boolean;
  public placeholder?: string;
}
```

### Task 2.2: Utilizzare buttonText personalizzato

**File**: `show-file-system.component.ts`

```TypeScript
public getSelectionButtonText(): string {
  // Prima controlla se c'è un testo personalizzato
  if (this.baseStart.buttonText) {
    return this.baseStart.buttonText;
  }
  
  // Altrimenti usa il default basato sul tipo
  return this.baseStart.typeOfSelection === 'FoldersAndFiles' 
    ? 'Select file' 
    : 'Select folder';
}
```

### Task 2.3: Implementare filtro per estensioni file

**File**: `show-file-system.component.ts`

```TypeScript
private applyFilter(): void {
  let filtered = [...this.currentItems];
  
  // Filtro per nome
  if (this.searchFilter && this.searchFilter.trim() !== '') {
    const filter = this.searchFilter.toLowerCase().trim();
    filtered = filtered.filter(item => 
      item.name.toLowerCase().includes(filter)
    );
  }
  
  // Filtro per estensioni (solo se specificato e per selezione file)
  if (this.baseStart.typeOfSelection === 'FoldersAndFiles' 
      && this.baseStart.fileExtensions 
      && this.baseStart.fileExtensions.length > 0) {
    filtered = filtered.filter(item => {
      if (item.type === 'folder') return true; // Sempre mostra cartelle per navigazione
      
      const extension = this.getFileExtension(item.name);
      return this.baseStart.fileExtensions.includes(extension);
    });
  }
  
  this.filteredItems = filtered;
}

private getFileExtension(filename: string): string {
  const lastDot = filename.lastIndexOf('.');
  return lastDot > 0 ? filename.substring(lastDot) : '';
}
```

## Step 3: Dettagli file e selezione multipla

### Task 3.1: Mostrare dettagli file

**File**: `show-file-system.component.html`

```HTML
<!-- Modificare content-item per mostrare dettagli -->
<div class="content-item" ...>
  <mat-icon class="item-icon" aria-hidden="true">
    {{getItemIcon(item)}}
  </mat-icon>
  <span class="item-name">{{item.name}}</span>
  
  <!-- Dettagli file opzionali -->
  <span class="item-details" *ngIf="showFileDetails && item.type !== 'folder'">
    <span class="file-size" *ngIf="item.size">{{formatFileSize(item.size)}}</span>
    <span class="file-date" *ngIf="item.lastModified">{{item.lastModified | date:'short'}}</span>
  </span>
</div>
```

**File**: `show-file-system.component.scss`

```SCSS
.item-details {
  margin-left: auto;
  display: flex;
  gap: 16px;
  font-size: 0.85rem;
  color: #666;
  
  .file-size {
    min-width: 80px;
    text-align: right;
  }
}
```

### Task 3.2: Implementare selezione multipla

**File**: `show-file-system.component.ts`

```TypeScript
// Nuove proprietà
selectedItems: Set<string> = new Set();

public onItemClick(item: MdFile, event?: MouseEvent): void {
  const isMultiSelect = this.baseStart.allowMultipleSelection && 
                       (event?.ctrlKey || event?.metaKey);
  
  if (this.baseStart.typeOfSelection === 'FoldersAndFiles' && item.type === 'folder') {
    this.navigateToFolder(item.fullPath || item.path);
    return;
  }
  
  if (isMultiSelect) {
    // Toggle selezione
    const key = item.fullPath || item.path;
    if (this.selectedItems.has(key)) {
      this.selectedItems.delete(key);
    } else {
      this.selectedItems.add(key);
    }
  } else {
    // Selezione singola
    this.selectedItems.clear();
    this.selectedItems.add(item.fullPath || item.path);
    this.activeNode = item;
    this.getFolder(item);
  }
}

public isItemSelected(item: MdFile): boolean {
  return this.selectedItems.has(item.fullPath || item.path);
}
```

## Step 4: Aggiornare i chiamanti

### Task 4.1: ProjectsComponent con testo personalizzato

**File**: `projects.component.ts`

```TypeScript
openNewFolder(): void {
  let data = new ShowFileMetadata();
  data.start = null;
  data.title = "Select project folder";
  data.typeOfSelection = "Folders";
  data.buttonText = "Create project here"; // Testo personalizzato
}
```

### Task 4.2: AddNewFileToMDEComponent con filtro estensioni

**File**: `add-new-file-to-mde.component.ts`

```TypeScript
openFileSystem() {
  let data = new ShowFileMetadata();
  data.title = "Select file to add";
  data.typeOfSelection = "FoldersAndFiles";
  data.buttonText = "Add to project";
  data.fileExtensions = ['.md', '.txt', '.doc', '.docx', '.pdf']; // Solo questi file
  data.showFileDetails = true; // Mostra dimensione e data
}
```

## Step 5: Testing e Validazione

### Task 5.1: Verificare tutti i contesti

1. Testare selezione cartella per nuovo progetto
2. Testare selezione cartella per clone
3. Testare selezione cartella per spostamento
4. Testare selezione file con filtri

### Task 5.2: Verificare retrocompatibilità

* Assicurarsi che i dialog esistenti funzionino senza modifiche
* Verificare che i parametri opzionali non causino errori

## Note Implementative

1. **Ordine di implementazione**: Seguire gli step in ordine per garantire stabilità
2. **Retrocompatibilità**: Tutti i nuovi parametri sono opzionali
3. **Performance**: Il filtro per estensioni deve essere efficiente
4. **UX**: Fornire feedback visivo chiaro su cosa può essere selezionato
5. **Accessibilità**: Mantenere supporto ARIA per screen reader

## Risultato Atteso

Al termine dell'implementazione, il componente sarà:

* **Flessibile**: Adattabile a diversi contesti
* **Intuitivo**: L'utente capisce cosa può selezionare
* **Performante**: Filtri efficienti anche con molti file
* **Retrocompatibile**: Non rompe il codice esistente
* **Estendibile**: Facile aggiungere nuove funzionalità

<br />
