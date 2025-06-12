import { CollectionViewer, DataSource, SelectionChange } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, Inject, Injectable, OnInit, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatMenuTrigger } from '@angular/material/menu';
import { BehaviorSubject, merge, Observable, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { NewDirectoryComponent } from '../new-directory/new-directory.component';
import { IFileInfoNode } from '../../../md-explorer/models/IFileInfoNode';
import { MdFile } from '../../../md-explorer/models/md-file';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { ShowFileMetadata } from './show-file-metadata';
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
    private dialogRef: MatDialogRef<ShowFileSystemComponent>) {
    
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

    // Carica special folders e drives
    forkJoin({
      folders: this.mdFileService.getSpecialFolders(),
      drives: this.mdFileService.getDrives()
    }).subscribe({
      next: ({folders, drives}) => {
        this.specialFolders = folders;
        this.drives = drives;

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

    // Aggiungi il path corrente alla history
    if (this.currentPath) {
      this.navigationHistory.push(this.currentPath);
    }

    this.currentPath = path;
    this.displayPath = this.formatDisplayPath(path);
    this.loadFolderContent(path);
  }

  public navigateUp(): void {
    if (this.navigationHistory.length > 0) {
      const previousPath = this.navigationHistory.pop()!;
      this.currentPath = previousPath;
      this.displayPath = this.formatDisplayPath(previousPath);
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

}
