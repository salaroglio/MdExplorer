import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnInit, ViewChild, ChangeDetectorRef, ChangeDetectionStrategy, AfterViewInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatMenuTrigger } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { Observable, BehaviorSubject } from 'rxjs';
import { IFileInfoNode } from '../../models/IFileInfoNode';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';
import { MdNavigationService } from '../../services/md-navigation.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { ChangeDirectoryComponent } from '../dialogs/change-directory/change-directory.component';
import { NewDirectoryComponent } from '../dialogs/new-directory/new-directory.component';
import { NewMarkdownComponent } from '../dialogs/new-markdown/new-markdown.component';
import { Clipboard } from '@angular/cdk/clipboard';
import { DeleteMarkdownComponent } from '../dialogs/delete-markdown/delete-markdown.component';
import { Router } from '@angular/router';
import { fadeInOnEnterAnimation } from 'angular-animations';
import { CopyFromClipboardComponent } from '../dialogs/copy-from-clipboard/copy-from-clipboard.component';
import { MoveMdFileComponent } from '../dialogs/move-md-file/move-md-file.component';
import { AddNewFileToMDEComponent } from '../dialogs/add-new-file-to-mde/add-new-file-to-mde.component';
import { TocGenerationService } from '../../services/toc-generation.service';
import { TocProgressService } from '../../services/toc-progress.service';

const TREE_DATA: IFileInfoNode[] = [];

@Component({
  selector: 'app-md-tree',
  templateUrl: './md-tree.component.html',
  styleUrls: ['./md-tree.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    fadeInOnEnterAnimation()
  ]
})
export class MdTreeComponent implements OnInit, AfterViewInit, OnDestroy {

  private hooked: boolean = false;
  private activeNode: any;
  public selectedNode: MdFile | null = null;
  mdFiles: Observable<MdFile[]>;
  
  // BehaviorSubject per tracciare lo stato di indicizzazione
  private indexedFilesSubject = new BehaviorSubject<Set<string>>(new Set());
  indexedFiles$ = this.indexedFilesSubject.asObservable();
  
  // Debouncing per aggiornamenti batch
  private pendingUpdates = new Map<string, boolean>();
  private updateTimer: any = null;
  
  // Contatore delle directory indicizzate
  private indexedFoldersCount = 0;
  private currentSnackbarRef: any = null;

  menuTopLeftPosition = { x: 0, y: 0 }
  @ViewChild(MatMenuTrigger, { static: true }) matMenuTrigger: MatMenuTrigger;


  private _transformer = (node: IFileInfoNode, level: number) => {
    return {
      expandable: (!!node.childrens && node.childrens.length > 0) || node.type == "folder",
      name: node.name,
      level: level,
      path: node.path,
      relativePath: node.path,
      fullPath: node.fullPath,
      type: node.type,
      index:node.index
    };
  }
  treeControl = new FlatTreeControl<IFileInfoNode>(
    node => node.level, node => node.expandable);

  treeFlattener = new MatTreeFlattener(
    this._transformer, node => node.level, node => node.expandable, node => node.childrens);

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  hasChild = (_: number, node: IFileInfoNode) => node.expandable;

  isFolder = (_: number, node: IFileInfoNode) => node.type == "folder";
  isMdPublish = (_: number, node: IFileInfoNode) => node.type == "folder" && node.name == "mdPublish";
  isEmptyRoot = (_: number, node: IFileInfoNode) => node.type == "emptyroot";

  ///////////////////////////////


  constructor(private router: Router,
    private mdFileService: MdFileService,
    private navService: MdNavigationService,
    public dialog: MatDialog,
    private snackBar: MatSnackBar,
    private clipboard: Clipboard,
    private changeDetectorRef: ChangeDetectorRef,
    private mdServerMessages: MdServerMessagesService,
    private tocService: TocGenerationService,
    private tocProgressService: TocProgressService
  ) {
    this.dataSource.data = TREE_DATA;
    this.mdFileService.serverSelectedMdFile.subscribe(_ => {      
      const myClonedArray = [];
      _.forEach(val => myClonedArray.push(Object.assign({}, val)));
      while (myClonedArray.length > 1) {
        var toExpand = myClonedArray.pop();
        var test = this.treeControl.dataNodes.find(_ => _.path == toExpand.path);
        this.treeControl.expand(test);
      }
      if (myClonedArray.length > 0) {
        var toExpand = myClonedArray.pop();
        this.activeNode = this.treeControl.dataNodes.find(_ => _.path == toExpand.path);
        
        if (this.activeNode!= undefined && this.activeNode.type == "folder") {
          this.treeControl.expand(this.activeNode);
        }
      }
    });

    // Aggiungi listener per file indicizzati
    this.mdServerMessages.addFileIndexedListener((data, component) => {
      const currentSet = this.indexedFilesSubject.value;
      const newSet = new Set(currentSet);
      newSet.add(data.path);
      this.indexedFilesSubject.next(newSet);
      
      // Trova e aggiorna direttamente il nodo nel dataSource
      this.updateNodeIndexStatus(data.path, true);
      
      // Forza il refresh del tree
      this.changeDetectorRef.detectChanges();
    }, this);

    // Reset contatore quando inizia una nuova indicizzazione
    this.mdServerMessages.addParsingProjectStartListener((data, component) => {
      this.indexedFoldersCount = 0;
      // Non chiudiamo la snackbar, la riutilizzeremo per i nuovi aggiornamenti
      if (this.currentSnackbarRef) {
        this.updateSnackbarContent('Iniziando indicizzazione...');
      }
    }, this);

    // Aggiungi listener per cartelle in indicizzazione
    this.mdServerMessages.addFolderIndexingStartListener((data, component) => {
      const node = this.findNodeByPath(data.path);
      if (node) {
        node.indexingStatus = 'indexing';
        this.changeDetectorRef.detectChanges();
      }
    }, this);

    this.mdServerMessages.addFolderIndexingCompleteListener((data, component) => {
      const node = this.findNodeByPath(data.path);
      if (node) {
        node.indexingStatus = 'completed';
        this.changeDetectorRef.detectChanges();
        
        // Mostra snackbar throttled per cartella indicizzata
        this.showIndexingSnackbar(node.name);
      }
    }, this);
    
    // Listener per fine indicizzazione completa
    this.mdServerMessages.addParsingProjectStopListener((data, component) => {
      if (this.currentSnackbarRef) {
        // Aggiorna con messaggio finale e chiudi dopo 3 secondi
        this.updateSnackbarContent(`Completato! ${this.indexedFoldersCount} directory indicizzate`);
        setTimeout(() => {
          if (this.currentSnackbarRef) {
            this.currentSnackbarRef.dismiss();
          }
        }, 3000);
      }
    }, this);

    // Listener per la creazione di nuovi file markdown
    this.mdServerMessages.addMarkdownFileCreatedListener((data, component) => {
      this.handleNewMarkdownFileCreated(data);
    }, this);
    
    // Listener per la cancellazione di file markdown
    this.mdServerMessages.addMarkdownFileDeletedListener((data, component) => {
      this.handleMarkdownFileDeleted(data);
    }, this);
    
    // Listener per forzare change detection (Rule #1 fix) - seguendo il pattern SignalR
    this.mdServerMessages.addRule1ForceUpdateListener((data, component) => {
      // Questo non verrà mai chiamato perché non c'è un vero evento SignalR
    }, this);
  }
 
  //="{ value: '', params: { delay: node.index * 100 } }"
  ngOnInit(): void {
    this.mdFiles = this.mdFileService.mdFiles;
    this.mdFileService.mdFiles.subscribe(data => {
      // Inizializza ricorsivamente tutte le proprietà
      this.initializeNodeProperties(data);
      this.dataSource.data = data;
      // Con OnPush, forza il re-check del componente
      this.changeDetectorRef.markForCheck();
    });
    this.mdFileService.loadAll(this.deferredOpenProject, this);
  }

  private initializeNodeProperties(nodes: any[]): void {
    nodes.forEach(node => {
      node.index = 0;
      if (node.level === 0) {
        node.index = Math.floor(Math.random() * 5);
      }
      // Inizializza la proprietà isIndexed per tutti i file markdown
      if (node.type === 'mdFile' || node.type === 'mdFileTimer') {
        node.isIndexed = node.isIndexed || false; // Mantieni il valore esistente o imposta false
      }
      // Ricorsione per i children
      if (node.childrens && node.childrens.length > 0) {
        this.initializeNodeProperties(node.childrens);
      }
    });
  }

  deferredOpenProject(data, objectThis: MdTreeComponent): void {
    objectThis.mdFileService.getLandingPage().subscribe(node => {
      if (node != null) {
        console.log('🏠 Landing page trovata:', node.name, 'Path:', node.fullPath);
        
        // Aspetta che l'albero sia completamente renderizzato
        setTimeout(() => {
          console.log('🌳 TreeControl dataNodes count:', objectThis.treeControl.dataNodes?.length);
          
          // Espandi manualmente l'albero fino al file
          objectThis.expandToLandingPage(node);
          
          // Usa setSelectedMdFileFromServer per attivare l'espansione dell'albero
          objectThis.mdFileService.setSelectedMdFileFromServer(node);
          objectThis.mdFileService.setSelectedMdFileFromSideNav(node);
          objectThis.navService.setNewNavigation(node);
          objectThis.activeNode = node;
          objectThis.selectedNode = node;
          objectThis.changeDetectorRef.markForCheck();
        }, 500);
      }
    });
  }

  onRightClick(event: MouseEvent, item) {
    // preventDefault avoids to show the visualization of the right-click menu of the browser
    event.preventDefault();
    if (item == null) {
      item = new MdFile("root", "root", 0, false);
      item.fullPath = "root";
    }
    // we record the mouse position in our object
    this.menuTopLeftPosition.x = event.clientX;
    this.menuTopLeftPosition.y = event.clientY;

    // we open the menu
    // we pass to the menu the information about our object
    this.matMenuTrigger.menuData = { item: item }

    // we open the menu
    this.matMenuTrigger.openMenu();

  }

  public async getNode(node: MdFile) {
    if (this.isFileWaiting(node)) {
      // Feedback per file in indicizzazione
      this.snackBar.open('File in indicizzazione', 'OK', { duration: 3000 });
      return;
    }
    
    try {
      // ✅ ASPETTA che la navigazione sia completata
      await this.router.navigate(['/main/navigation/document']);
      
      // ✅ SOLO DOPO navigazione riuscita, aggiorna gli stati
      this.mdFileService.setSelectedMdFileFromSideNav(node);
      this.navService.setNewNavigation(node);
      this.activeNode = node;
      this.selectedNode = node;
      this.changeDetectorRef.markForCheck();
      
    } catch (error) {
      // ✅ Se navigazione fallisce, nessun state viene cambiato
      console.error('Navigation failed:', error);
      this.snackBar.open('Errore di navigazione', 'OK', { duration: 3000 });
    }
  }

  // Manu management

  createMdOn(node: MdFile) {

    this.dialog.open(NewMarkdownComponent, {
      width: '300px',
      //height:'400px',
      data: node,
    });
  }

  setMdAsLandingPage(node: MdFile) {
    this.mdFileService.SetLandingPage(node).subscribe(_ => {
      this.snackBar.open(node.name, "is project landing page", { duration: 5000 });
      
      // Espandi manualmente l'albero fino al file
      setTimeout(() => {
        this.expandToLandingPage(node);
        
        // Seleziona e espandi automaticamente la nuova landing page
        this.mdFileService.setSelectedMdFileFromServer(node);
        this.mdFileService.setSelectedMdFileFromSideNav(node);
        this.navService.setNewNavigation(node);
        this.activeNode = node;
        this.selectedNode = node;
        this.changeDetectorRef.markForCheck();
        
        // Naviga al documento
        this.router.navigate(['/main/navigation/document']);
      }, 100);
    });
  }

  moveDocument(node: MdFile):void {
    this.dialog.open(MoveMdFileComponent, {
      width: '300px',
      data: node,
    });
  }

  createDirectoryOn(node: MdFile) {
    if (node == null) {
      node = new MdFile("root", "root", 0, false);
      node.fullPath = "root";
    }
    this.dialog.open(NewDirectoryComponent, {
      width: '300px',
      data: node,
    });
  }

  renameDirectoryOn(node: MdFile) {
    if (node == null) {
      node = new MdFile("root", "root", 0, false);
      node.fullPath = "root";
    }
    this.dialog.open(ChangeDirectoryComponent, {
      width: '300px',
      data: node,
    });
  }

  openFolderOn(node: MdFile) {
    console.log('[MdTreeComponent] openFolderOn() called');
    console.log('[MdTreeComponent] node:', node);
    console.log('[MdTreeComponent] node.fullPath:', node.fullPath);
    
    this.mdFileService.openFolderOnFileExplorer(node).subscribe(
      result => {
        console.log('[MdTreeComponent] openFolderOnFileExplorer success:', result);
        this.snackBar.open("file explorer open", "", { duration: 500 });
      },
      error => {
        console.error('[MdTreeComponent] openFolderOnFileExplorer error:', error);
        this.snackBar.open("Error opening file explorer: " + error.message, "", { duration: 3000 });
      }
    );
  }

  async openTocDirectory(node: MdFile) {
    console.log('[MdTreeComponent] openTocDirectory() called');
    console.log('[MdTreeComponent] node:', node);
    console.log('[MdTreeComponent] node.name:', node.name);
    console.log('[MdTreeComponent] node.relativePath:', node.relativePath);
    console.log('[MdTreeComponent] node.fullPath:', node.fullPath);
    console.log('[MdTreeComponent] node.path:', node.path);
    
    // Check if TOC file already exists
    if (this.tocFileExists(node)) {
      console.log('[MdTreeComponent] TOC file exists, navigating directly');
      this.navigateToTocFile(node);
    } else {
      console.log('[MdTreeComponent] TOC file does not exist, generating with AI');
      // Start TOC generation with AI
      this.generateTocWithAI(node, true);
    }
  }
  
  async refreshTocDirectory(node: MdFile) {
    console.log('[MdTreeComponent] refreshTocDirectory() called');
    
    const directoryName = node.name;
    let relativePath = node.relativePath || '';
    // Remove leading backslash if present
    if (relativePath.startsWith('\\')) {
      relativePath = relativePath.substring(1);
    }
    const tocPath = relativePath ? 
      `${relativePath}/${directoryName}.md.directory` : 
      `${directoryName}.md.directory`;
    
    this.tocService.refreshToc(tocPath).subscribe({
      next: (result) => {
        if (result.success) {
          this.snackBar.open('TOC aggiornato con successo', 'OK', { duration: 3000 });
          // Navigate to the updated TOC
          this.navigateToTocFile(node);
        } else {
          this.snackBar.open('Aggiornamento TOC fallito', 'OK', { duration: 3000 });
        }
      },
      error: (err) => {
        console.error('Error refreshing TOC:', err);
        this.snackBar.open('Errore durante aggiornamento TOC', 'OK', { duration: 3000 });
      }
    });
  }
  
  async quickTocDirectory(node: MdFile) {
    console.log('[MdTreeComponent] quickTocDirectory() called');
    
    let directoryPath = node.relativePath || node.name;
    // Remove leading backslash if present
    if (directoryPath.startsWith('\\')) {
      directoryPath = directoryPath.substring(1);
    }
    
    this.tocService.generateQuickToc(directoryPath).subscribe({
      next: (result) => {
        if (result.success) {
          this.snackBar.open('TOC rapida generata', 'OK', { duration: 3000 });
          // Navigate to the TOC file
          this.navigateToTocFile(node);
        } else {
          this.snackBar.open('Generazione TOC rapida fallita', 'OK', { duration: 3000 });
        }
      },
      error: (err) => {
        console.error('Error generating quick TOC:', err);
        this.snackBar.open('Errore durante generazione TOC rapida', 'OK', { duration: 3000 });
      }
    });
  }

  async forceRegenerateTocDirectory(node: MdFile) {
    console.log('[MdTreeComponent] forceRegenerateTocDirectory() called');
    
    // Show confirmation dialog
    const confirmMessage = 'Questo sovrascriverà completamente il file TOC esistente, perdendo eventuali modifiche manuali. Continuare?';
    if (!confirm(confirmMessage)) {
      return;
    }
    
    let directoryPath = node.relativePath || node.name;
    // Remove leading backslash if present
    if (directoryPath.startsWith('\\')) {
      directoryPath = directoryPath.substring(1);
    }
    
    // Show progress dialog
    this.tocProgressService.showProgress(directoryPath);
    
    this.tocService.forceRegenerateToc(directoryPath).subscribe({
      next: (result) => {
        console.log('[MdTreeComponent] Force regeneration result:', result);
        
        // SEMPRE chiudi il progress dialog
        this.tocProgressService.hideProgress();
        
        if (result.success) {
          this.snackBar.open('TOC rigenerato con successo', 'OK', { duration: 3000 });
          setTimeout(() => {
            this.navigateToTocFile(node);
          }, 500);
        } else {
          this.snackBar.open(
            result.message || 'Rigenerazione TOC fallita', 
            'OK', 
            { duration: 5000 }
          );
        }
      },
      error: (err) => {
        console.error('[MdTreeComponent] Error force regenerating TOC:', err);
        
        // SEMPRE chiudi il progress dialog
        this.tocProgressService.hideProgress();
        
        let errorMessage = 'Errore durante rigenerazione TOC';
        if (err.error?.error) {
          errorMessage += ': ' + err.error.error;
        } else if (err.error?.message) {
          errorMessage += ': ' + err.error.message;
        }
        this.snackBar.open(errorMessage, 'OK', { duration: 10000 });
      }
    });
  }
  
  tocFileExists(node: MdFile): boolean {
    // Check if the TOC file exists in the tree
    const directoryName = node.name;
    const tocFileName = `${directoryName}.md.directory`;
    
    if (node.childrens && node.childrens.length > 0) {
      return node.childrens.some(child => child.name === tocFileName);
    }
    
    return false;
  }
  
  private generateTocWithAI(node: MdFile, navigateAfter: boolean) {
    // Remove leading backslash if present
    let directoryPath = node.relativePath || node.name;
    if (directoryPath.startsWith('\\')) {
      directoryPath = directoryPath.substring(1);
    }
    console.log('[MdTreeComponent] generateTocWithAI - directoryPath:', directoryPath);
    
    // Mostra il progress dialog
    this.tocProgressService.showProgress(directoryPath);
    
    this.tocService.generateToc(directoryPath).subscribe({
      next: (result) => {
        console.log('[MdTreeComponent] TOC generation result:', result);
        
        // SEMPRE chiudi il progress dialog quando riceviamo una risposta
        this.tocProgressService.hideProgress();
        
        if (result.success) {
          this.snackBar.open('TOC generato con successo', 'OK', { duration: 3000 });
          
          if (navigateAfter) {
            // Naviga al file TOC generato
            setTimeout(() => {
              this.navigateToTocFile(node);
            }, 500);
          }
        } else {
          this.snackBar.open(
            result.message || 'TOC generato senza AI (modello non disponibile)', 
            'OK', 
            { duration: 5000 }
          );
          
          if (navigateAfter && result.tocPath) {
            this.navigateToTocFile(node);
          }
        }
      },
      error: (err) => {
        console.error('[MdTreeComponent] Error generating TOC:', err);
        
        // SEMPRE chiudi il progress dialog in caso di errore
        this.tocProgressService.hideProgress();
        
        // Messaggio di errore dettagliato
        let errorMessage = 'Errore durante generazione TOC';
        if (err.error?.error) {
          errorMessage += ': ' + err.error.error;
        } else if (err.error?.message) {
          errorMessage += ': ' + err.error.message;
        } else if (err.message) {
          errorMessage += ': ' + err.message;
        }
        
        this.snackBar.open(errorMessage, 'OK', { duration: 10000 });
      }
    });
  }
  
  private async navigateToTocFile(node: MdFile) {
    const directoryName = node.name;
    const relativePath = node.relativePath ? 
      `${node.relativePath}/${directoryName}.md.directory` : 
      `${directoryName}.md.directory`;
    
    // Crea un oggetto MdFile per il file .md.directory
    const tocFile: MdFile = {
      name: `${directoryName}.md.directory`,
      relativePath: relativePath,
      fullPath: node.fullPath ? `${node.fullPath}/${directoryName}.md.directory` : `${directoryName}.md.directory`,
      path: node.path,
      type: 'mdFile',
      index: 0,
      childrens: [],
      level: node.level,
      expandable: false,
      isLoading: false,
      fullDirectoryPath: node.fullPath || ''
    };
    
    console.log('[MdTreeComponent] tocFile created:', tocFile);
    
    try {
      // Naviga alla route del documento
      await this.router.navigate(['/main/navigation/document']);
      
      // Imposta il file selezionato
      this.mdFileService.setSelectedMdFileFromSideNav(tocFile);
      this.navService.setNewNavigation(tocFile);
      this.activeNode = tocFile;
      this.selectedNode = tocFile;
      this.changeDetectorRef.markForCheck();
      
      console.log('[MdTreeComponent] Navigation to TOC directory completed');
    } catch (error) {
      console.error('[MdTreeComponent] Error navigating to TOC directory:', error);
      this.snackBar.open('Errore apertura TOC directory', 'OK', { duration: 3000 });
    }
  }

  AddExistingFileOnMDEProject(node: MdFile) {
    this.dialog.open(AddNewFileToMDEComponent, {
      width: '600px',
      data: node,
    });
  }

  getLinkFromNode(node: MdFile) {
    let finalPath = node.relativePath.replace(/\\/g, "/");  
    this.clipboard.copy(finalPath);

  }

  deleteFile(node: MdFile) {
    this.dialog.open(DeleteMarkdownComponent, {
      width: '300px',
      data: node,
    });

  }

  cloneTimerDocument(node: MdFile) {
    this.mdFileService.cloneTimerDocument(node).subscribe(data => {
      this.mdFileService.addNewFile(data);
      this.mdFileService.setSelectedMdFileFromSideNav(data[data.length - 1]);
    });
  }

  openDocumentSettings(node: MdFile) {
    this.mdFileService.setSelectedMdFileFromSideNav(node);
    this.router.navigate(['/main/navigation/documentsettings']);
  }

  pasteFromClipboard(node: MdFile) {
    this.dialog.open(CopyFromClipboardComponent, {
      width: '300px',      
      data: node,
    });
  }

  forceOpenFile(node: MdFile) {
    const message = 'File non ancora indicizzato. Alcune funzionalità potrebbero non funzionare. Continuare?';
    if (confirm(message)) {
      this.router.navigate(['/main/navigation/document']);
      this.mdFileService.setSelectedMdFileFromSideNav(node);
      this.navService.setNewNavigation(node);
      this.activeNode = node;
      this.selectedNode = node;
      this.changeDetectorRef.markForCheck();
    }
  }

  // Debug per tracciare aggiornamenti dell'Observable
  ngAfterViewInit() {
    this.indexedFiles$.subscribe(indexedFiles => {
      // Observable indexedFiles aggiornato, size: indexedFiles.size
      // Forza il re-rendering del template
      this.changeDetectorRef.detectChanges();
    });

    // Registra i listener per TOC Generation progress
    this.mdServerMessages.addTocGenerationProgressListener((data, objectThis) => {
      objectThis.tocProgressService.updateProgress(data);
    }, this);

    this.mdServerMessages.addTocGenerationCompleteListener((data, objectThis) => {
      objectThis.tocProgressService.hideProgress();
      objectThis.snackBar.open('TOC generato con successo!', 'OK', { duration: 3000 });
    }, this);
  }



  private updateNodeIndexStatus(path: string, isIndexed: boolean): void {
    // Aggiungi l'aggiornamento alla coda
    this.pendingUpdates.set(path, isIndexed);
    
    // Cancella il timer esistente
    if (this.updateTimer) {
      clearTimeout(this.updateTimer);
    }
    
    // Imposta un nuovo timer per processare gli aggiornamenti in batch
    this.updateTimer = setTimeout(() => {
      this.processPendingUpdates();
    }, 100); // Attendi 100ms per raggruppare più aggiornamenti
  }
  
  private processPendingUpdates(): void {
    // Processa tutti gli aggiornamenti pendenti in un singolo batch
    this.pendingUpdates.forEach((isIndexed, path) => {
      const node = this.findNodeByPath(path);
      if (node) {
        node.isIndexed = isIndexed;
        node.indexingStatus = isIndexed ? 'completed' : 'idle';
      }
    });
    
    // Pulisci gli aggiornamenti pendenti
    this.pendingUpdates.clear();
    
    // Aggiorna la vista una sola volta per tutti i cambiamenti
    this.changeDetectorRef.markForCheck();
  }

  private findNodeByPath(path: string): MdFile | null {
    // Prima cerca per fullPath esatto
    let found = this.searchInNodes(this.dataSource.data as MdFile[], path);
    if (found) return found;
    
    // Se non trovato, cerca per nome file (caso rinominazione)
    const fileName = path.split('\\').pop() || path.split('/').pop();
    if (fileName) {
      found = this.searchNodesByName(this.dataSource.data as MdFile[], fileName);
    }
    
    return found;
  }

  private searchInNodes(nodes: MdFile[], targetPath: string): MdFile | null {
    for (const node of nodes) {
      if (node.fullPath === targetPath) {
        return node;
      }
      if (node.childrens && node.childrens.length > 0) {
        const found = this.searchInNodes(node.childrens, targetPath);
        if (found) return found;
      }
    }
    return null;
  }
  
  private searchNodesByName(nodes: MdFile[], targetName: string): MdFile | null {
    for (const node of nodes) {
      if (node.name === targetName) {
        return node;
      }
      if (node.childrens && node.childrens.length > 0) {
        const found = this.searchNodesByName(node.childrens, targetName);
        if (found) return found;
      }
    }
    return null;
  }

  // Metodi helper per il template
  isFileIndexed(node: MdFile): boolean {
    // Combina lo stato dal nodo e dal Set di tracking
    const nodeIndexed = node.isIndexed || false;
    const setIndexed = this.indexedFilesSubject.value.has(node.fullPath);
    const result = nodeIndexed || setIndexed;
    
    return result;
  }

  isFileWaiting(node: MdFile): boolean {
    const isMarkdownFile = node.type === 'mdFile' || node.type === 'mdFileTimer';
    const isIndexed = this.isFileIndexed(node);
    return isMarkdownFile && !isIndexed;
  }
  
  // TrackBy function per ottimizzare il rendering dell'albero
  // Usa una chiave stabile che non cambia durante i rename
  trackByPath(index: number, node: MdFile): string {
    // Per i rename, il path della directory rimane lo stesso, solo il nome cambia
    // Usiamo path + level per creare una chiave stabile
    return `${node.path || ''}_${node.level || 0}_${index}`;
  }
  
  // Helper per verificare se un nodo è selezionato
  isNodeSelected(node: MdFile): boolean {
    return this.selectedNode?.fullPath === node.fullPath;
  }
  
  // Espandi manualmente l'albero fino alla landing page
  private expandToLandingPage(targetNode: MdFile): void {
    console.log('🎯 Tentativo espansione verso:', targetNode.fullPath);
    
    // Trova il percorso completo del file nel dataStore
    const pathHierarchy = this.mdFileService.searchMdFileIntoDataStore(this.dataSource.data as MdFile[], targetNode);
    console.log('📁 Gerarchia trovata:', pathHierarchy.map(n => n.name));
    
    if (pathHierarchy && pathHierarchy.length > 0) {
      // Espandi tutte le cartelle padre (escludi l'ultimo che è il file)
      for (let i = pathHierarchy.length - 1; i > 0; i--) {
        const folderToExpand = pathHierarchy[i];
        console.log('🔍 Cercando nodo per espansione:', folderToExpand.name, 'Path:', folderToExpand.path);
        
        // Trova il nodo corrispondente nel treeControl
        const treeNode = this.treeControl.dataNodes.find(node => 
          node.path === folderToExpand.path || 
          node.fullPath === folderToExpand.fullPath ||
          node.relativePath === folderToExpand.relativePath ||
          (node.name === folderToExpand.name && node.type === 'folder')
        );
        
        if (treeNode) {
          console.log('✅ Espandendo nodo:', treeNode.name);
          this.treeControl.expand(treeNode);
        } else {
          console.log('❌ Nodo non trovato nel treeControl per:', folderToExpand.name);
          console.log('📊 Nodi disponibili nel treeControl:', this.treeControl.dataNodes.map(n => ({ name: n.name, path: n.path, type: n.type })));
        }
      }
      
      // Forza un update del tree
      this.changeDetectorRef.detectChanges();
    } else {
      console.log('❌ Nessuna gerarchia trovata per:', targetNode.name);
    }
  }
  
  // Gestione intelligente delle notifiche di indicizzazione
  private showIndexingSnackbar(folderName: string): void {
    
    this.indexedFoldersCount++;
    
    // Se non c'è una snackbar attiva, creane una nuova
    if (!this.currentSnackbarRef) {
      this.currentSnackbarRef = this.snackBar.open(
        `✅ Directory indicizzate: ${this.indexedFoldersCount} | Ultima: ${folderName}`,
        'Chiudi',
        {
          duration: 0, // Non scade automaticamente
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar']
        }
      );
      
      // Cleanup quando viene chiusa manualmente
      this.currentSnackbarRef.afterDismissed().subscribe(() => {
        this.currentSnackbarRef = null;
        this.indexedFoldersCount = 0; // Reset del contatore
      });
    } else {
      // Aggiorna il contenuto della snackbar esistente
      this.updateSnackbarContent(folderName);
    }
  }
  
  private updateSnackbarContent(folderName: string): void {
    if (this.currentSnackbarRef && this.currentSnackbarRef.instance) {
      try {
        // Prova ad aggiornare il messaggio della snackbar esistente
        const newMessage = `✅ Directory indicizzate: ${this.indexedFoldersCount} | Ultima: ${folderName}`;
        
        // Accesso diretto al componente della snackbar
        if (this.currentSnackbarRef.instance.snackBarRef) {
          this.currentSnackbarRef.instance.snackBarRef._data.message = newMessage;
        } else if (this.currentSnackbarRef.instance.data) {
          this.currentSnackbarRef.instance.data.message = newMessage;
        }
        
        // Forza il change detection per aggiornare la vista
        this.changeDetectorRef.detectChanges();
      } catch (error) {
        // Se l'aggiornamento fallisce, chiudi la vecchia e crea una nuova silenziosamente
        this.currentSnackbarRef.dismiss();
        this.currentSnackbarRef = null;
        this.showIndexingSnackbar(folderName);
      }
    }
  }

  // Gestisce la creazione di un nuovo file markdown
  private handleNewMarkdownFileCreated(fileData: any): void {
    
    // STEP 2: Controlla se il file esiste già (caso rinominazione)
    const existingFile = this.findNodeByPath(fileData.fullPath);
    
    if (existingFile) {
      console.log('🔄 [Handler] File rinominato trovato, aggiornando:', existingFile.name, '→', fileData.name);
      
      // Aggiorna le proprietà di indicizzazione invece di aggiungere nuovo nodo
      existingFile.isIndexed = fileData.isIndexed ?? true;
      existingFile.indexingStatus = fileData.indexingStatus ?? 'completed';
      
      // Aggiungi anche al Set di tracking se indicizzato
      if (existingFile.isIndexed) {
        const currentSet = this.indexedFilesSubject.value;
        const newSet = new Set(currentSet);
        newSet.add(existingFile.fullPath);
        this.indexedFilesSubject.next(newSet);
      }
      
      this.changeDetectorRef.markForCheck();
      
      return;
    }
    
    // Converte i dati ricevuti in un oggetto MdFile
    // I dati arrivano dal backend in lowercase, mappiamoli correttamente
    const newMdFile = {
      name: fileData.name,
      fullPath: fileData.fullPath,
      path: fileData.path,
      relativePath: fileData.relativePath,
      type: fileData.type,
      level: fileData.level,
      expandable: fileData.expandable,
      isIndexed: fileData.isIndexed,
      indexingStatus: fileData.indexingStatus,
      childrens: []
    };

    // Costruisce la gerarchia completa per il servizio
    const hierarchyPath = this.buildFileHierarchy(newMdFile);
    
    // Usa il servizio per aggiungere il file al datastore
    this.mdFileService.addNewFile(hierarchyPath);
    
    // IMPORTANTE: Aggiungi il file al Set di tracking dato che è già indicizzato
    const currentSet = this.indexedFilesSubject.value;
    const newSet = new Set(currentSet);
    newSet.add(newMdFile.fullPath);
    this.indexedFilesSubject.next(newSet);
    
    // Mostra una notifica di successo
    this.snackBar.open(
      `✅ Nuovo file creato: ${newMdFile.name}`,
      'Chiudi',
      {
        duration: 3000,
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        panelClass: ['success-snackbar']
      }
    );

    // Forza il refresh del componente
    this.changeDetectorRef.markForCheck();
  }

  // Costruisce la gerarchia completa per un file
  private buildFileHierarchy(newFile: any): any[] {
    const hierarchy = [];
    
    // Se il file è nella root, ritorna solo il file
    if (newFile.level === 0) {
      return [newFile];
    }
    
    // Altrimenti, costruisce la gerarchia delle cartelle parent
    const pathParts = newFile.relativePath.split('\\').filter(part => part.length > 0);
    let currentPath = '';
    
    // Aggiungi le cartelle parent
    for (let i = 0; i < pathParts.length - 1; i++) {
      currentPath += '\\' + pathParts[i];
      const folderNode = {
        name: pathParts[i],
        fullPath: newFile.fullPath.substring(0, newFile.fullPath.indexOf(currentPath)) + currentPath,
        path: currentPath,
        relativePath: currentPath,
        type: 'folder',
        level: i,
        expandable: true,
        childrens: []
      };
      hierarchy.push(folderNode);
    }
    
    // Aggiungi il file alla fine
    hierarchy.push(newFile);
    
    return hierarchy;
  }
  
  // Handler per la cancellazione di file markdown
  private handleMarkdownFileDeleted(fileData: any): void {
    console.log('🗑️ [Handler] File da rimuovere:', fileData.name, 'Path:', fileData.fullPath);
    
    // Trova il nodo da rimuovere
    const nodeToDelete = this.findNodeByPath(fileData.fullPath);
    
    if (!nodeToDelete) {
      console.log('⚠️ [Handler] Nodo non trovato nel tree:', fileData.fullPath);
      return;
    }
    
    console.log('🎯 [Handler] Nodo trovato, rimozione in corso:', nodeToDelete.name);
    
    // Rimuovi il file dal datastore usando il servizio
    this.mdFileService.recursiveDeleteFileFromDataStore(nodeToDelete);
    
    // Rimuovi dall'indice se presente
    const currentSet = this.indexedFilesSubject.value;
    if (currentSet.has(fileData.fullPath)) {
      const newSet = new Set(currentSet);
      newSet.delete(fileData.fullPath);
      this.indexedFilesSubject.next(newSet);
    }
    
    // Forza il refresh del componente
    this.changeDetectorRef.markForCheck();
    
    console.log('✅ [Handler] File rimosso dal tree:', fileData.name);
  }
  
  // Handler per forzare l'aggiornamento di file rinominati (Rule #1 fix)
  public handleRule1ForceUpdate(filePath: string): void {
    const foundNode = this.findNodeByPath(filePath);
    if (!foundNode) {
      return;
    }
    
    // Aggiorna le proprietà di indicizzazione del nodo
    foundNode.isIndexed = true;
    foundNode.indexingStatus = 'completed';
    
    // Aggiorna il Set di tracking con il nuovo fullPath
    const currentSet = new Set(this.indexedFilesSubject.value);
    currentSet.add(foundNode.fullPath);
    this.indexedFilesSubject.next(currentSet);
    
    // Forza change detection per aggiornare immediatamente il template
    this.changeDetectorRef.detectChanges();
  }

  ngOnDestroy(): void {
    // Pulisci il timer se esiste
    if (this.updateTimer) {
      clearTimeout(this.updateTimer);
    }
    
    // Chiudi snackbar attiva
    if (this.currentSnackbarRef) {
      this.currentSnackbarRef.dismiss();
    }
  }

}
