import { CollectionViewer, DataSource, SelectionChange } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, Inject, Injectable, OnInit, ViewChild, HostListener, ElementRef } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatMenuTrigger } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BehaviorSubject, merge, Observable, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { NewDirectoryComponent } from '../new-directory/new-directory.component';
import { IFileInfoNode } from '../../../md-explorer/models/IFileInfoNode';
import { MdFile } from '../../../md-explorer/models/md-file';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { ShowFileMetadata, BreadcrumbSegment, NewDirectoryDialogData } from './show-file-metadata';
import { SpecialFolder, Drive, FileExplorerState } from './file-explorer.models';



// IFileInfoNode è interfaccia
// MdFile è la classe -> DynamicFlatNode


/**
 * Database for dynamic data. When expanding a node in the tree, the data source will need to fetch
 * the descendants data from the database.
 */
@Injectable({ providedIn: 'root' })
export class DynamicDatabase {

  constructor(private mdFileService: MdFileService,) {
    this.mdFileService.loadDynFolders('root', 1);

    var md1 = new MdFile('12Folder', 'c:primoFolder', 0, true);
    var md2 = new MdFile('2Folder', 'c:primoFoldersottoFolder', 1, true);
    var md3 = new MdFile('3Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
    var md4 = new MdFile('4Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
    var md5 = new MdFile('5Folder', 'c:cuccu', 3, false);
    this.dataMap.set(md1, [md2]);
    this.dataMap.set(md2, [md3, md4]);
    //this.dataMap.set(md3, [md4]);
    this.dataMap.set(md4, [md5]);


    var test = this.dataMap.get(md1);
    this.rootLevelNodes = [md1];
  }

  dataMap = new Map<MdFile, MdFile[]>();
  rootLevelNodes: MdFile[];

  /** Initial data from database */
  initialData(): MdFile[] {
    return this.rootLevelNodes;
  }

  getChildren(node: MdFile): MdFile[] | undefined {

    var test = this.dataMap.get(node);
    return test;
  }

  isExpandable(node: MdFile): boolean {
    return this.dataMap.has(node);
  }
}

class DynamicDataSource implements DataSource<MdFile> {

  dataChange = new BehaviorSubject<MdFile[]>([]);

  get data(): MdFile[] { return this.dataChange.value; }
  set data(value: MdFile[]) {
    this._treeControl.dataNodes = value;
    this.dataChange.next(value);
  }

  constructor(private _treeControl: FlatTreeControl<MdFile>,
    private _database: DynamicDatabase,
    private _mdFileService: MdFileService,
    private baseStart: string,
    private typeOfSelection: string) {
    this.data = _database.initialData();
    console.log("constructor-> this.typeOfSelection " + this.typeOfSelection);
    this._mdFileService.loadDocumentFolder(baseStart, 0, this.typeOfSelection).subscribe(_ => {// 'root'
      this.data = _;
    });


  }

  connect(collectionViewer: CollectionViewer): Observable<MdFile[]> {
    this._treeControl.expansionModel.changed.subscribe(change => {
      if ((change as SelectionChange<MdFile>).added ||
        (change as SelectionChange<MdFile>).removed) {
        this.handleTreeControl(change as SelectionChange<MdFile>);
      }
    });

    return merge(collectionViewer.viewChange, this.dataChange).pipe(map(() => this.data));
  }

  disconnect(collectionViewer: CollectionViewer): void { }

  /** Handle expand/collapse behaviors */
  handleTreeControl(change: SelectionChange<MdFile>) {
    if (change.added) {
      change.added.forEach(node => this.toggleNode(node, true));
    }
    if (change.removed) {
      change.removed.slice().reverse().forEach(node => this.toggleNode(node, false));
    }
  }

  /**
   * Toggle the node, remove from display list
   */
  toggleNode(node: MdFile, expand: boolean) {
    
    this._mdFileService.loadDocumentFolder(node.path, node.level + 1, this.typeOfSelection).subscribe(_ => {

      const children = _;
      const index = this.data.indexOf(node);

      if (!children || index < 0) { // If no children, or cannot find the node, no op
        return;
      }

      node.isLoading = true;

      setTimeout(() => {
        if (expand) {
          const nodes = children; // punto per fare chiamata remota
          this.data.splice(index + 1, 0, ...nodes);
        } else {
          let count = 0;
          for (let i = index + 1; i < this.data.length
            && this.data[i].level > node.level; i++, count++) { }
          this.data.splice(index + 1, count);
        }

        // notify the change
        this.dataChange.next(this.data);
        node.isLoading = false;
      });
    });
  }

  refreshNode(node: MdFile) {
    
    this._mdFileService.loadDocumentFolder(node.path, node.level + 1,this.typeOfSelection).subscribe(children => {
      const index = this.data.indexOf(node);
      let count = 0;
      for (let i = index + 1; i < this.data.length
        && this.data[i].level > node.level; i++, count++) { }
      this.data.splice(index + 1, count); // toglie i nodi figlio
      const nodes = children;
      this.data.splice(index + 1, 0, ...nodes); // mette i nuovi nodi
      this.dataChange.next(this.data);
    });
  }
}


@Component({
  selector: 'app-show-file-system',
  templateUrl: './show-file-system.component.html',
  styleUrls: ['./show-file-system.component.scss']
})
export class ShowFileSystemComponent implements OnInit {

  public title: string = "Document's Folder";

  // Existing properties for context menu
  menuTopLeftPosition = { x: 0, y: 0 }
  @ViewChild(MatMenuTrigger, { static: true }) matMenuTrigger: MatMenuTrigger;
  activeNode: any;
  folder: {
    name: string,
    path: string
  }

  // New properties for file explorer
  specialFolders: SpecialFolder[] = [];
  drives: Drive[] = [];
  networkShares: any[] = [];
  currentPath: string = '';
  displayPath: string = '';
  currentItems: MdFile[] = [];
  filteredItems: MdFile[] = [];
  searchFilter: string = '';
  navigationHistory: string[] = [];
  isLoading: boolean = false;

  // Performance optimization: caching
  private folderCache = new Map<string, { data: MdFile[], timestamp: number }>();
  private readonly CACHE_DURATION = 30000; // 30 seconds

  // NEW: Breadcrumb navigation
  public pathSegments: BreadcrumbSegment[] = [];

  // NEW: Search filter tracking
  private filterAppliedToPath: string = '';

  // NEW: Context menu and hover tracking
  public hoveredNode: MdFile | null = null;
  public contextMenuNode: MdFile | null = null;

  // NEW: ViewChild for filter input
  @ViewChild('filterInput') filterInput: ElementRef;

  // Legacy properties (manteniamo per compatibilità)
  getLevel = (node: MdFile) => node.level;
  isExpandable = (node: MdFile) => node.expandable;
  treeControl: FlatTreeControl<MdFile>;
  dataSource: DynamicDataSource;
  hasChild = (_: number, _nodeData: MdFile) => _nodeData.expandable;

  constructor(
    @Inject(MAT_DIALOG_DATA) public baseStart: ShowFileMetadata,
    private database: DynamicDatabase,
    public dialog: MatDialog,
    private mdFileService: MdFileService,
    private dialogRef: MatDialogRef<ShowFileSystemComponent>,
    private snackBar: MatSnackBar) {

    // Inizializza legacy tree control per compatibilità
    this.treeControl = new FlatTreeControl<MdFile>(this.getLevel, this.isExpandable);
    let start = this.baseStart.start == null ? 'root' : this.baseStart.start;
    this.title = this.baseStart.title;
    this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService, start, baseStart.typeOfSelection);
  }

  createDirectoryOn(node: MdFile) {
    if (node == null) {
      node = new MdFile("root", "root", 0, false);
      node.fullPath = "root";
    }
    let dialogRef = this.dialog.open(NewDirectoryComponent, {
      width: '300px',
      data: node,
    });

    dialogRef.afterClosed().subscribe(_ => {
      this.dataSource.refreshNode(node);
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

  ngOnInit(): void {
    this.folder = { name: "<select project>", path: "" };
    this.filteredItems = [];
    this.loadInitialData();
  }

  private loadInitialData(): void {
    this.isLoading = true;

    // Carica special folders, drives e network shares
    forkJoin({
      folders: this.mdFileService.getSpecialFolders(),
      drives: this.mdFileService.getDrives(),
      networkShares: this.mdFileService.getNetworkShares()
    }).subscribe({
      next: ({folders, drives, networkShares}) => {
        this.specialFolders = folders;
        this.drives = drives;
        this.networkShares = networkShares;

        // Naviga alla cartella iniziale
        const initialPath = this.baseStart.start === 'root' ? 'project' : this.baseStart.start;
        const initialFolder = this.specialFolders.find(f => f.name.toLowerCase() === initialPath?.toLowerCase());
        if (initialFolder) {
          this.navigateToFolder(initialFolder.path);
        } else {
          this.navigateToFolder(this.specialFolders[0]?.path || '');
        }
      },
      error: (error) => {
        console.error('Error loading initial data:', error);
        this.isLoading = false;
      }
    });
  }

  public navigateToFolder(path: string): void {
    if (!path || path === this.currentPath) return;

    // NEW: Reset automatico del filtro quando si naviga
    if (this.searchFilter && this.filterAppliedToPath !== path) {
      this.searchFilter = '';
      this.filterAppliedToPath = '';
    }

    // Aggiungi il path corrente alla history
    if (this.currentPath) {
      this.navigationHistory.push(this.currentPath);
    }

    this.currentPath = path;
    this.displayPath = this.formatDisplayPath(path);
    this.buildBreadcrumb(path); // NEW: Costruisci breadcrumb
    this.loadFolderContent(path);
  }

  public navigateUp(): void {
    if (this.navigationHistory.length > 0) {
      const previousPath = this.navigationHistory.pop()!;
      this.currentPath = previousPath;
      this.displayPath = this.formatDisplayPath(previousPath);
      this.buildBreadcrumb(previousPath); // FIX: Aggiorna breadcrumb
      this.loadFolderContent(previousPath);
    }
  }

  private loadFolderContent(path: string): void {
    // Check cache first
    const cached = this.folderCache.get(path);
    const now = Date.now();
    
    if (cached && (now - cached.timestamp) < this.CACHE_DURATION) {
      this.currentItems = cached.data;
      this.applyFilter(); // Apply current filter to cached data
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.currentItems = [];

    this.mdFileService.loadDocumentFolder(path, 0, this.baseStart.typeOfSelection)
      .subscribe({
        next: (items) => {
          const data = items || [];
          this.currentItems = data;
          this.applyFilter(); // Apply current filter to new data
          
          // Cache the result
          this.folderCache.set(path, { data, timestamp: now });
          
          // Clean old cache entries (keep cache size manageable)
          this.cleanOldCacheEntries();
          
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading folder content:', error);
          this.currentItems = [];
          this.filteredItems = [];
          this.isLoading = false;
        }
      });
  }

  private cleanOldCacheEntries(): void {
    const now = Date.now();
    for (const [key, value] of this.folderCache.entries()) {
      if ((now - value.timestamp) > this.CACHE_DURATION) {
        this.folderCache.delete(key);
      }
    }
    
    // Limit cache size to prevent memory issues
    if (this.folderCache.size > 50) {
      const entries = Array.from(this.folderCache.entries());
      entries.sort((a, b) => a[1].timestamp - b[1].timestamp);
      
      // Keep only the 30 most recent entries
      for (let i = 0; i < entries.length - 30; i++) {
        this.folderCache.delete(entries[i][0]);
      }
    }
  }

  private formatDisplayPath(path: string): string {
    // Accorcia il path per la visualizzazione
    if (path.length > 50) {
      return '...' + path.substring(path.length - 47);
    }
    return path;
  }

  public onItemClick(item: MdFile): void {
    if (this.baseStart.typeOfSelection === 'FoldersAndFiles' && item.type === 'folder') {
      // Per selezione file, le cartelle servono solo per navigare
      // Non selezionare la cartella
      return;
    }
    
    this.activeNode = item;
    
    if (item.type === 'folder') {
      // Per le cartelle, seleziona ma non naviga (single click)
      this.getFolder(item);
    } else {
      // Per i file, seleziona direttamente
      this.getFolder(item);
    }
  }

  public onItemDoubleClick(item: MdFile): void {
    if (item.type === 'folder') {
      // Double click su cartella: naviga
      this.navigateToFolder(item.fullPath || item.path);
    }
  }

  public getFolder(node: IFileInfoNode) {
    this.folder.name = node.name;
    this.folder.path = node.fullPath || node.path;
  }

  // Legacy method mantained for compatibility
  public openFolderOn(item: any): void {
    if (item && item.fullPath) {
      this.mdFileService.openFolderOnFileExplorer(item).subscribe({
        next: () => console.log('Folder opened in explorer'),
        error: (error) => console.error('Error opening folder:', error)
      });
    }
  }

  public closeDialog() {
    // Determina quale path usare in base al tipo di selezione
    let selectedPath: string;
    
    if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
      // Per file: usa sempre folder.path (che viene aggiornato quando si seleziona un file)
      selectedPath = this.folder.path;
    } else {
      // Per cartelle: priorità a folder.path (cartella selezionata), 
      // altrimenti usa currentPath (cartella in cui stiamo navigando)
      selectedPath = this.folder.path || this.currentPath;
    }
    
    this.dialogRef.close({ event: 'open', data: selectedPath });
  }

  // TrackBy functions for performance optimization
  public trackByPath(index: number, item: SpecialFolder | Drive): string {
    return item.path;
  }

  public trackByItem(index: number, item: MdFile): string {
    return item.fullPath || item.path || item.name;
  }

  // Filter functionality
  public onFilterChange(event: any): void {
    this.searchFilter = event.target.value;

    // NEW: Traccia il path dove è stato applicato il filtro
    if (this.searchFilter && this.searchFilter.trim() !== '') {
      this.filterAppliedToPath = this.currentPath;
    } else {
      this.filterAppliedToPath = '';
    }

    this.applyFilter();
  }

  private applyFilter(): void {
    let filtered = [...this.currentItems];

    // Filtro per nome
    if (this.searchFilter && this.searchFilter.trim() !== '') {
      const filter = this.searchFilter.toLowerCase().trim();
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(filter)
      );
    }

    // FILTRO PER ESTENSIONI RIMOSSO - mostra tutti i file
    // Commentato per permettere la visualizzazione di tutti i file disponibili
    // Se necessario in futuro, l'utente può aggiungere una configurazione

    this.filteredItems = filtered;
  }
  
  private getFileExtension(filename: string): string {
    const lastDot = filename.lastIndexOf('.');
    return lastDot > 0 ? filename.substring(lastDot) : '';
  }

  // Selection button text
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

  // Validation for selection
  public canSelectItem(): boolean {
    if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
      // Solo file possono essere selezionati
      return this.activeNode && this.activeNode.type !== 'folder' && !!this.folder.path;
    }
    
    // Per selezione cartelle: può selezionare la cartella corrente o una cartella selezionata
    // Se c'è una cartella selezionata (activeNode), usa quella
    // Altrimenti usa la cartella corrente in cui si sta navigando
    if (this.activeNode && this.activeNode.type === 'folder') {
      return true;
    }
    
    // Se non c'è activeNode ma stiamo navigando in una cartella, possiamo selezionare la cartella corrente
    return !!(this.currentPath && this.currentPath.length > 0);
  }

  // Check if item is selectable
  public isItemSelectable(item: MdFile): boolean {
    if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
      return item.type !== 'folder';
    }
    return item.type === 'folder';
  }

  // Accessibility helper
  public getItemAriaLabel(item: MdFile): string {
    const type = item.type === 'folder' ? 'folder' : 'file';
    return `${type} ${item.name}. ${item.type === 'folder' ? 'Double click to open' : 'Click to select'}`;
  }

  // ============================================
  // NEW METHODS FOR UX IMPROVEMENTS
  // ============================================

  /**
   * Costruisce il breadcrumb path cliccabile
   * Cross-platform: gestisce sia / che \ come separatori
   */
  private buildBreadcrumb(path: string): void {
    this.pathSegments = [];

    if (!path) return;

    // Normalizza i separatori per la gestione cross-platform
    const normalizedPath = path.replace(/\\/g, '/');

    // Trova special folder come primo elemento
    const specialFolder = this.specialFolders.find(f => {
      const normalizedFolderPath = f.path.replace(/\\/g, '/');
      return normalizedPath.startsWith(normalizedFolderPath) || normalizedPath === normalizedFolderPath;
    });

    if (specialFolder) {
      this.pathSegments.push({
        name: specialFolder.name,
        fullPath: specialFolder.path,
        icon: specialFolder.icon
      });

      // Aggiungi sottocartelle relative
      const normalizedSpecialPath = specialFolder.path.replace(/\\/g, '/');
      let relativePath = normalizedPath.substring(normalizedSpecialPath.length);

      // Rimuovi il separatore iniziale se presente
      if (relativePath.startsWith('/')) {
        relativePath = relativePath.substring(1);
      }

      if (relativePath) {
        const parts = relativePath.split('/').filter(p => p);
        let currentPath = specialFolder.path;

        parts.forEach(part => {
          // Usa il separatore del sistema operativo originale
          const separator = specialFolder.path.includes('\\') ? '\\' : '/';
          currentPath = `${currentPath}${separator}${part}`;
          this.pathSegments.push({
            name: part,
            fullPath: currentPath
          });
        });
      }
    } else {
      // Fallback per path normali (senza special folder)
      const parts = normalizedPath.split('/').filter(p => p);
      let currentPath = '';

      parts.forEach((part, index) => {
        if (index === 0) {
          // Prima parte (es: C:, D:, /home, etc.)
          currentPath = part;
          // Ripristina \ se era nel path originale
          if (path.includes('\\')) {
            currentPath = part;
          }
        } else {
          // Usa il separatore appropriato
          const separator = path.includes('\\') ? '\\' : '/';
          currentPath = `${currentPath}${separator}${part}`;
        }

        this.pathSegments.push({
          name: part,
          fullPath: currentPath
        });
      });
    }
  }

  /**
   * Naviga a un segmento specifico del breadcrumb
   */
  public navigateToBreadcrumb(segment: BreadcrumbSegment): void {
    this.navigateToFolder(segment.fullPath);
  }

  /**
   * Copia il path corrente negli appunti
   */
  public async copyPathToClipboard(path?: string): Promise<void> {
    const pathToCopy = path || this.currentPath;

    try {
      if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(pathToCopy);
        this.showSuccessNotification('Path copied to clipboard');
      } else {
        // Fallback per ambienti non sicuri
        this.fallbackCopyToClipboard(pathToCopy);
        this.showSuccessNotification('Path copied to clipboard');
      }
    } catch (error) {
      console.error('Failed to copy path:', error);
      this.snackBar.open('Failed to copy path', 'Close', { duration: 3000 });
    }
  }

  /**
   * Fallback per copia negli appunti (cross-browser compatibility)
   */
  private fallbackCopyToClipboard(text: string): void {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
      document.execCommand('copy');
    } catch (error) {
      console.error('Fallback copy failed:', error);
    }

    document.body.removeChild(textArea);
  }

  /**
   * Getter per verificare se il filtro è attivo
   */
  public get isFilterActive(): boolean {
    return !!(this.searchFilter && this.searchFilter.trim() !== '');
  }

  /**
   * Pulisce il filtro di ricerca
   */
  public clearFilter(): void {
    this.searchFilter = '';
    this.filterAppliedToPath = '';
    this.applyFilter();
  }

  /**
   * Aggiorna createDirectoryOn con contesto migliorato
   */
  public createDirectoryOnImproved(node: MdFile | null): void {
    if (node == null) {
      node = new MdFile("root", "root", 0, false);
      node.fullPath = this.currentPath || "root";
    }

    // Prepara i dati con contesto completo
    const dialogData: NewDirectoryDialogData = {
      parentNode: node,
      parentPath: node.fullPath || node.path,
      parentName: node.name,
      isRoot: node.name === "root",
      currentPath: this.currentPath
    };

    const dialogRef = this.dialog.open(NewDirectoryComponent, {
      width: '500px',
      data: dialogData,
      disableClose: false,
      autoFocus: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Refresh node nel tree view legacy
        this.dataSource.refreshNode(node);

        // Refresh anche la vista corrente
        this.refreshCurrentFolder();

        // Mostra notifica di successo
        this.showSuccessNotification(`Folder "${result.name}" created successfully`);
      }
    });
  }

  /**
   * Cleanup quando il context menu si chiude
   */
  public onMenuClosed(): void {
    setTimeout(() => {
      this.contextMenuNode = null;
    }, 200);
  }

  /**
   * Aggiorna l'onRightClick con tracking del context menu
   */
  public onRightClickImproved(event: MouseEvent, item: MdFile | null): void {
    event.preventDefault();

    // Normalizza item per root
    if (item == null) {
      item = new MdFile("root", "root", 0, false);
      item.fullPath = this.currentPath || "root";
    }

    this.contextMenuNode = item;

    // Posiziona menu
    this.menuTopLeftPosition.x = event.clientX;
    this.menuTopLeftPosition.y = event.clientY;

    this.matMenuTrigger.menuData = { item: item };
    this.matMenuTrigger.openMenu();
  }

  /**
   * Refresh della cartella corrente (invalida cache)
   */
  public refreshCurrentFolder(): void {
    this.folderCache.delete(this.currentPath);
    this.loadFolderContent(this.currentPath);
  }

  /**
   * Refresh di una cartella specifica
   */
  public refreshFolder(item: MdFile): void {
    const pathToRefresh = item.fullPath || item.path;
    this.folderCache.delete(pathToRefresh);

    if (pathToRefresh === this.currentPath) {
      this.loadFolderContent(pathToRefresh);
    }
  }

  /**
   * Mostra notifica di successo
   */
  private showSuccessNotification(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['success-snackbar']
    });
  }

  /**
   * Focus sull'input del filtro
   */
  private focusFilter(): void {
    if (this.filterInput && this.filterInput.nativeElement) {
      this.filterInput.nativeElement.focus();
    }
  }

  /**
   * Keyboard shortcuts handler
   * Cross-platform: usa Ctrl su Windows/Linux, Cmd su Mac
   */
  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent): void {
    // Ctrl/Cmd + F: Focus sul filtro
    if ((event.ctrlKey || event.metaKey) && event.key === 'f') {
      event.preventDefault();
      this.focusFilter();
    }

    // Ctrl/Cmd + N: Nuova cartella nella cartella corrente
    if ((event.ctrlKey || event.metaKey) && event.key === 'n') {
      event.preventDefault();
      this.createDirectoryOnImproved(null);
    }

    // Escape: Clear filter
    if (event.key === 'Escape' && this.isFilterActive) {
      event.preventDefault();
      this.clearFilter();
    }

    // Alt + Up: Navigate up
    if (event.altKey && event.key === 'ArrowUp') {
      event.preventDefault();
      this.navigateUp();
    }

    // F5: Refresh
    if (event.key === 'F5') {
      event.preventDefault();
      this.refreshCurrentFolder();
    }
  }

}
