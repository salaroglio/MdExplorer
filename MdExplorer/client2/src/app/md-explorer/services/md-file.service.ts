import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { MdFile } from '../models/md-file';
import { IDocumentSettings } from './Types/IDocumentSettings';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { SpecialFolder, Drive } from '../../commons/components/show-file-system/file-explorer.models';
import { AppStoreService } from './app-store.service';

@Injectable({
  providedIn: 'root'
})
export class MdFileService {

  private _whatDisplayForToolbar: BehaviorSubject<string>;
  public _mdFiles: BehaviorSubject<MdFile[]>;
  public _mdDynFolderDocument: BehaviorSubject<MdFile[]>;
  public _serverSelectedMdFile: BehaviorSubject<MdFile[]>;
  private _navigationArray: MdFile[] = [];// deve morire
  private _selectedMdFileFromToolbar: BehaviorSubject<MdFile[]>;
  private _selectedMdFileFromSideNav: BehaviorSubject<MdFile>;
  private _selectedDirectoryFromNewDirectory: BehaviorSubject<MdFile>;
  private _revealInTree = new Subject<MdFile>();

  private dataStore: {
    mdFiles: MdFile[]
    mdFoldersDocument: MdFile[]
    mdDynFolderDocument: MdFile[]
    serverSelectedMdFile: MdFile[]
  }
  constructor(private http: HttpClient,
    private mdServerMessages: MdServerMessagesService,
    private injector: Injector,
    private appStoreService: AppStoreService) {

    var defaultSelectedMdFile = [];
    this.dataStore = {
      mdFiles: [],
      mdFoldersDocument: [],
      mdDynFolderDocument: [],
      serverSelectedMdFile: defaultSelectedMdFile
    };

    this._mdFiles = new BehaviorSubject<MdFile[]>([]);
    this._mdDynFolderDocument = new BehaviorSubject<MdFile[]>([]);
    this._serverSelectedMdFile = new BehaviorSubject<MdFile[]>([]);
    this._selectedMdFileFromToolbar = new BehaviorSubject<MdFile[]>([]);
    this._selectedMdFileFromSideNav = new BehaviorSubject<MdFile>(null);
    this._selectedDirectoryFromNewDirectory = new BehaviorSubject<MdFile>(null);
    this._whatDisplayForToolbar = new BehaviorSubject<string>('block');

    // Subscribe to Git branch switch events to refresh tree
    this.mdServerMessages.gitBranchSwitched$.subscribe((data) => {
      console.log('🌳 Git branch switched - refreshing tree. Files indexed:', data.fileCount);
      // Use loadAll() to properly update dataStore and notify subscribers
      this.loadAll(null, null);
    });

    // Subscribe to Git pull events to refresh tree
    this.mdServerMessages.gitPullRefreshed$.subscribe((data) => {
      console.log('🌳 Git pull completed - refreshing tree. Files changed:', data.fileCount);
      this.loadAll(null, null);
    });

    // Subscribe to project changing events - clear data to show skeleton loader
    // Use setTimeout to avoid circular dependency issues
    setTimeout(() => {
      const { ProjectsService } = require('./projects.service');
      const projectsService = this.injector.get(ProjectsService);
      projectsService.projectChanging$.subscribe(() => {
        console.log('🔄 Project changing - clearing tree data for skeleton');
        this.dataStore.mdFiles = [];
        this._mdFiles.next([]);
      });
    }, 0);
  }

  get whatDisplayForToolbar(): Observable<string> {
    return this._whatDisplayForToolbar.asObservable();
  }

  setWhatDisplayForToolbar(value: string) {
    this._whatDisplayForToolbar.next(value);
  }

  get mdFiles(): Observable<MdFile[]> {
    return this._mdFiles.asObservable();
  }

  get mdDynFolderDocument(): Observable<MdFile[]> {
    return this._mdDynFolderDocument.asObservable();
  }

  get serverSelectedMdFile(): Observable<MdFile[]> {
    return this._serverSelectedMdFile.asObservable();
  }

  get selectedMdFileFromToolbar(): Observable<MdFile[]> {
    return this._selectedMdFileFromToolbar.asObservable();
  }

  get selectedMdFileFromSideNav(): Observable<MdFile> {
    return this._selectedMdFileFromSideNav.asObservable();
  }

  /**
   * Get the current value of selected file (for synchronous access)
   */
  get currentSelectedMdFile(): MdFile | null {
    return this._selectedMdFileFromSideNav.value;
  }

  get revealInTree$(): Observable<MdFile> {
    return this._revealInTree.asObservable();
  }

  requestRevealInTree(file: MdFile): void {
    this._revealInTree.next(file);
  }

  get selectedDirectoryFromNewDirectory(): Observable<MdFile> {
    return this._selectedDirectoryFromNewDirectory.asObservable();
  }


  // breadcrumb
  get navigationArray(): MdFile[] {
    return this._navigationArray;
  }

  set navigationArray(mdFile: MdFile[]) {
    this._navigationArray = mdFile;
  }

  moveMdFile(mdFile: MdFile, pathDestination: string) {
    const url = '../api/mdfiles/MoveMdFile';
    return this.http.post<any>(url, {
      sourceRelativePath: mdFile.relativePath,
      sourceFileName: mdFile.name,
      destinationPath: pathDestination
    });
  }

  openInheritingTemplateWord(InheringTemplate: string) {
    const url = '../api/mdFiles/openinheritingtemplateWord';
    return this.http.post<any>(url, { templateName: InheringTemplate });
  }

  opencustomwordtemplate(mdFile: MdFile) {
    const url = '../api/mdFiles/opencustomwordtemplate';
    return this.http.post<any>(url, mdFile);
  }

  setDocumentSettings(documentDescriptor: IDocumentSettings , mdFile:MdFile) {
    const url = '../api/mdFiles/setdocumentsettings';
    return this.http.post<any>(url, { documentDescriptor, mdFile });
  }

  getDocumentSettings(mdFile: MdFile): Observable<IDocumentSettings> {
    const url = '../api/mdFiles/getdocumentsettings';
    var params = new HttpParams().set('fullPath', mdFile.fullPath)
    return this.http.get<IDocumentSettings>(url, { params });
  }


  // This function adds a new file,
  // looking for the right position in the
  // folder hierarchy.
  // It assumes that all structures are complete,
  // and the only thing to add is the file itself.
  addNewFile(data: MdFile[]) {
    // searching directories    
    const currentItem = data[0];
    
    // Assicuriamoci che le proprietà di indicizzazione siano preservate
    if (currentItem.type === 'mdFile' || currentItem.type === 'mdFileTimer') {
      // Preserva le proprietà esistenti o imposta i default
      currentItem.isIndexed = currentItem.isIndexed ?? true; // Default true per nuovi file
      currentItem.indexingStatus = currentItem.indexingStatus ?? 'completed';
    }
    
    const currentFolder = this.dataStore.mdFiles.find(item => item.fullPath == currentItem.fullPath);

    if (currentFolder) {
      this.recursiveSearchFolder(data, 0, currentFolder);
    } else {
      // The file is in the root
      const dummyItem = this.dataStore.mdFiles.pop();
      this.dataStore.mdFiles.push(currentItem, dummyItem); // Simplified push operation
      this._mdFiles.next([...this.dataStore.mdFiles]);
    }
  }


  // This function adds new directories
  // if one or more on the file path are missing.
  // At the end of the process, it will call the classic addNewFile method.
  addNewDirectoryExtended(folders: MdFile[]) {

    let currentfolder = [];
    folders.forEach((folder, index) => {
      const dataFound: MdFile[] = [];
      this.recursiveSearch(this.dataStore.mdFiles, folder, dataFound);
      currentfolder.push(folder);
      if (dataFound.length === 0) {
        this.addNewDirectory(currentfolder);
      }

    });


  }

  /**
   * Adds a new file to the datastore, creating any missing parent directories.
   * This is the correct method to use when a file is created via FileSystemWatcher
   * and its parent directories may not exist in the tree yet.
   *
   * @param hierarchy Array of nodes: [folder1, folder2, ..., file]
   *                  where folders are ordered from root to deepest, and file is last
   * @returns Subject<void> that emits when the operation is complete and tree is updated
   */
  addNewFileWithDirectories(hierarchy: MdFile[]): Subject<void> {
    console.log('🔧 [addNewFileWithDirectories] INIZIO - hierarchy:', JSON.stringify(hierarchy, null, 2));
    const completed = new Subject<void>();

    if (!hierarchy || hierarchy.length === 0) {
      console.log('⚠️ [addNewFileWithDirectories] hierarchy vuoto, esco');
      setTimeout(() => {
        completed.next();
        completed.complete();
      }, 0);
      return completed;
    }

    // Separate directories from the file (file is the last element with type != 'folder')
    const directories: MdFile[] = [];
    let file: MdFile | null = null;

    for (const node of hierarchy) {
      if (node.type === 'folder') {
        directories.push(node);
      } else {
        file = node;
      }
    }
    console.log('📁 [addNewFileWithDirectories] directories trovate:', directories.length, 'file:', file?.name);

    // Create missing directories in order (from root to deepest)
    // Build the path incrementally as addNewDirectory expects
    const pathSoFar: MdFile[] = [];

    for (const dir of directories) {
      // Check if this directory already exists in the datastore
      const dataFound: MdFile[] = [];
      this.recursiveSearch(this.dataStore.mdFiles, dir, dataFound);
      console.log('🔍 [addNewFileWithDirectories] Cerco dir:', dir.name, 'fullPath:', dir.fullPath, '- trovata:', dataFound.length > 0);

      pathSoFar.push(dir);

      if (dataFound.length === 0) {
        // Directory doesn't exist, create it
        // addNewDirectory expects the full path array from root
        console.log('➕ [addNewFileWithDirectories] Creo directory:', dir.name);
        this.addNewDirectory([...pathSoFar]);
      }
    }

    // Now add the file - at this point all parent directories exist
    if (file) {
      // Ensure indexing properties are set
      file.isIndexed = file.isIndexed ?? true;
      file.indexingStatus = file.indexingStatus ?? 'completed';

      // Use the full hierarchy for addNewFile so it can navigate to the correct parent
      console.log('📄 [addNewFileWithDirectories] Aggiungo file:', file.name);
      this.addNewFile(hierarchy);
    }

    // Emit completion after Angular has a chance to process the changes
    // Use setTimeout to ensure change detection has run
    setTimeout(() => {
      completed.next();
      completed.complete();
    }, 50);

    return completed;
  }

  // This function adds a new directory.
  // Assuming that all directories/folders are already present,
  // and there is just one to add consequently to
  // what already exists in the store.
  addNewDirectory(data: MdFile[]) {
    //alert(JSON.stringify(data, null, 2));
    // Initialize the current item and mark it as expandable
    const currentItem = data[0];
    currentItem.expandable = true;

    // Search for the directory in the current datastore
    const currentFolder = this.dataStore.mdFiles.find(item => item.fullPath == currentItem.fullPath);

    if (currentFolder) {
      // If found, perform a recursive search to insert the directory
      this.recursiveSearchFolder(data, 0, currentFolder);
    } else {
      // If the directory is in the root, handle the dummy item and reinsert
      const dummyItem = this.dataStore.mdFiles.pop(); // Remove the last item (dummy)
      this.dataStore.mdFiles.push(currentItem, dummyItem); // Add the current item and then the dummy back

      // Notify subscribers of the update
      this._mdFiles.next([...this.dataStore.mdFiles]);
    }
  }




  recursiveSearchFolder(data: MdFile[], i: number, parentFolder: MdFile) {
    
    const currentItem = data[i + 1];
    if (!currentItem) return; // Guard clause
    
    // Assicuriamoci che le proprietà di indicizzazione siano preservate
    if (currentItem.type === 'mdFile' || currentItem.type === 'mdFileTimer') {
      currentItem.isIndexed = currentItem.isIndexed ?? true;
      currentItem.indexingStatus = currentItem.indexingStatus ?? 'completed';
    }
    
    const currentFolder = parentFolder.childrens.find(folder => folder.fullPath == currentItem.fullPath);

    if (currentFolder) {
      this.recursiveSearchFolder(data, i + 1, currentFolder);
    } else {
      parentFolder.childrens.push(currentItem); // Directly use currentItem
      this._mdFiles.next([...this.dataStore.mdFiles]);
    }
  }





  getShallowStructure(): Observable<MdFile[]> {
    return this.http.get<MdFile[]>('../api/mdfiles/GetShallowStructure');
  }

  loadAll(callback: (data: any, objectThis: any) => any, objectThis: any) {
    console.warn('🔴 [DIAG] loadAll() CALLED at:', new Date().toISOString(), '- stack trace:', new Error().stack);
    // Pre-fetch catalog + installed apps for update checks
    this.appStoreService.prefetchCatalogAndInstalled();
    return this.http.get<MdFile[]>('../api/mdfiles/GetShallowStructure')
      .subscribe(data => {
        // Assicuriamo che tutte le proprietà siano definite fin dall'inizio
        this.initializeIndexingProperties(data);
        // Compatta le cartelle annidate (VS Code-style)
        this.compactFolders(data);
        this.dataStore.mdFiles = data;
        this._mdFiles.next([...this.dataStore.mdFiles]);
        if (callback != null) {
          callback(data, objectThis);
        }
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  private initializeIndexingProperties(nodes: any[]): void {
    nodes.forEach(node => {
      // Assicura che le proprietà esistano fin dall'inizio
      if (node.type === 'mdFile' || node.type === 'mdFileTimer') {
        node.isIndexed = node.isIndexed ?? false;
        node.indexingStatus = node.indexingStatus ?? 'idle';
      }
      if (node.childrens && node.childrens.length > 0) {
        this.initializeIndexingProperties(node.childrens);
      }
    });
  }

  /**
   * Compatta catene di cartelle con un solo figlio cartella (VS Code-style)
   * Es: src/main/java/com viene mostrato come "src / main / java / com" su una riga
   */
  private compactFolders(nodes: MdFile[]): void {
    nodes.forEach(node => this.compactSingleNode(node));
  }

  private compactSingleNode(node: MdFile): void {
    if (node.type !== 'folder' || !node.childrens?.length) {
      return;
    }

    // Raccogli i segmenti della catena
    const segments: { name: string; fullPath: string; level: number }[] = [
      { name: node.name, fullPath: node.fullPath, level: node.level }
    ];
    let current = node;
    let lastCompactedLevel = node.level;

    // Segui la catena finché c'è esattamente 1 figlio che è una cartella
    while (current.childrens?.length === 1 && current.childrens[0].type === 'folder') {
      current = current.childrens[0] as MdFile;
      lastCompactedLevel++;
      segments.push({ name: current.name, fullPath: current.fullPath, level: lastCompactedLevel });
    }

    // Se abbiamo compresso almeno 2 livelli
    if (segments.length > 1) {
      node.isCompacted = true;
      node.compactedPath = segments.map(s => s.name).join(' / ');
      node.compactedSegments = segments;
      // I figli diventano quelli dell'ultimo nodo compresso
      node.childrens = current.childrens as MdFile[];
      // La riga compattata rappresenta il segmento FINALE della catena: l'icona
      // TOC e openTocFile() operano sull'ultimo segmento, quindi hasToc deve
      // riflettere quello, non il primo segmento (dove resta congelato).
      node.hasToc = current.hasToc;
      // Il fullPath del nodo diventa quello dell'ultimo segmento per le operazioni di default
      // Ma manteniamo il path originale per la visualizzazione
    }

    // Processa ricorsivamente i figli (che ora sono i figli dell'ultimo nodo compresso se compattato)
    if (node.childrens?.length) {
      node.childrens.forEach(child => this.compactSingleNode(child as MdFile));
    }
  }

  /**
   * Aggiunge un file direttamente nella cartella parent specificata.
   * Gestisce correttamente le compact folders (VS Code-style).
   * @param file Il nuovo file da aggiungere
   * @param parentFullPath Il fullPath della cartella parent
   * @returns true se il file è stato aggiunto, false se la cartella parent non è stata trovata
   */
  addFileToParent(file: MdFile, parentFullPath: string): boolean {
    // Cerca la cartella parent nel dataStore (incluse compact folders)
    const parentFolder = this.findFolderInDataStore(this.dataStore.mdFiles, parentFullPath);

    if (parentFolder) {
      // Assicura proprietà di indicizzazione
      if (file.type === 'mdFile' || file.type === 'mdFileTimer') {
        file.isIndexed = file.isIndexed ?? true;
        file.indexingStatus = file.indexingStatus ?? 'completed';
      }
      parentFolder.childrens.push(file);
      this._mdFiles.next([...this.dataStore.mdFiles]);
      return true;
    }

    // Controlla se il file va nella root (parentFullPath coincide con la root del progetto)
    // In questo caso parentFullPath non corrisponde a nessuna cartella perché È la root
    if (this.dataStore.mdFiles.length > 0) {
      // Verifica: se nessuna cartella di primo livello ha un fullPath che inizia con parentFullPath,
      // allora parentFullPath non è una cartella nel tree. Ma se il file ha level === 0,
      // è un file root.
      if (file.level === 0) {
        if (file.type === 'mdFile' || file.type === 'mdFileTimer') {
          file.isIndexed = file.isIndexed ?? true;
          file.indexingStatus = file.indexingStatus ?? 'completed';
        }
        const dummyItem = this.dataStore.mdFiles.pop();
        this.dataStore.mdFiles.push(file, dummyItem);
        this._mdFiles.next([...this.dataStore.mdFiles]);
        return true;
      }
    }

    // Step 3: Try breaking a compact folder chain at the parent path
    const brokenNode = this.breakCompactFolderAt(this.dataStore.mdFiles, parentFullPath);
    if (brokenNode) {
      if (file.type === 'mdFile' || file.type === 'mdFileTimer') {
        file.isIndexed = file.isIndexed ?? true;
        file.indexingStatus = file.indexingStatus ?? 'completed';
      }
      brokenNode.childrens.push(file);
      this._mdFiles.next([...this.dataStore.mdFiles]);
      return true;
    }

    // Step 4: Create missing folder hierarchy and insert there
    const createdParent = this.createMissingFolderHierarchy(parentFullPath);
    if (createdParent) {
      if (file.type === 'mdFile' || file.type === 'mdFileTimer') {
        file.isIndexed = file.isIndexed ?? true;
        file.indexingStatus = file.indexingStatus ?? 'completed';
      }
      createdParent.childrens.push(file);
      this._mdFiles.next([...this.dataStore.mdFiles]);
      return true;
    }

    return false;
  }

  /**
   * Cerca ricorsivamente una cartella nel dataStore, gestendo le compact folders.
   * Per le compact folders, controlla se l'ultimo segmento compattato corrisponde
   * al targetFullPath — in quel caso node.childrens è il posto giusto.
   */
  private findFolderInDataStore(nodes: MdFile[], targetFullPath: string): MdFile | null {
    if (!nodes) return null;

    for (const node of nodes) {
      if (node.type !== 'folder') continue;

      // Match diretto sul fullPath del nodo
      if (node.fullPath && node.fullPath.toLowerCase() === targetFullPath.toLowerCase()) {
        return node;
      }

      // Se il nodo è compattato, controlla i segmenti compattati
      if (node.isCompacted && node.compactedSegments) {
        const lastSegment = node.compactedSegments[node.compactedSegments.length - 1];
        if (lastSegment && lastSegment.fullPath.toLowerCase() === targetFullPath.toLowerCase()) {
          // L'ultimo segmento compattato corrisponde: node.childrens punta ai figli
          // della cartella più profonda nella catena compattata
          return node;
        }
        // Controlla anche i segmenti intermedi (non l'ultimo)
        // Se il target corrisponde a un segmento intermedio, la cartella è stata assorbita
        // e non esiste come nodo separato nel tree
        for (let i = 0; i < node.compactedSegments.length - 1; i++) {
          if (node.compactedSegments[i].fullPath.toLowerCase() === targetFullPath.toLowerCase()) {
            // Il target è un segmento intermedio della compattazione
            // Non possiamo inserire qui direttamente, ritorna null per usare il fallback
            return null;
          }
        }
      }

      // Ricerca ricorsiva nei figli
      if (node.childrens && node.childrens.length > 0) {
        const found = this.findFolderInDataStore(node.childrens as MdFile[], targetFullPath);
        if (found) return found;
      }
    }

    return null;
  }

  /**
   * Trova un nodo compattato che contiene targetFullPath come segmento intermedio.
   * Restituisce il nodo compattato e l'indice del segmento, oppure null.
   */
  private findCompactIntermediateNode(
    nodes: MdFile[],
    targetFullPath: string
  ): { compactNode: MdFile; segmentIndex: number } | null {
    if (!nodes) return null;
    const target = targetFullPath.toLowerCase();

    for (const node of nodes) {
      if (node.type !== 'folder') continue;

      if (node.isCompacted && node.compactedSegments) {
        // Check intermediate segments (all except the last one)
        for (let i = 0; i < node.compactedSegments.length - 1; i++) {
          if (node.compactedSegments[i].fullPath.toLowerCase() === target) {
            return { compactNode: node, segmentIndex: i };
          }
        }
      }

      // Recurse into children
      if (node.childrens && node.childrens.length > 0) {
        const found = this.findCompactIntermediateNode(node.childrens as MdFile[], targetFullPath);
        if (found) return found;
      }
    }

    return null;
  }

  /**
   * Spezza una compact folder chain al segmento che corrisponde a splitPath.
   * Esempio: `a / b / c` spezzato a `b` → nodo diventa `a / b` (head), figlio diventa `c` (tail).
   * Restituisce il nodo head (il cui childrens è il punto di inserimento).
   */
  private breakCompactFolderAt(nodes: MdFile[], splitPath: string): MdFile | null {
    const result = this.findCompactIntermediateNode(nodes, splitPath);
    if (!result) return null;

    const { compactNode, segmentIndex } = result;
    const segments = compactNode.compactedSegments!;

    // Tail segments: from segmentIndex+1 to end
    const tailSegments = segments.slice(segmentIndex + 1);

    // Create tail node with original children
    const tailFirstSeg = tailSegments[0];
    const tailNode = new MdFile(tailFirstSeg.name, tailFirstSeg.fullPath, tailFirstSeg.level, true);
    tailNode.type = 'folder';
    tailNode.fullPath = tailFirstSeg.fullPath;
    tailNode.relativePath = tailFirstSeg.fullPath; // will be relative, but consistent with tree
    tailNode.childrens = compactNode.childrens as MdFile[];
    tailNode.isLoading = false;
    tailNode.index = 0;

    if (tailSegments.length > 1) {
      tailNode.isCompacted = true;
      tailNode.compactedSegments = tailSegments;
      const lastTailSeg = tailSegments[tailSegments.length - 1];
      tailNode.compactedPath = tailSegments.map(s => s.name).join(' / ');
      tailNode.fullPath = lastTailSeg.fullPath;
    } else {
      tailNode.isCompacted = false;
      tailNode.compactedSegments = undefined;
      tailNode.compactedPath = undefined;
    }

    // Update head (the original node) — segments 0..segmentIndex
    const headSegments = segments.slice(0, segmentIndex + 1);
    const lastHeadSeg = headSegments[headSegments.length - 1];

    if (headSegments.length > 1) {
      compactNode.isCompacted = true;
      compactNode.compactedSegments = headSegments;
      compactNode.compactedPath = headSegments.map(s => s.name).join(' / ');
    } else {
      compactNode.isCompacted = false;
      compactNode.compactedSegments = undefined;
      compactNode.compactedPath = undefined;
      compactNode.name = headSegments[0].name;
    }
    compactNode.fullPath = lastHeadSeg.fullPath;

    // Head's children become just the tail node (new files will be pushed alongside)
    compactNode.childrens = [tailNode];

    return compactNode;
  }

  /**
   * Crea la gerarchia di cartelle mancanti tra la root del progetto e parentDirPath.
   * Restituisce il nodo della cartella più profonda creata (il punto di inserimento), oppure null.
   */
  private createMissingFolderHierarchy(parentDirPath: string): MdFile | null {
    // Get project root path
    const { ProjectsService } = require('./projects.service');
    const projectsService = this.injector.get(ProjectsService);
    const currentProject = projectsService.currentProjects$.getValue();
    if (!currentProject || !currentProject.path) return null;
    const projectRoot = currentProject.path.replace(/[\/\\]$/, '');

    // Normalize paths for comparison
    const normParent = parentDirPath.toLowerCase().replace(/\//g, '\\');
    const normRoot = projectRoot.toLowerCase().replace(/\//g, '\\');

    if (!normParent.startsWith(normRoot)) return null;

    // Walk UP from parentDirPath toward projectRoot, collecting missing folder paths
    const missingPaths: string[] = [];
    let currentPath = parentDirPath;

    while (currentPath.toLowerCase().replace(/\//g, '\\') !== normRoot) {
      // Check if this path exists in the tree
      const existingNode = this.findFolderInDataStore(this.dataStore.mdFiles, currentPath);
      if (existingNode) break;

      // Check if this path is an intermediate compact segment
      const compactResult = this.findCompactIntermediateNode(this.dataStore.mdFiles, currentPath);
      if (compactResult) {
        // Break the compact chain and use the result as the anchor
        this.breakCompactFolderAt(this.dataStore.mdFiles, currentPath);
        // After breaking, the folder should now be findable
        const brokenNode = this.findFolderInDataStore(this.dataStore.mdFiles, currentPath);
        if (brokenNode) break;
      }

      missingPaths.push(currentPath);
      // Move up one level
      const sep = Math.max(currentPath.lastIndexOf('\\'), currentPath.lastIndexOf('/'));
      if (sep <= 0) break;
      currentPath = currentPath.substring(0, sep);
    }

    if (missingPaths.length === 0) return null;

    // Find the anchor parent (the existing node we stopped at)
    let anchorParent: MdFile | null = null;
    if (currentPath.toLowerCase().replace(/\//g, '\\') === normRoot) {
      // Insert at root level — anchorParent stays null
    } else {
      anchorParent = this.findFolderInDataStore(this.dataStore.mdFiles, currentPath);
    }

    // Walk DOWN creating missing folders (missingPaths is deepest-first, reverse it)
    missingPaths.reverse();
    let lastCreated: MdFile | null = null;

    for (const folderPath of missingPaths) {
      const folderName = folderPath.substring(Math.max(folderPath.lastIndexOf('\\'), folderPath.lastIndexOf('/')) + 1);
      // Calculate level relative to project root
      const relativeParts = folderPath.substring(projectRoot.length + 1).split(/[\\\/]/);
      const level = relativeParts.length - 1;

      const newFolder = new MdFile(folderName, folderPath, level, true);
      newFolder.type = 'folder';
      newFolder.fullPath = folderPath;
      newFolder.relativePath = folderPath.substring(projectRoot.length + 1);
      newFolder.childrens = [];
      newFolder.isLoading = false;
      newFolder.index = 0;
      newFolder.isIndexed = true;
      newFolder.indexingStatus = 'completed';

      if (anchorParent) {
        anchorParent.childrens.push(newFolder);
      } else {
        // Insert at root level (before the dummy/emptyroot item)
        const dummyItem = this.dataStore.mdFiles[this.dataStore.mdFiles.length - 1];
        if (dummyItem && (dummyItem.type === 'emptyroot' || dummyItem.type === 'dummy')) {
          this.dataStore.mdFiles.splice(this.dataStore.mdFiles.length - 1, 0, newFolder);
        } else {
          this.dataStore.mdFiles.push(newFolder);
        }
      }

      anchorParent = newFolder;
      lastCreated = newFolder;
    }

    if (lastCreated) {
      this._mdFiles.next([...this.dataStore.mdFiles]);
    }

    return lastCreated;
  }

  /**
   * Rinomina una cartella nel dataStore, aggiornando ricorsivamente i path
   * della cartella e di tutti i suoi discendenti.
   * Returns true se il nodo è stato trovato e aggiornato.
   */
  renameFolderInDataStore(oldFullPath: string, newFullPath: string): boolean {
    const node = this.findFolderInDataStore(this.dataStore.mdFiles, oldFullPath);
    if (!node) return false;

    const newName = newFullPath.substring(Math.max(
      newFullPath.lastIndexOf('\\'), newFullPath.lastIndexOf('/')
    ) + 1);

    // Aggiorna il nodo stesso
    node.name = newName;
    this.rewritePaths(node, oldFullPath, newFullPath);

    // Aggiorna compact segments se presenti
    if (node.isCompacted && node.compactedSegments) {
      for (const seg of node.compactedSegments) {
        if (seg.fullPath.toLowerCase().startsWith(oldFullPath.toLowerCase())) {
          seg.fullPath = newFullPath + seg.fullPath.substring(oldFullPath.length);
        }
      }
    }

    // Aggiorna ricorsivamente tutti i discendenti
    this.rewriteChildrenPaths(node.childrens, oldFullPath, newFullPath);

    this._mdFiles.next([...this.dataStore.mdFiles]);
    return true;
  }

  private rewritePaths(node: any, oldPrefix: string, newPrefix: string): void {
    if (node.fullPath && node.fullPath.toLowerCase().startsWith(oldPrefix.toLowerCase())) {
      node.fullPath = newPrefix + node.fullPath.substring(oldPrefix.length);
    }
    if (node.path && node.path.toLowerCase().startsWith(oldPrefix.toLowerCase())) {
      node.path = newPrefix + node.path.substring(oldPrefix.length);
    }
    if (node.relativePath && node.relativePath.toLowerCase().startsWith(oldPrefix.toLowerCase())) {
      node.relativePath = newPrefix + node.relativePath.substring(oldPrefix.length);
    }
  }

  private rewriteChildrenPaths(children: any[], oldPrefix: string, newPrefix: string): void {
    if (!children) return;
    for (const child of children) {
      this.rewritePaths(child, oldPrefix, newPrefix);
      // Aggiorna il name solo se è una cartella il cui path è cambiato direttamente
      // (i file figli non cambiano nome, solo il path)
      if (child.isCompacted && child.compactedSegments) {
        for (const seg of child.compactedSegments) {
          if (seg.fullPath.toLowerCase().startsWith(oldPrefix.toLowerCase())) {
            seg.fullPath = newPrefix + seg.fullPath.substring(oldPrefix.length);
          }
        }
      }
      if (child.childrens) {
        this.rewriteChildrenPaths(child.childrens, oldPrefix, newPrefix);
      }
    }
  }

  updateFileIndexStatus(path: string, isIndexed: boolean): void {
    // Ricostruisce completamente l'array invece di modificare gli oggetti esistenti
    const updateNodeInArray = (nodes: any[]): any[] => {
      return nodes.map(node => {
        if (node.fullPath === path) {
          // Crea un nuovo oggetto invece di modificare quello esistente
          return {
            ...node,
            isIndexed: isIndexed,
            indexingStatus: isIndexed ? 'completed' : 'idle'
          };
        }
        
        if (node.childrens && node.childrens.length > 0) {
          return {
            ...node,
            childrens: updateNodeInArray(node.childrens)
          };
        }
        
        return node;
      });
    };

    // Ricostruisce completamente l'array
    this.dataStore.mdFiles = updateNodeInArray(this.dataStore.mdFiles);
    
    // Emette il nuovo array
    this._mdFiles.next([...this.dataStore.mdFiles]);
  }

  // Forza aggiornamento stato indicizzazione per file rinominati Rule #1
  forceFileAsIndexed(filePath: string): void {
    this.updateFileIndexStatus(filePath, true);
    
    setTimeout(() => {
      this.mdServerMessages.triggerRule1ForceUpdate(filePath);
    }, 100);
  }


  loadDynFolders(path: string, level: number) {
    const url = '../api/mdfiles/GetDynFoldersDocument';
    var params = new HttpParams().set('path', path).set('level', String(level));

    return this.http.get<MdFile[]>(url, { params })
      .subscribe(data => {
        if (this.dataStore.mdDynFolderDocument.length > 0) {          
          //var test = this.dataStore.mdDynFolderDocument.find(_ => _.path == path);
          //test.children = data;
        } else {
          this.dataStore.mdDynFolderDocument = data;
        }
        this._mdDynFolderDocument.next(Object.assign({}, this.dataStore).mdDynFolderDocument);
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  loadDocumentFolder(path: string, level: number, typeOfSelection:string): Observable<MdFile[]> {
    let url = '../api/mdfiles/GetDynFoldersDocument';
        
    if (typeOfSelection==="FoldersAndFiles") {
      url = '../api/mdfiles/GetDynFoldersAndFilesDocument';
    }
    console.log(url);
    var params = new HttpParams().set('path', path).set('level', String(level));
    return this.http.get<MdFile[]>(url, { params });
  }



  loadPublishNodes(path: string, level: number): Observable<MdFile[]> {
    const url = '../api/mdPublishNodes';
    var params = new HttpParams().set('path', path).set('level', String(level));
    return this.http.get<MdFile[]>(url, { params });
  }


  GetHtml(path: string) { //, currentFile: MdFile
    const url = '../api/mdexplorer/' + path;
    return this.http.get(url, { responseType: 'text' })//, currentFile      
  }

  getLandingPage() {
    const url = '../api/mdfiles/GetLandingPage';
    return this.http.get<MdFile>(url);
  }

  SetLandingPage(file: MdFile) {
    const url = '../api/mdfiles/SetLandingPage';
    return this.http.post<MdFile>(url, file);
  }

  setDevelopmentTags(folder: MdFile, projectRoot: string, tags: string[]) {
    const url = '../api/mdfiles/SetDevelopmentTags';
    return this.http.post(url, {
      folderPath: folder.fullPath,
      projectRoot: projectRoot,
      tags: tags
    });
  }

  openFolderOnFileExplorer(file: MdFile) {
    console.log('[MdFileService] openFolderOnFileExplorer() called');
    console.log('[MdFileService] file:', file);
    console.log('[MdFileService] file.fullPath:', file.fullPath);
    
    const url = '../api/mdfiles/OpenFolderOnFileExplorer';
    console.log('[MdFileService] POST to:', url);
    
    return this.http.post<MdFile>(url, file).pipe(
      tap(response => {
        console.log('[MdFileService] Response received:', response);
      }),
      catchError(error => {
        console.error('[MdFileService] Error in openFolderOnFileExplorer:', error);
        throw error;
      })
    );
  }

  deleteFile(file: MdFile) {
    const url = '../api/mdfiles/DeleteFile';
    return this.http.post<MdFile>(url, file);
      

      //this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    
  }

  //Minimum information to set
  // 1. fullPath:ex: "C:\Users\Carlo\Documents\2-personale\sviluppo\MdExplorer\UnitTestMdExplorer\RockSolidEdition\using-chatGPT\eargaer.md"
  // 2. level: not important

  recursiveDeleteFileFromDataStore(fileToFind: MdFile) {
    
    const dataFound: MdFile[] = [];
    this.recursiveSearch(this.dataStore.mdFiles, fileToFind, dataFound);

    if (dataFound.length === 1) {
      const dataIndex = this.dataStore.mdFiles.indexOf(dataFound[0]);
      if (dataIndex > -1) {
        this.dataStore.mdFiles.splice(dataIndex, 1);
      }
    } if (dataFound.length > 1) {
      //let cursor = this.dataStore.mdFiles;
      let currentFolder: MdFile[] = this.dataStore.mdFiles;
      for (var i = dataFound.length -1 ; i >0 ; i--) {
        currentFolder = currentFolder[currentFolder.indexOf(dataFound[i])].childrens;
      }
      currentFolder.splice(currentFolder.indexOf(dataFound[0]), 1);
    }
    this._mdFiles.next([...this.dataStore.mdFiles]);

  }

  recursiveSearchForShowData(fileToFind):MdFile[] {
    let dataFound: MdFile[] = [];
    this.recursiveSearch(this.dataStore.mdFiles, fileToFind, dataFound);
    return dataFound;
  }


  CreateNewDirectoryEx(path: string, directoryName: string, directoryLevel: number) {
    const url = '../api/mdfiles/CreateNewDirectoryEx';
    var newData = {
      directoryPath: path,
      directoryName: directoryName,
      directoryLevel: directoryLevel,
    }
    return this.http.post<MdFile>(url, newData);
  }


  CreateNewDirectory(path: string, directoryName: string, directoryLevel: number) {
    const url = '../api/mdfiles/CreateNewDirectory';
    var newData = {
      directoryPath: path,
      directoryName: directoryName,
      directoryLevel: directoryLevel,
    }
    return this.http.post<MdFile[]>(url, newData);
  }

  RenameDirectory(path: string, directoryName: string, directoryLevel: number) {
    const url = '../api/mdfiles/RenameDirectory';
    var newData = {
      directoryPath: path,
      directoryName: directoryName,
      directoryLevel: directoryLevel,
    }
    return this.http.post<MdFile[]>(url, newData);
  }
  pasteFromClipboard(node: any) {
    const url = '../api/mdfiles/pasteFromClipboard';
    console.log('[MdFileService] pasteFromClipboard called with:', node);
    console.log('[MdFileService] Making POST request to:', url);
    return this.http.post<any>(url, node);
  }

  /**
   * Save an annotated screenshot with marker descriptions.
   * @param formData FormData containing OriginalImage, AnnotatedImage, DocumentPath, ImageName, DescriptionsJson, ConnectionId
   */
  saveAnnotatedScreenshot(formData: FormData) {
    const url = '../api/mdfiles/SaveAnnotatedScreenshot';
    console.log('[MdFileService] saveAnnotatedScreenshot called');
    return this.http.post<{
      success: boolean;
      originalImagePath?: string;
      annotatedImagePath?: string;
      insertedMarkdown?: string;
      errorMessage?: string;
    }>(url, formData);
  }

  addExistingFileToMDEProject(node: MdFile,path:String) {
    const url = '../api/mdfiles/addExistingFileToMDEProject';
    return this.http.post<string>(url, { mdFile: node, fullPath:path });
  }

  getTextFromClipboard() {
    const url = '../api/mdfiles/getTextFromClipboard';
    return this.http.get<any>(url)    
  }

  


  cloneTimerDocument(node: MdFile) {
    const url = '../api/mdfiles/CloneTimerMd';
    return this.http.post<MdFile[]>(url, node);
  }

  CreateNewMd(path: string, title: string, directoryLevel: number, documentTypeId: number, documentType: string) {
    const url = '../api/mdfiles/CreateNewMd';
    var newData = {
      directoryPath: path,
      title: title,
      directoryLevel: directoryLevel,
      documentTypeId: documentTypeId,
      documentType: documentType
    }
    return this.http.post<MdFile[]>(url, newData);
  }


  //fileFoundMd: boolean = false;


  /**
   * Funzione di sostituzione di un nodo, con un altro
   * @param oldFile
   * @param newFile
   */
  changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, oldFile);
    var leaf = returnFound[0];
    
    if (!leaf) {
      console.error('❌ [Service] File non trovato nel datastore:', oldFile.name);
      return;
    }
    
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

  setSelectedMdFileFromSideNav(selectedFile: MdFile) {
    console.log('[MdFileService] setSelectedMdFileFromSideNav called with:', selectedFile);
    console.log('[MdFileService] _selectedMdFileFromSideNav value before:', this._selectedMdFileFromSideNav.value);
    console.log('[MdFileService] _selectedMdFileFromSideNav has observers:', this._selectedMdFileFromSideNav.observers?.length || 0);
    this._selectedMdFileFromSideNav.next(selectedFile);
    console.log('[MdFileService] _selectedMdFileFromSideNav value after:', this._selectedMdFileFromSideNav.value);
  }

  setSelectedDirectoryFromNewDirectory(selectedDirectory: MdFile) {
    this._selectedDirectoryFromNewDirectory.next(selectedDirectory);
  }
   
  setSelectedMdFileFromToolbar(selectedFile: MdFile) {
    let returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    this._selectedMdFileFromToolbar.next(returnFound);
  }

  setSelectedMdFileFromServer(selectedFile: MdFile) {    
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    this._serverSelectedMdFile.next(returnFound);
  }

  setSelectionMdFile(selectedFile: MdFile[]) {
    this._serverSelectedMdFile.next(selectedFile);
  }


  getMdFileFromDataStore(selectedFile: MdFile): MdFile {
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    return returnFound[0];
  }

  searchMdFileIntoDataStore(arrayMd: MdFile[], FileToFind: MdFile): MdFile[] {
    //this.fileFoundMd = false;
    var arrayFound: MdFile[] = [];
    this.recursiveSearch(arrayMd, FileToFind, arrayFound);
    return arrayFound;
  }


  recursiveSearch(arrayMd: MdFile[], fileToFind: MdFile, arrayFound: MdFile[]): boolean {
    if (arrayMd.length === 0) {
      return false;
    }    
    const thatFile = arrayMd.find(item => item.fullPath.toLowerCase() === fileToFind.fullPath.toLowerCase());

    if (!thatFile) {
      return arrayMd.some(item => {
        const found = this.recursiveSearch(item.childrens, fileToFind, arrayFound);
        if (found) {
          arrayFound.push(item);
        }
        return found;
      });
    } else {
      arrayFound.push(thatFile);
      return true;
    }
  }

  // New methods for file explorer functionality
  getSpecialFolders(): Observable<SpecialFolder[]> {
    const url = '../api/mdfiles/GetSpecialFolders';
    return this.http.get<SpecialFolder[]>(url);
  }

  getDrives(): Observable<Drive[]> {
    const url = '../api/mdfiles/GetDrives';
    return this.http.get<Drive[]>(url);
  }

  getNetworkShares(): Observable<any[]> {
    const url = '../api/mdfiles/GetNetworkShares';
    return this.http.get<any[]>(url);
  }


}

export interface INewFileCreated {
  newName: string;
  newPath: string;
  newLevel: number;
  expandable: boolean;
  relativePath: boolean;
}
