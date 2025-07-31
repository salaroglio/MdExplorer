---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 07/07/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Implementazione Pattern Memento + Observer per Tree State Management

## Panoramica

### Problema da Risolvere
Quando un file viene rinominato tramite la Rule #1 o altre operazioni, l'albero dei file perde lo stato di espansione dei nodi. Questo costringe l'utente a ri-espandere manualmente l'albero per tornare alla posizione di lavoro.

### Soluzione Proposta
Implementare una combinazione dei pattern **Memento** e **Observer** per:
- **Memento Pattern**: Salvare e ripristinare lo stato dell'albero (nodi espansi, selezione corrente)
- **Observer Pattern**: Reagire automaticamente agli eventi di modifica file (rinominamento, creazione, cancellazione)
- **Facade Pattern**: Fornire un'interfaccia semplificata per la gestione dello stato

## Architettura

```
┌─────────────────────┐
│   md-tree.component │
└──────────┬──────────┘
           │ usa
           ▼
┌─────────────────────┐     osserva      ┌──────────────────┐
│ TreeStateManager    │◄─────────────────│ MdFileService    │
│    (Facade)         │                   │                  │
└──────────┬──────────┘                   └──────────────────┘
           │ delega
           ▼
┌─────────────────────┐     salva        ┌──────────────────┐
│ TreeStateService    │─────────────────►│ TreeStateMemento │
│    (Caretaker)      │                   │    (Memento)     │
└─────────────────────┘                   └──────────────────┘
```

## Nuovi File da Creare

### 1. `/src/app/md-explorer/models/tree-state-memento.interface.ts`

```typescript
/**
 * Interfaccia Memento che rappresenta uno snapshot dello stato dell'albero
 */
export interface TreeStateMemento {
  /** Percorsi completi dei nodi espansi */
  expandedNodes: string[];
  
  /** Percorso del nodo selezionato (se presente) */
  selectedNodePath: string | null;
  
  /** Percorso del nodo attivo (se presente) */
  activeNodePath: string | null;
  
  /** Posizione di scroll del contenitore dell'albero */
  scrollPosition: number;
  
  /** Timestamp della creazione del memento */
  timestamp: Date;
  
  /** Chiave opzionale per identificare il tipo di operazione */
  operationType?: 'rename' | 'create' | 'delete' | 'refresh';
}

/**
 * Mappa per tracciare i rinominamenti di percorsi
 */
export interface PathRenameMap {
  oldPath: string;
  newPath: string;
}
```

### 2. `/src/app/md-explorer/models/tree-event.interface.ts`

```typescript
/**
 * Eventi dell'albero che richiedono preservazione dello stato
 */
export interface TreeEvent {
  type: 'node-expanded' | 'node-collapsed' | 'node-selected' | 
        'file-renamed' | 'file-created' | 'file-deleted' | 
        'tree-refreshed';
  payload: any;
  timestamp: Date;
}

/**
 * Evento specifico per rinominamento file
 */
export interface FileRenamedEvent extends TreeEvent {
  type: 'file-renamed';
  payload: {
    oldPath: string;
    newPath: string;
    oldName: string;
    newName: string;
  };
}
```

### 3. `/src/app/md-explorer/services/tree-state.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { TreeStateMemento, PathRenameMap } from '../models/tree-state-memento.interface';

/**
 * Servizio Caretaker che gestisce il salvataggio e ripristino dei Memento
 */
@Injectable({
  providedIn: 'root'
})
export class TreeStateService {
  private states = new Map<string, TreeStateMemento>();
  private pathRenameMappings = new Map<string, string>();
  private lastSavedState: TreeStateMemento | null = null;

  /**
   * Salva uno stato con una chiave specifica
   */
  saveState(key: string, state: TreeStateMemento): void {
    this.states.set(key, { ...state });
    this.lastSavedState = { ...state };
    
    // Mantieni solo gli ultimi 10 stati per evitare memory leak
    if (this.states.size > 10) {
      const firstKey = this.states.keys().next().value;
      this.states.delete(firstKey);
    }
  }

  /**
   * Recupera uno stato salvato
   */
  getState(key: string): TreeStateMemento | undefined {
    return this.states.get(key);
  }

  /**
   * Recupera l'ultimo stato salvato
   */
  getLastState(): TreeStateMemento | null {
    return this.lastSavedState;
  }

  /**
   * Registra un rinominamento di percorso
   */
  registerPathRename(oldPath: string, newPath: string): void {
    this.pathRenameMappings.set(oldPath, newPath);
    
    // Aggiorna anche i percorsi figli
    this.pathRenameMappings.forEach((value, key) => {
      if (key.startsWith(oldPath + '/')) {
        const newChildPath = key.replace(oldPath, newPath);
        this.pathRenameMappings.set(key, newChildPath);
      }
    });
  }

  /**
   * Ottiene il nuovo percorso per un percorso rinominato
   */
  getMappedPath(originalPath: string): string {
    return this.pathRenameMappings.get(originalPath) || originalPath;
  }

  /**
   * Pulisce le mappature dei rinominamenti
   */
  clearRenameMappings(): void {
    this.pathRenameMappings.clear();
  }

  /**
   * Trasforma uno stato considerando i rinominamenti
   */
  transformStateWithRenameMappings(state: TreeStateMemento): TreeStateMemento {
    return {
      ...state,
      expandedNodes: state.expandedNodes.map(path => this.getMappedPath(path)),
      selectedNodePath: state.selectedNodePath ? this.getMappedPath(state.selectedNodePath) : null,
      activeNodePath: state.activeNodePath ? this.getMappedPath(state.activeNodePath) : null
    };
  }

  /**
   * Pulisce tutti gli stati salvati
   */
  clearAllStates(): void {
    this.states.clear();
    this.lastSavedState = null;
    this.pathRenameMappings.clear();
  }
}
```

### 4. `/src/app/md-explorer/services/tree-state-manager.service.ts`

```typescript
import { Injectable, OnDestroy } from '@angular/core';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Subject, Subscription, merge } from 'rxjs';
import { debounceTime, filter } from 'rxjs/operators';
import { TreeStateService } from './tree-state.service';
import { MdFileService } from './md-file.service';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { TreeStateMemento } from '../models/tree-state-memento.interface';
import { IFileInfoNode } from '../models/IFileInfoNode';

/**
 * Facade service che coordina la gestione dello stato dell'albero
 */
@Injectable({
  providedIn: 'root'
})
export class TreeStateManager implements OnDestroy {
  private subscriptions = new Subscription();
  private stateChangeDebouncer = new Subject<void>();
  private treeControl: FlatTreeControl<IFileInfoNode> | null = null;
  private scrollContainer: HTMLElement | null = null;

  constructor(
    private treeStateService: TreeStateService,
    private mdFileService: MdFileService,
    private mdServerMessages: MdServerMessagesService
  ) {
    this.setupStateChangeDebouncer();
    this.observeFileSystemEvents();
  }

  /**
   * Inizializza il manager con il tree control
   */
  initialize(treeControl: FlatTreeControl<IFileInfoNode>, scrollContainer?: HTMLElement): void {
    this.treeControl = treeControl;
    this.scrollContainer = scrollContainer || null;
  }

  /**
   * Salva lo stato corrente dell'albero
   */
  saveCurrentState(operationType?: string): TreeStateMemento | null {
    if (!this.treeControl) {
      console.warn('TreeStateManager: treeControl not initialized');
      return null;
    }

    const expandedNodes = this.treeControl.dataNodes
      .filter(node => this.treeControl!.isExpanded(node))
      .map(node => node.fullPath);

    const state: TreeStateMemento = {
      expandedNodes,
      selectedNodePath: this.getSelectedNodePath(),
      activeNodePath: this.getActiveNodePath(),
      scrollPosition: this.scrollContainer?.scrollTop || 0,
      timestamp: new Date(),
      operationType: operationType as any
    };

    const stateKey = operationType || 'manual-save';
    this.treeStateService.saveState(stateKey, state);
    
    console.log(`TreeStateManager: Stato salvato (${stateKey}):`, {
      expandedCount: expandedNodes.length,
      hasSelection: !!state.selectedNodePath
    });

    return state;
  }

  /**
   * Ripristina lo stato dell'albero
   */
  restoreState(stateKey?: string, delayMs: number = 100): void {
    if (!this.treeControl) {
      console.warn('TreeStateManager: treeControl not initialized');
      return;
    }

    setTimeout(() => {
      const state = stateKey 
        ? this.treeStateService.getState(stateKey)
        : this.treeStateService.getLastState();

      if (!state) {
        console.warn('TreeStateManager: Nessuno stato da ripristinare');
        return;
      }

      // Trasforma lo stato considerando eventuali rinominamenti
      const transformedState = this.treeStateService.transformStateWithRenameMappings(state);

      // Ripristina i nodi espansi
      this.restoreExpandedNodes(transformedState.expandedNodes);

      // Ripristina la selezione (se presente)
      if (transformedState.selectedNodePath) {
        this.restoreSelection(transformedState.selectedNodePath);
      }

      // Ripristina la posizione di scroll
      if (this.scrollContainer && transformedState.scrollPosition > 0) {
        this.scrollContainer.scrollTop = transformedState.scrollPosition;
      }

      // Pulisci le mappature dopo il ripristino
      this.treeStateService.clearRenameMappings();

      // CRITICO: Notifica il tree component di aggiornare la UI
      // Necessario per componenti con ChangeDetectionStrategy.OnPush
      this.notifyTreeComponentForUpdate();

      console.log('TreeStateManager: Stato ripristinato:', {
        expandedCount: transformedState.expandedNodes.length,
        hadSelection: !!transformedState.selectedNodePath
      });
    }, delayMs);
  }

  /**
   * Esegue un'operazione preservando lo stato dell'albero
   */
  executeWithStatePreservation<T>(
    operation: () => T | Promise<T>, 
    operationType: string = 'operation'
  ): Promise<T> {
    // Salva lo stato prima dell'operazione
    this.saveCurrentState(operationType);

    // Esegui l'operazione
    const result = operation();

    // Gestisci sia operazioni sincrone che asincrone
    if (result instanceof Promise) {
      return result.then(value => {
        this.scheduleStateRestore(operationType);
        return value;
      });
    } else {
      this.scheduleStateRestore(operationType);
      return Promise.resolve(result);
    }
  }

  /**
   * Registra un rinominamento di file per aggiornare i percorsi
   */
  registerFileRename(oldPath: string, newPath: string): void {
    this.treeStateService.registerPathRename(oldPath, newPath);
    console.log(`TreeStateManager: Registrato rinominamento ${oldPath} -> ${newPath}`);
  }

  private setupStateChangeDebouncer(): void {
    this.subscriptions.add(
      this.stateChangeDebouncer.pipe(
        debounceTime(300)
      ).subscribe(() => {
        this.restoreState();
      })
    );
  }

  private observeFileSystemEvents(): void {
    // Osserva eventi di rinominamento dal server
    this.mdServerMessages.addMarkdownFileRenamedListener((data, component) => {
      if (data.oldPath && data.newPath) {
        this.registerFileRename(data.oldPath, data.newPath);
      }
    }, this);

    // Osserva aggiornamenti del file service che richiedono preservazione stato
    this.subscriptions.add(
      this.mdFileService.mdFiles.pipe(
        // Salta il primo emit (caricamento iniziale)
        filter((_, index) => index > 0)
      ).subscribe(() => {
        // Schedula il ripristino dello stato dopo l'aggiornamento dei dati
        this.scheduleStateRestore('data-update');
      })
    );
  }

  private scheduleStateRestore(operationType: string): void {
    console.log(`TreeStateManager: Scheduling state restore for ${operationType}`);
    this.stateChangeDebouncer.next();
  }

  private restoreExpandedNodes(expandedPaths: string[]): void {
    if (!this.treeControl) return;

    // Prima collassa tutto per partire da uno stato pulito
    this.treeControl.collapseAll();

    // Espandi i nodi salvati
    expandedPaths.forEach(path => {
      const node = this.treeControl!.dataNodes.find(n => n.fullPath === path);
      if (node && this.treeControl!.isExpandable(node)) {
        this.treeControl!.expand(node);
      }
    });
  }

  private restoreSelection(selectedPath: string): void {
    // Questo metodo dovrebbe comunicare con il componente per ripristinare la selezione
    // Implementazione dipende da come viene gestita la selezione nel tree component
    const node = this.treeControl?.dataNodes.find(n => n.fullPath === selectedPath);
    if (node) {
      // Trigger selezione tramite service
      this.mdFileService.setSelectedMdFileFromSideNav(node as any);
    }
  }

  /**
   * CRITICO: Metodo per notificare il tree component di aggiornare la UI
   * Necessario per componenti con ChangeDetectionStrategy.OnPush
   */
  private notifyTreeComponentForUpdate(): void {
    // Il tree component deve gestire il change detection internamente
    // Questo sarà fatto tramite il callback nel metodo initialize
    if (this.treeComponentUpdateCallback) {
      this.treeComponentUpdateCallback();
    }
  }

  // IMPORTANTE: Callback per aggiornare il tree component
  private treeComponentUpdateCallback: (() => void) | null = null;

  /**
   * Imposta il callback per aggiornare il tree component
   */
  setUpdateCallback(callback: () => void): void {
    this.treeComponentUpdateCallback = callback;
  }

  private getSelectedNodePath(): string | null {
    // Implementazione dipende da come il tree component traccia il nodo selezionato
    // Il tree component deve fornire questa info tramite il metodo getSelectedNodePath()
    return null;
  }

  private getActiveNodePath(): string | null {
    // Implementazione dipende da come il tree component traccia il nodo attivo
    // Il tree component deve fornire questa info tramite il metodo getActiveNodePath()
    return null;
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    this.treeStateService.clearAllStates();
  }
}
```

## Modifiche ai File Esistenti

### 1. Modifiche a `md-tree.component.ts`

Aggiungi questi import all'inizio del file:
```typescript
import { TreeStateManager } from '../../services/tree-state-manager.service';
```

**CRITICO - Aggiungi proprietà per tracciare la selezione:**
```typescript
export class MdTreeComponent implements OnInit, OnDestroy, AfterViewInit {
  // ... proprietà esistenti ...
  
  // CRITICO: Aggiungi questa proprietà per tracciare il nodo selezionato
  private selectedNodePath: string | null = null;
  
  // IMPORTANTE: Aggiungi riferimento al container per scroll
  @ViewChild('treeContainer', { static: false }) 
  treeContainer: ElementRef<HTMLElement>;
}
```

Aggiungi al costruttore:
```typescript
constructor(
  // ... parametri esistenti ...
  private treeStateManager: TreeStateManager
) {
  // ... codice esistente ...
}
```

**IMPORTANTE - Implementa AfterViewInit per inizializzare con scroll container:**
```typescript
ngAfterViewInit(): void {
  // Inizializza il tree state manager con il container di scroll
  this.treeStateManager.initialize(
    this.treeControl, 
    this.treeContainer?.nativeElement
  );
  
  // CRITICO: Imposta il callback per aggiornare la UI dopo il ripristino dello stato
  // Necessario per componenti con ChangeDetectionStrategy.OnPush
  this.treeStateManager.setUpdateCallback(() => {
    this.changeDetectorRef.markForCheck();
  });
}
```

Modifica il metodo `ngOnInit`:
```typescript
ngOnInit(): void {
  // NON inizializzare qui il tree state manager - spostato in ngAfterViewInit
  
  this.mdFiles = this.mdFileService.mdFiles;
  this.mdFileService.mdFiles.subscribe(data => {
    // Non salvare lo stato al primo caricamento
    const isFirstLoad = this.dataSource.data.length === 0;
    
    if (!isFirstLoad) {
      // Salva lo stato prima dell'aggiornamento
      this.treeStateManager.saveCurrentState('data-refresh');
    }
    
    // Inizializza ricorsivamente tutte le proprietà
    this.initializeNodeProperties(data);
    this.dataSource.data = data;
    
    // Con OnPush, forza il re-check del componente
    this.changeDetectorRef.markForCheck();
    
    // Il ripristino dello stato avverrà automaticamente tramite l'observer
  });
  
  this.mdFileService.loadAll(this.deferredOpenProject, this);
}
```

**CRITICO - Modifica il metodo selectItem per tracciare la selezione:**
```typescript
selectItem(node: IMdFile) {
  // CRITICO: Salva il percorso del nodo selezionato
  this.selectedNodePath = node.fullPath;
  
  // ... resto del codice esistente ...
  this.selectedNode = node;
  this.mdFileService.setSelectedMdFileFromSideNav(node);
}
```

Aggiungi metodi per fornire info al TreeStateManager:
```typescript
/**
 * Fornisce il percorso del nodo selezionato al TreeStateManager
 */
getSelectedNodePath(): string | null {
  // CRITICO: Ora restituisce il valore reale invece di null
  return this.selectedNodePath;
}

/**
 * Fornisce il percorso del nodo attivo al TreeStateManager
 */
getActiveNodePath(): string | null {
  return this.activeNode?.path || null;
}
```

Modifica il metodo `handleNewMarkdownFileCreated`:
```typescript
private handleNewMarkdownFileCreated(data: any): void {
  // Se è un rinominamento, registralo nel state manager
  if (data.oldPath && data.newPath) {
    this.treeStateManager.registerFileRename(data.oldPath, data.newPath);
  }
  
  // Esegui il refresh con preservazione dello stato
  this.treeStateManager.executeWithStatePreservation(() => {
    this.mdFileService.loadAll(null, null);
  }, 'file-created');
}
```

### 2. Modifiche a `md-file.service.ts`

Aggiungi un nuovo Subject per eventi di rinominamento:
```typescript
// Aggiungi dopo gli altri Subject
private _fileRenamed = new Subject<{oldPath: string, newPath: string}>();
public fileRenamed$ = this._fileRenamed.asObservable();
```

Modifica il metodo `changeDataStoreMdFiles`:
```typescript
changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
  var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, oldFile);
  var leaf = returnFound[0];
  
  if (!leaf) {
    console.error('❌ [Service] File non trovato nel datastore:', oldFile.name);
    return;
  }
  
  // Emetti evento di rinominamento PRIMA di aggiornare i dati
  this._fileRenamed.next({
    oldPath: oldFile.fullPath,
    newPath: newFile.fullPath
  });
  
  // Aggiorna le proprietà del file
  leaf.name = newFile.name;
  leaf.fullPath = newFile.fullPath;
  leaf.path = newFile.path;
  leaf.relativePath = newFile.relativePath;
  
  // Per file rinominati via Rule #1, forza come indicizzato
  leaf.isIndexed = true;
  leaf.indexingStatus = 'completed';
  
  // Forza nuova referenza per triggerare OnPush change detection
  this._mdFiles.next([...this.dataStore.mdFiles]);
  this._serverSelectedMdFile.next([...returnFound]);
  
  // Notifica il tree component per aggiornare il Set di tracking
  this.mdServerMessages.triggerRule1ForceUpdate(leaf.fullPath);
}
```

### 3. Modifiche a `rules.component.ts`

Inietta il TreeStateManager e modifica il metodo `changeName`:
```typescript
constructor(
  // ... parametri esistenti ...
  private treeStateManager: TreeStateManager
) { }

changeName(): void {
  this.clearError();
  this.isProcessing = true;
  
  // Salva lo stato dell'albero prima del rinominamento
  this.treeStateManager.saveCurrentState('rule1-rename');
  
  this.refactoringService.renameFileName(this.data)
    .subscribe({
      next: (data) => {
        // ... codice esistente ...
        
        // Registra il rinominamento per l'aggiornamento dei percorsi
        this.treeStateManager.registerFileRename(data.oldPath, data.newPath);
        
        this.mdFileService.changeDataStoreMdFiles(oldFile, newFile);
        
        // Forza l'indicizzazione con un delay per assicurare la consistenza
        setTimeout(() => {
          this.mdFileService.forceFileAsIndexed(newFile.fullPath);
          this.isProcessing = false;
          this.dialogRef.close(data);
          // Il ripristino dello stato avverrà automaticamente
        }, 150);
      },
      error: (error) => {
        // ... codice esistente ...
      }
    });
}
```

### 4. Modifiche a `md-explorer.module.ts`

Aggiungi i nuovi servizi ai providers:
```typescript
import { TreeStateService } from './services/tree-state.service';
import { TreeStateManager } from './services/tree-state-manager.service';

@NgModule({
  // ... configurazione esistente ...
  providers: [
    // ... providers esistenti ...
    TreeStateService,
    TreeStateManager
  ]
})
export class MdExplorerModule { }
```

### 5. **IMPORTANTE - Modifiche al template `md-tree.component.html`**

Aggiungi il riferimento template per il container di scroll:
```html
<!-- Aggiungi #treeContainer al div che contiene il mat-tree -->
<div class="tree-container" #treeContainer>
  <mat-tree [dataSource]="dataSource" [treeControl]="treeControl">
    <!-- ... resto del template esistente ... -->
  </mat-tree>
</div>
```

Se il container è diverso nel tuo template, assicurati di aggiungere `#treeContainer` all'elemento HTML che:
- Contiene il `<mat-tree>`
- Ha la proprietà CSS `overflow-y: auto` o `scroll`
- È l'elemento che effettivamente scrolla quando l'albero è lungo

## Flusso di Esecuzione

### Scenario 1: Rinominamento file tramite Rule #1

1. L'utente modifica il titolo del documento
2. Il sistema rileva la violazione della Rule #1 e mostra il dialog
3. L'utente clicca "Apply Suggestion" in `RulesComponent`
4. Il flusso:
   ```
   RulesComponent.changeName()
   ├── TreeStateManager.saveCurrentState('rule1-rename')
   ├── refactoringService.renameFileName()
   ├── TreeStateManager.registerFileRename(oldPath, newPath)
   ├── mdFileService.changeDataStoreMdFiles()
   │   └── emette _fileRenamed event
   └── Dialog si chiude
   
   TreeStateManager (observer)
   ├── riceve notifica di aggiornamento dati
   ├── applica trasformazione percorsi
   └── ripristina stato con delay di 300ms
   ```

### Scenario 2: Creazione nuovo file

1. L'utente crea un nuovo file markdown
2. Il flusso:
   ```
   NewMarkdownComponent
   ├── crea il file
   └── chiude dialog
   
   MonitorMDHostedService
   └── emette evento 'markdownFileCreated'
   
   md-tree.component
   ├── handleNewMarkdownFileCreated()
   ├── TreeStateManager.executeWithStatePreservation()
   │   ├── salva stato
   │   ├── ricarica dati
   │   └── schedula ripristino
   └── stato ripristinato dopo 300ms
   ```

## Test e Verifica

### Test Manuali da Eseguire

1. **Test Rinominamento Rule #1**:
   - Espandi diversi livelli dell'albero
   - Modifica il titolo di un documento per violare la Rule #1
   - Applica il suggerimento
   - ✓ Verifica che l'albero mantenga lo stato espanso
   - ✓ Verifica che il file rinominato sia visibile

2. **Test Creazione File**:
   - Espandi una cartella
   - Crea un nuovo file in quella cartella
   - ✓ Verifica che la cartella rimanga espansa
   - ✓ Verifica che il nuovo file sia visibile

3. **Test Performance**:
   - Con un albero di 1000+ file
   - Espandi 20+ cartelle
   - Esegui un rinominamento
   - ✓ Verifica che il ripristino sia fluido (<500ms)

### Debug e Troubleshooting

Per debuggare problemi di stato:

1. Abilita i log nel browser console:
   ```javascript
   localStorage.setItem('debug:treeState', 'true');
   ```

2. Controlla i log per:
   - "TreeStateManager: Stato salvato"
   - "TreeStateManager: Stato ripristinato"
   - "TreeStateManager: Registrato rinominamento"

3. Verifica lo stato salvato:
   ```javascript
   // Nella console del browser
   const stateService = window.ng.getInjector().get('TreeStateService');
   console.log(stateService.getLastState());
   ```

## Note Implementative

1. **Change Detection**: Tutti i componenti usano `OnPush`, quindi è importante chiamare `markForCheck()` dopo il ripristino dello stato

2. **Timing**: Il delay di 100-300ms è necessario per permettere al DOM di aggiornarsi prima del ripristino

3. **Memory Management**: Il TreeStateService mantiene solo gli ultimi 10 stati per evitare memory leak

4. **Compatibilità**: L'implementazione è retrocompatibile - se il TreeStateManager non è inizializzato, l'albero funziona come prima

5. **Estensibilità**: È facile aggiungere nuovi tipi di eventi o modificare la strategia di preservazione dello stato

## ⚠️ Punti Critici per l'Implementazione

### 1. **Tracciamento Selezione (CRITICO)**
- **DEVE** aggiungere la proprietà `selectedNodePath` in `md-tree.component.ts`
- **DEVE** aggiornare questa proprietà nel metodo `selectItem()`
- Senza questo, la selezione si perderà sempre dopo il refresh

### 2. **Change Detection (CRITICO)**
- **DEVE** impostare il callback di aggiornamento in `ngAfterViewInit()`
- **DEVE** chiamare `changeDetectorRef.markForCheck()` nel callback
- Senza questo, l'UI non si aggiornerà con OnPush

### 3. **Scroll Container (IMPORTANTE)**
- **DOVREBBE** aggiungere `@ViewChild('treeContainer')` al componente
- **DOVREBBE** aggiungere `#treeContainer` nel template HTML
- Senza questo, funziona ma senza ripristino scroll

### 4. **Gestione TreeStateManager nel componente**
Il TreeStateManager deve essere gestito correttamente per integrare lo stato:
- Nel metodo `getSelectedNodePath()` del componente, restituire `this.selectedNodePath`
- Non dimenticare di aggiungere `AfterViewInit` all'implements della classe

### 5. **Ordine di Inizializzazione**
- L'inizializzazione del TreeStateManager **DEVE** avvenire in `ngAfterViewInit()`, non in `ngOnInit()`
- Questo garantisce che il ViewChild sia disponibile

## 🚀 Checklist Pre-Implementazione

Prima di iniziare l'implementazione, verifica di avere:

- [ ] Angular 11+ con CDK Tree installato
- [ ] RxJS per gestione eventi reattivi
- [ ] Accesso ai servizi esistenti (MdFileService, MdServerMessagesService)
- [ ] Comprensione del pattern OnPush Change Detection

## 💡 Suggerimenti per Test Rapido

Per verificare rapidamente che l'implementazione funzioni:

1. Espandi alcuni nodi nell'albero
2. Apri la console del browser
3. Rinomina un file (deve apparire log "TreeStateManager: Stato salvato")
4. Dopo 300ms deve apparire "TreeStateManager: Stato ripristinato"
5. I nodi espansi devono rimanere espansi