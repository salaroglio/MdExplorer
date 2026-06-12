import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnInit, ViewChild, ChangeDetectorRef, ChangeDetectionStrategy, AfterViewInit, OnDestroy } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacyMenuTrigger as MatMenuTrigger } from '@angular/material/legacy-menu';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { Observable, BehaviorSubject, Subject, Subscription, fromEvent } from 'rxjs';
import { auditTime, takeUntil } from 'rxjs/operators';
import { CompactSegment, IFileInfoNode } from '../../models/IFileInfoNode';
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
import { ProjectsService } from '../../services/projects.service';
import { UrlHandlerService } from '../../../services/url-handler.service';
import { P2PService } from '../../../services/p2p.service';
import { ProjectSettingsService } from '../../../projects/services/project-settings.service';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../commons/components/show-file-system/show-file-metadata';
import { InstallWizardDialogComponent, InstallWizardData } from '../dialogs/install-wizard/install-wizard.component';
import { AppStoreService } from '../../services/app-store.service';
import { BulkExportProgressService } from '../../services/bulk-export-progress.service';
import { FileEventsService } from '../../services/file-events.service';
import { HttpClient } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { MarkAssistantService } from '../../../mark-assistant/mark-assistant.service';
import { IndexingProgressService } from '../../services/indexing-progress.service';
import { IndexingProgressSnackComponent } from '../indexing-progress-snack/indexing-progress-snack.component';

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

  // Completed in ngOnDestroy: EVERY subscription in this component must pipe
  // takeUntil(destroy$). This component is destroyed/recreated on each project
  // enter/exit; un-torn-down subscriptions kept dead instances alive and made
  // every file event get processed N times.
  private destroy$ = new Subject<void>();
  
  // BehaviorSubject per tracciare lo stato di indicizzazione
  private indexedFilesSubject = new BehaviorSubject<Set<string>>(new Set());
  indexedFiles$ = this.indexedFilesSubject.asObservable();
  
  // Debouncing per aggiornamenti batch
  private pendingUpdates = new Map<string, boolean>();
  private updateTimer: any = null;
  
  // Contatore delle directory indicizzate
  private indexedFoldersCount = 0;
  private currentSnackbarRef: any = null;

  // State preservation for tree expansion during branch switch
  private expansionStateBeforeRefresh: Set<string> | null = null;

  // Compact folders - segment hover state
  hoveredSegment: CompactSegment | null = null;
  selectedCompactSegment: CompactSegment | null = null;

  // Sticky scroll (VS Code-style)
  stickyScrollEnabled = true;
  stickyAncestors: IFileInfoNode[] = [];
  private scrollSub: Subscription;

  // Drag & Drop state
  draggedNode: MdFile | null = null;
  dragOverNode: MdFile | null = null;
  dragOverSegment: CompactSegment | null = null;
  private dndMovingPaths: { oldPath: string; newPath: string } | null = null;

  // Skeleton loader state
  isLoading = true;

  // Event queue with debounce: accumulates events for 200ms, then processes as batch
  private eventQueue: Array<{ handler: () => void; label: string }> = [];
  private isProcessingQueue = false;
  private batchTimer: any = null;
  private readonly BATCH_DEBOUNCE_MS = 200;

  menuTopLeftPosition = { x: 0, y: 0 }
  @ViewChild(MatMenuTrigger, { static: true }) matMenuTrigger: MatMenuTrigger;


  private _transformer = (node: IFileInfoNode, level: number) => {
    return {
      expandable: (!!node.childrens && node.childrens.length > 0) || node.type == "folder" || node.type == "externalAppRoot" || node.type == "externalAppCategory",
      name: node.name,
      level: level,
      path: node.path,
      relativePath: node.path,
      fullPath: node.fullPath,
      type: node.type,
      index: node.index,
      isLoading: node.isLoading,
      childrens: [],
      isIndexed: node.isIndexed,
      indexingStatus: node.indexingStatus,
      indexingProgress: node.indexingProgress,
      developmentTags: node.developmentTags,
      // True when the folder owns a generated TOC file (drives the TOC icon)
      hasToc: node.hasToc,
      // Compact folder properties
      isCompacted: node.isCompacted,
      compactedPath: node.compactedPath,
      compactedSegments: node.compactedSegments,
      // External app
      appId: node.appId,
      appExecutable: node.appExecutable,
      appArgs: node.appArgs,
      appIcon: node.appIcon,
      appDescription: node.appDescription
    };
  }
  // trackBy per fullPath: l'expansionModel del FlatTreeControl di default usa
  // l'IDENTITÀ dell'oggetto-nodo. Gli update incrementali (compact-folder break,
  // createMissingFolderHierarchy, re-fetch) sostituiscono le istanze dei nodi →
  // l'identità cambia → l'espansione "si stacca" dai nodi correnti (icone e figli
  // non si aprono più). Chiavando l'espansione per fullPath (stabile e unico),
  // lo stato di espansione sopravvive al cambio di istanza. Gemello del [trackBy]
  // sul <mat-tree> che riconcilia le righe DOM.
  treeControl = new FlatTreeControl<IFileInfoNode>(
    node => node.level, node => node.expandable,
    // Return tipizzato `any` di proposito: manteniamo K=IFileInfoNode (così i
    // generics di MatTreeFlatDataSource/Flattener restano invariati e il build
    // non va in cascata), ma a runtime l'expansionModel usa la stringa fullPath
    // come chiave — è esattamente ciò che serve per resistere al churn di istanze.
    { trackBy: (node: IFileInfoNode): any => node.fullPath });

  treeFlattener = new MatTreeFlattener(
    this._transformer, node => node.level, node => node.expandable, node => node.childrens);

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  hasChild = (_: number, node: IFileInfoNode) => node.expandable;

  isFolder = (_: number, node: IFileInfoNode) => node.type == "folder";
  isMdPublish = (_: number, node: IFileInfoNode) => node.type == "folder" && node.name == "mdPublish";
  isEmptyRoot = (_: number, node: IFileInfoNode) => node.type == "emptyroot";
  isExternalAppRoot = (_: number, node: IFileInfoNode) => node.type == "externalAppRoot";
  isExternalAppCategory = (_: number, node: IFileInfoNode) => node.type == "externalAppCategory";
  isExternalApp = (_: number, node: IFileInfoNode) => node.type == "externalApp";
  isExternalAppNotInstalled = (_: number, node: IFileInfoNode) => node.type == "externalAppNotInstalled";

  isImageIcon(icon: string | undefined): boolean {
    if (!icon) return false;
    return icon.startsWith('data:image/') || icon.startsWith('http://') || icon.startsWith('https://');
  }

  ///////////////////////////////


  // P2P availability state
  isP2PAvailable = false;

  // RAG availability state
  isRagEnabled = false;

  constructor(private router: Router,
    private mdFileService: MdFileService,
    private navService: MdNavigationService,
    public dialog: MatDialog,
    private snackBar: MatSnackBar,
    private clipboard: Clipboard,
    private changeDetectorRef: ChangeDetectorRef,
    private mdServerMessages: MdServerMessagesService,
    private tocService: TocGenerationService,
    private tocProgressService: TocProgressService,
    private projectsService: ProjectsService,
    private urlHandlerService: UrlHandlerService,
    private p2pService: P2PService,
    private projectSettingsService: ProjectSettingsService,
    private appStoreService: AppStoreService,
    private bulkExportProgressService: BulkExportProgressService,
    private fileEventsService: FileEventsService,
    private http: HttpClient,
    private translate: TranslateService,
    private markAssistant: MarkAssistantService,
    private indexingProgressService: IndexingProgressService
  ) {
    this.dataSource.data = TREE_DATA;
    this.mdFileService.serverSelectedMdFile.pipe(takeUntil(this.destroy$)).subscribe(_ => {
      const myClonedArray = [];
      _.forEach(val => myClonedArray.push(Object.assign({}, val)));
      while (myClonedArray.length > 1) {
        var toExpand = myClonedArray.pop();
        var test = this.treeControl.dataNodes?.find(_ => _.path == toExpand.path);
        if (test) {
          this.treeControl.expand(test);
        }
      }
      if (myClonedArray.length > 0) {
        var toExpand = myClonedArray.pop();
        this.activeNode = this.treeControl.dataNodes?.find(_ => _.path == toExpand.path);

        if (this.activeNode != undefined && this.activeNode.type == "folder") {
          this.treeControl.expand(this.activeNode);
        }
      }
    });

    // File indicizzati: aggiorna lo stato del nodo. SOLO markForCheck — un
    // detectChanges() sincrono per ogni fileIndexed (la pipeline ne emette uno
    // PER FILE) è il pattern di render rientrante che corrompe il differ del
    // MatTree mentre loadAll sta sostituendo l'albero.
    this.mdServerMessages.fileIndexed$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      const currentSet = this.indexedFilesSubject.value;
      const newSet = new Set(currentSet);
      newSet.add(data.path);
      this.indexedFilesSubject.next(newSet);

      // Trova e aggiorna direttamente il nodo nel dataSource
      this.updateNodeIndexStatus(data.path, true);

      this.changeDetectorRef.markForCheck();
    });

    // Building knowledge progress — UNA sola snackbar custom (basso a destra) per tutta
    // l'indicizzazione. Lo stato (percent / processed / total) è pilotato da
    // IndexingProgressService e popolato dall'evento SignalR knowledgeProgress.
    // Prima qui c'era un MatSnackBar.open() ripetuto per ogni folderIndexingComplete
    // → "scoppiettare" di snackbar. Adesso una sola istanza, vive da parsingProjectStart
    // a parsingProjectStop + 1.5s.
    this.mdServerMessages.parsingProjectStart$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.indexingProgressService.reset();
      this.openIndexingSnackbar();
    });

    // Folder spinner sulla tree (independent dal progresso globale)
    this.mdServerMessages.folderIndexingStart$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      const node = this.findNodeByPath(data.path);
      if (node) {
        node.indexingStatus = 'indexing';
        this.changeDetectorRef.markForCheck();
      }
    });

    this.mdServerMessages.folderIndexingComplete$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      const node = this.findNodeByPath(data.path);
      if (node) {
        node.indexingStatus = 'completed';
        this.changeDetectorRef.markForCheck();
      }
      // Non aprire più snackbar qui — il progresso ora è unificato via knowledgeProgress
      // sul componente IndexingProgressSnackComponent.
    });

    // Avanzamento globale "Building knowledge" alimentato dal backend dopo
    // ogni cartella nella fase ParseLinks.
    this.mdServerMessages.knowledgeProgress$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      const processed = data?.processed ?? 0;
      const total = data?.total ?? 0;
      const percent = data?.percent ?? 0;
      this.indexingProgressService.setProgress(processed, total, percent);
    });

    // Fine indicizzazione: forza 100% e auto-dismiss dopo 1.5s
    this.mdServerMessages.parsingProjectStop$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.indexingProgressService.setComplete();
      setTimeout(() => {
        if (this.currentSnackbarRef) {
          this.currentSnackbarRef.dismiss();
        }
      }, 1500);
    });

    // Mark Actions — recursive "Riassumi documentazione" job emits a `toc-ready`
    // event per folder right after its <name>.md.directory is (re)generated. We
    // flip node.hasToc here so the document icon appears on the folder right
    // away, without waiting for the user to reopen the project. The Mark dialog
    // ignores this phase (MarkAssistantService.onFolderProgress).
    this.mdServerMessages.markFolderProgress$.pipe(takeUntil(this.destroy$)).subscribe(p => {
      if (p?.phase !== 'toc-ready' || !p.folderFullPath) return;
      const node = this.findNodeByPath(p.folderFullPath);
      if (node) {
        node.hasToc = true;
        this.changeDetectorRef.markForCheck();
      }
    });

    // Creazione di nuovi file markdown (queued + debounced)
    this.mdServerMessages.markdownFileCreated$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      this.enqueueEvent(() => this.handleNewMarkdownFileCreated(data), 'fileCreated');
    });

    // Cancellazione di file markdown (queued + debounced)
    this.mdServerMessages.markdownFileDeleted$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      this.enqueueEvent(() => this.handleMarkdownFileDeleted(data), 'fileDeleted');
    });

    // Creazione cartella (queued + debounced)
    this.mdServerMessages.folderCreated$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      this.enqueueEvent(() => this.handleFolderCreated(data), 'folderCreated');
    });

    // Cancellazione cartella (queued + debounced)
    this.mdServerMessages.folderDeleted$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      this.enqueueEvent(() => this.handleFolderDeleted(data), 'folderDeleted');
    });

    // Rename cartella → rewrite ricorsivo dei path nel tree
    this.mdServerMessages.folderRenamed$.pipe(takeUntil(this.destroy$)).subscribe(data => {
      const oldFullPath = data.oldFullPath || data.OldFullPath;
      const newFullPath = data.fullPath || data.FullPath;
      console.log(`✏️ folderRenamed: ${oldFullPath} → ${newFullPath}`);
      const updated = this.mdFileService.renameFolderInDataStore(oldFullPath, newFullPath);
      if (!updated) {
        // Cartella non nel tree (mai espansa) — nulla da fare
        console.log('📂 [folderRenamed] Folder not in tree (unexpanded) — nothing to update');
      }
      this.changeDetectorRef.markForCheck();
    });

    // Storm FSW → processa il payload deduplicato incrementalmente.
    // NON scarta la coda eventi: gli eventi individuali pre-soglia NON fanno
    // parte del batch storm (il backend batcha solo i post-soglia), quindi
    // buttarli via significava perdere cambiamenti. Le mutazioni sono
    // idempotenti, l'eventuale sovrapposizione è innocua.
    this.mdServerMessages.fileSystemStorm$.pipe(takeUntil(this.destroy$)).subscribe(changes => {
      console.log(`⚡ FileSystem storm ended - ${changes?.length || 0} deduplicated changes`);
      this.flushEventQueue();
      if (changes && changes.length > 0) {
        this.processStormChanges(changes);
      }
    });

    // Listener per forzare change detection (Rule #1 fix) - seguendo il pattern SignalR
    this.mdServerMessages.addRule1ForceUpdateListener((data, component) => {
      // Questo non verrà mai chiamato perché non c'è un vero evento SignalR
    }, this);

    // Git branch switch / pull - capture expansion state BEFORE refresh
    this.mdServerMessages.gitBranchSwitched$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      console.log('🔄 Git branch switched detected - capturing expansion state');
      this.expansionStateBeforeRefresh = this.captureExpansionState();
      console.log('📦 Captured', this.expansionStateBeforeRefresh.size, 'expanded nodes');
    });

    this.mdServerMessages.gitPullRefreshed$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      console.log('🔄 Git pull detected - capturing expansion state');
      this.expansionStateBeforeRefresh = this.captureExpansionState();
      console.log('📦 Captured', this.expansionStateBeforeRefresh.size, 'expanded nodes');
    });

    // Listener per cambio progetto - mostra skeleton loader
    this.projectsService.projectChanging$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      console.log('🔄 Project changing - showing skeleton loader');
      this.isLoading = true;
      this.changeDetectorRef.markForCheck();
    });

    // Listener per "Reveal in Tree" dal pulsante mirino nel sidenav
    this.mdFileService.revealInTree$.pipe(takeUntil(this.destroy$)).subscribe(file => {
      this.revealAndScrollToNode(file);
    });

    // FSW overflow/errore: md-file.service fa già il reload completo; qui solo
    // il feedback visibile all'utente (mai recovery silenzioso).
    this.mdServerMessages.fileSystemWatcherError$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.snackBar.open(this.translate.instant('MD_TREE.WATCHER_OVERFLOW_RELOAD'), '', { duration: 5000 });
    });
  }
 
  //="{ value: '', params: { delay: node.index * 100 } }"
  ngOnInit(): void {
    // Subscribe to P2P availability
    this.p2pService.isAvailable$.pipe(takeUntil(this.destroy$)).subscribe(available => {
      this.isP2PAvailable = available;
    });

    // Subscribe to RAG enabled status
    this.projectsService.ragEnabled$.pipe(takeUntil(this.destroy$)).subscribe(enabled => {
      this.isRagEnabled = enabled;
      this.changeDetectorRef.markForCheck();
    });

    this.loadStickyScrollSetting();

    this.mdFiles = this.mdFileService.mdFiles;
    this.mdFileService.mdFiles.pipe(takeUntil(this.destroy$)).subscribe(data => {
      // Ignora emissioni vuote (BehaviorSubject emette [] inizialmente)
      if (data && data.length > 0) {
        // COALESCE: durante una raffica di update incrementali (es. un agente che
        // crea molti file) questa subscription emette N volte in un singolo stack
        // sincrono. Il vecchio codice faceva N volte `dataSource.data=[...]` +
        // `detectChanges()` rientrante: i render del CDK MatTree si calpestavano,
        // corrompendo il suo differ e lasciando righe DOM orfane NON più rimovibili
        // (nemmeno con dataSource.data=[]; solo il reload le sanava).
        // Ora bufferizziamo l'ultima emissione e applichiamo UN solo render per
        // microtask → il differ resta consistente. I DATI restano incrementali:
        // cambia solo la cadenza con cui vengono spinti nella view.
        this._pendingTreeData = data;
        this.scheduleTreeRender();
      }
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

  // ── Coalescing del rendering dell'albero ──
  // Buffer dell'ultima emissione + flag di scheduling. Più emissioni nello stesso
  // stack sincrono (raffica) collassano in un solo render nel microtask successivo,
  // evitando il rendering rientrante che corrompe il differ del MatTree.
  private _pendingTreeData: any[] | null = null;
  private _treeRenderScheduled = false;

  private scheduleTreeRender(): void {
    if (this._treeRenderScheduled) { return; }
    this._treeRenderScheduled = true;
    Promise.resolve().then(() => {
      this._treeRenderScheduled = false;
      const data = this._pendingTreeData;
      this._pendingTreeData = null;
      if (!data || data.length === 0) { return; }

      // Inizializza ricorsivamente tutte le proprietà
      this.initializeNodeProperties(data);
      // Una NUOVA array per il change detection con OnPush
      this.dataSource.data = [...data];

      // Restore expansion state if we have saved state (from branch switch)
      if (this.expansionStateBeforeRefresh !== null) {
        this.restoreExpansionState(this.expansionStateBeforeRefresh);
        this.expansionStateBeforeRefresh = null; // Clear after use
      }

      // Nascondi skeleton loader quando i dati REALI arrivano
      this.isLoading = false;

      // OnPush: solo markForCheck. NIENTE detectChanges() sincrono: era la fonte
      // del rendering rientrante che corrompeva il differ del MatTree.
      this.changeDetectorRef.markForCheck();
    });
  }

  deferredOpenProject(data, objectThis: MdTreeComponent): void {
    // Check if URL handler is opening a specific document - skip landing page in that case
    if (objectThis.urlHandlerService.skipLandingPage) {
      console.log('🚫 [deferredOpenProject] Skipping landing page - URL handler is opening a specific document');
      return;
    }

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

  // ==================== Compact Folder Segment Methods ====================

  onSegmentHover(segment: CompactSegment): void {
    this.hoveredSegment = segment;
  }

  onSegmentLeave(): void {
    this.hoveredSegment = null;
  }

  onSegmentRightClick(event: MouseEvent, segment: CompactSegment, node: MdFile): void {
    event.preventDefault();
    event.stopPropagation();

    // Store the selected segment for use in create operations
    this.selectedCompactSegment = segment;

    // Build a synthetic MdFile representing the clicked segment. Downstream
    // consumers (openTocDirectory → navigateToTocFile, createDirectoryOn, etc.)
    // rely on relativePath being populated; without it they hit the no-prefix
    // fallback and end up writing files at the project root.
    const segmentItem = new MdFile(segment.name, segment.fullPath, segment.level, true);
    segmentItem.fullPath = segment.fullPath;
    segmentItem.type = 'folder';

    if (node.isCompacted && node.compactedSegments && node.relativePath != null) {
      // The compact node's own relativePath is the FIRST segment of the chain
      // (compactSingleNode never reassigns it). Extend it forward by appending
      // the names of the segments between index 1 and the clicked segment.
      const idx = node.compactedSegments.findIndex(s => s.fullPath === segment.fullPath);
      const baseRel = node.relativePath.replace(/\\/g, '/');
      if (idx === 0) {
        segmentItem.relativePath = node.relativePath;
      } else if (idx > 0) {
        const extra = node.compactedSegments.slice(1, idx + 1).map(s => s.name).join('/');
        segmentItem.relativePath = `${baseRel}/${extra}`;
      }
    }

    this.menuTopLeftPosition.x = event.clientX;
    this.menuTopLeftPosition.y = event.clientY;
    this.matMenuTrigger.menuData = { item: segmentItem };
    this.matMenuTrigger.openMenu();
  }

  // ==================== End Compact Folder Methods ====================

  public async getNode(node: MdFile) {
    // NOTA gating rimosso il 2026-05-23:
    // Prima qui c'era un early-return per `isFileWaiting(node)` (file non
    // ancora indicizzato) che apriva una snackbar "FILE_INDEXING" e bloccava
    // l'apertura. L'indicizzazione popola LinkInsideMarkdown ed embedding RAG,
    // ma il file in sé è leggibile da subito. Il blocco contraddiceva il
    // design async della pipeline (vedi IndexingPipelineService + Task.Yield).

    // External app: check for updates, then navigate to embedded view
    if (node.type === 'externalApp') {
      console.log('[md-tree] Opening external app:', node.appId, 'executable:', node.appExecutable);
      this.appStoreService.checkUpdate(node.appId).subscribe({
        next: (result) => {
          if (result.hasUpdate && result.catalogApp) {
            const dialogRef = this.dialog.open(InstallWizardDialogComponent, {
              width: '480px',
              data: {
                appId: node.appId,
                appName: node.name,
                appDescription: node.appDescription,
                appIcon: node.appIcon,
                mode: 'update',
                installedVersion: result.installedVersion,
                catalogApp: result.catalogApp
              } as InstallWizardData
            });
            dialogRef.afterClosed().subscribe(closeResult => {
              if (closeResult === 'updated') {
                // Force ExternalAppComponent to re-launch by emitting null first
                this.mdFileService.setSelectedMdFileFromSideNav(null);
                setTimeout(() => this.openExternalApp(node), 300);
              } else {
                this.openExternalApp(node);
              }
            });
          } else {
            this.openExternalApp(node);
          }
        },
        error: () => {
          // If check fails, open app anyway
          this.openExternalApp(node);
        }
      });
      return;
    }
    if (node.type === 'externalAppNotInstalled') {
      this.dialog.open(InstallWizardDialogComponent, {
        width: '480px',
        data: {
          appId: node.appId,
          appName: node.name,
          appDescription: node.appDescription,
          appIcon: node.appIcon,
          mode: 'install'
        } as InstallWizardData
      });
      return;
    }

    // PromptLab file: navigate to PromptLab view
    if (node.type === 'promptlab') {
      try {
        await this.router.navigate(['/main/navigation/promptlab']);
        this.mdFileService.setSelectedMdFileFromSideNav(node);
        this.navService.setNewNavigation(node);
        this.activeNode = node;
        this.selectedNode = node;
        this.changeDetectorRef.markForCheck();
      } catch (error) {
        console.error('PromptLab navigation failed:', error);
        this.snackBar.open(this.translate.instant('MD_TREE.NAV_ERROR_PROMPTLAB'), 'OK', { duration: 3000 });
      }
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
      this.snackBar.open(this.translate.instant('MD_TREE.NAV_ERROR'), 'OK', { duration: 3000 });
    }
  }

  private async openExternalApp(node: MdFile): Promise<void> {
    await this.router.navigate(['/main/navigation/external-app']);
    this.mdFileService.setSelectedMdFileFromSideNav(node);
  }

  // ==================== Drag & Drop ====================

  onDragStart(event: DragEvent, node: MdFile): void {
    if (node.type !== 'mdFile' && node.type !== 'mdFileTimer') {
      event.preventDefault();
      return;
    }
    this.draggedNode = node;
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('text/plain', node.fullPath);
    event.stopPropagation();
  }

  onDragEnd(event: DragEvent): void {
    this.draggedNode = null;
    this.dragOverNode = null;
    this.dragOverSegment = null;
  }

  onDragOver(event: DragEvent, node: MdFile): void {
    if (node.type !== 'folder' || !this.draggedNode) {
      return;
    }
    event.preventDefault();
    event.dataTransfer.dropEffect = 'move';
    // Only highlight the whole node if no compact segment is being hovered
    if (!this.dragOverSegment) {
      this.dragOverNode = node;
    }
  }

  onDragLeave(event: DragEvent): void {
    this.dragOverNode = null;
  }

  // Drag over a specific compact segment
  onSegmentDragOver(event: DragEvent, segment: CompactSegment, node: MdFile): void {
    if (node.type !== 'folder' || !this.draggedNode) {
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    event.dataTransfer.dropEffect = 'move';
    this.dragOverSegment = segment;
    this.dragOverNode = null; // Don't highlight the whole row
  }

  onSegmentDragLeave(event: DragEvent): void {
    this.dragOverSegment = null;
  }

  // Drop on a specific compact segment
  onSegmentDrop(event: DragEvent, segment: CompactSegment, node: MdFile): void {
    event.preventDefault();
    event.stopPropagation();
    this.performDrop(segment.fullPath, segment.name);
  }

  onDrop(event: DragEvent, node: MdFile): void {
    event.preventDefault();
    event.stopPropagation();
    console.log('[DnD] onDrop', { nodeType: node.type, nodeName: node.name, hasDraggedNode: !!this.draggedNode, hasSegment: !!this.dragOverSegment });

    if (!this.draggedNode || node.type !== 'folder') {
      console.warn('[DnD] onDrop aborted: draggedNode=' + !!this.draggedNode + ', nodeType=' + node.type);
      this.draggedNode = null;
      this.dragOverNode = null;
      this.dragOverSegment = null;
      return;
    }

    // If a compact segment was hovered, use its path
    const targetPath = this.dragOverSegment ? this.dragOverSegment.fullPath : node.fullPath;
    const targetName = this.dragOverSegment ? this.dragOverSegment.name : node.name;
    this.performDrop(targetPath, targetName);
  }

  private performDrop(targetPath: string, targetName: string): void {
    console.log('[DnD] performDrop called', { targetPath, targetName, hasDraggedNode: !!this.draggedNode });
    if (!this.draggedNode) {
      console.warn('[DnD] performDrop: draggedNode is null, aborting');
      this.dragOverNode = null;
      this.dragOverSegment = null;
      return;
    }

    // Skip if file is already in the target folder
    const fileDirPath = this.getParentDirPath(this.draggedNode.fullPath);
    console.log('[DnD] performDrop paths', { fileDirPath, targetPath, draggedFullPath: this.draggedNode.fullPath });
    if (fileDirPath === targetPath) {
      this.snackBar.open(this.translate.instant('MD_TREE.ALREADY_IN_FOLDER'), '', { duration: 2000 });
      this.draggedNode = null;
      this.dragOverNode = null;
      this.dragOverSegment = null;
      return;
    }

    const draggedFile = this.draggedNode;
    this.draggedNode = null;
    this.dragOverNode = null;
    this.dragOverSegment = null;

    const oldFullPath = draggedFile.fullPath;
    const separator = oldFullPath.includes('\\') ? '\\' : '/';
    const newFullPath = targetPath + separator + draggedFile.name;

    // Set suppress flag BEFORE HTTP call so SignalR handlers skip FSW-leaked events
    this.dndMovingPaths = { oldPath: oldFullPath, newPath: newFullPath };

    console.log('[DnD] calling moveMdFile', { name: draggedFile.name, relativePath: draggedFile.relativePath, targetPath });
    this.mdFileService.moveMdFile(draggedFile, targetPath).subscribe({
      next: () => {
        console.log('[DnD] moveMdFile success');
        this.snackBar.open(this.translate.instant('MD_TREE.MOVED_FILE', { fileName: draggedFile.name, targetName: targetName }), 'OK', { duration: 3000 });

        // Save expansion state BEFORE any _mdFiles.next() calls
        const expandedPaths = this.captureExpansionState();

        // Incremental update: remove from old parent, add to new parent
        this.mdFileService.recursiveDeleteFileFromDataStore(draggedFile);

        // Compute new relativePath from project root
        const projectPath = this.projectsService.currentProjects$.value?.path || '';
        let newRelativePath = newFullPath;
        if (projectPath && newFullPath.toLowerCase().startsWith(projectPath.toLowerCase())) {
          newRelativePath = newFullPath.substring(projectPath.length);
          // Remove leading separator
          if (newRelativePath.startsWith('\\') || newRelativePath.startsWith('/')) {
            newRelativePath = newRelativePath.substring(1);
          }
        }

        const movedFile = {
          ...draggedFile,
          fullPath: newFullPath,
          relativePath: newRelativePath,
          path: newRelativePath.replace(/\\/g, '/'),
          fullDirectoryPath: targetPath,
          isIndexed: true,
          indexingStatus: 'completed'
        } as MdFile;

        this.mdFileService.addFileToParent(movedFile, targetPath);

        // Update indexed files tracking Set: remove old path, add new
        const currentSet = this.indexedFilesSubject.value;
        const newSet = new Set(currentSet);
        newSet.delete(oldFullPath);
        newSet.add(newFullPath);
        this.indexedFilesSubject.next(newSet);

        // Restore expansion state (uses setTimeout internally)
        this.restoreExpansionState(expandedPaths);
        this.changeDetectorRef.markForCheck();

        // Clear suppress flag after delay to catch any buffered FSW events
        setTimeout(() => { this.dndMovingPaths = null; }, 500);
      },
      error: (err) => {
        console.error('[DnD] moveMdFile error:', err);
        this.dndMovingPaths = null;
        this.snackBar.open(this.translate.instant('MD_TREE.MOVE_ERROR', { error: err.error?.message || err.message }), 'OK', { duration: 5000 });
      }
    });
  }

  // ==================== End Drag & Drop ====================

  // Manu management

  createMdOn(node: MdFile) {

    this.dialog.open(NewMarkdownComponent, {
      width: '400px',
      data: node,
      panelClass: 'new-markdown-dialog',
    });
  }

  setMdAsLandingPage(node: MdFile) {
    this.mdFileService.SetLandingPage(node).subscribe(_ => {
      this.snackBar.open(this.translate.instant('MD_TREE.IS_LANDING_PAGE', { name: node.name }), 'OK', { duration: 5000 });
      
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
        this.snackBar.open(this.translate.instant('MD_TREE.EXPLORER_OPENED'), '', { duration: 500 });
      },
      error => {
        console.error('[MdTreeComponent] openFolderOnFileExplorer error:', error);
        this.snackBar.open(this.translate.instant('MD_TREE.EXPLORER_ERROR', { error: error.message }), '', { duration: 3000 });
      }
    );
  }

  async openTocDirectory(node: MdFile) {
    // Deterministic TOC: always regenerate, then navigate to the .md.directory file.
    // The generation reads the file system + each doc's TL;DR + MD5 hash, and appends
    // the aggregated knowledge graph from any .mde-doc/*.kg.md siblings. No AI involved.
    this.generateTocWithAI(node, true);
  }

  /**
   * Summons the Mark assistant scoped to a folder. Mark opens its window with a
   * context menu of folder actions (currently just "Riassumi documentazione").
   * Resolves the folder path through the same compact-folder convention as
   * generateTocWithAI — for a compacted node the real folder is the LAST segment.
   */
  openMarkForFolder(node: MdFile) {
    if (node == null) return;
    const lastSeg = node.isCompacted && node.compactedSegments?.length
      ? node.compactedSegments[node.compactedSegments.length - 1]
      : null;
    const folderFullPath = lastSeg ? lastSeg.fullPath : node.fullPath;
    const folderName = lastSeg ? lastSeg.name : node.name;
    this.markAssistant.launchFolderActions({ folderFullPath, folderName });
  }

  /**
   * Opens the folder's existing TOC file (<dirname>.md.directory) directly,
   * without regenerating it. Wired to the document icon shown on folder nodes
   * whose node.hasToc === true.
   */
  openTocFile(node: MdFile, event: MouseEvent) {
    // Stop the click from bubbling to the folder row (which would toggle it).
    event.stopPropagation();
    this.navigateToTocFile(node);
  }
  
  exportFolderToWord(node: MdFile) {
    // Use fullPath (absolute) to avoid issues with compact folders
    // where node.name/relativePath miss intermediate segments
    const folderFullPath = node.fullPath;
    const displayName = node.isCompacted && node.compactedSegments
      ? node.compactedSegments.map(s => s.name).join('/')
      : node.name;

    this.bulkExportProgressService.showProgress(displayName);

    const connectionId = this.mdServerMessages.connectionId;
    this.http.post<any>(`../api/mdexport/bulk?ConnectionId=${connectionId}`, {
      folderFullPath: folderFullPath
    }).subscribe({
      next: (result) => {
        console.log('[MdTreeComponent] Bulk export started:', result);
        if (result.total === 0) {
          this.bulkExportProgressService.hideProgress();
          this.snackBar.open(this.translate.instant('MD_TREE.NO_MD_FILES'), 'OK', { duration: 3000 });
        }
      },
      error: (err) => {
        console.error('[MdTreeComponent] Error starting bulk export:', err);
        this.bulkExportProgressService.hideProgress();
        this.snackBar.open(this.translate.instant('MD_TREE.EXPORT_START_ERROR'), 'OK', { duration: 3000 });
      }
    });
  }

  private generateTocWithAI(node: MdFile, navigateAfter: boolean) {
    // For compact folders node.fullPath points to the FIRST segment of the chain
    // (compactSingleNode never reassigns it — see the TODO comment in md-file.service.ts).
    // The folder whose children the user is actually looking at is the LAST segment;
    // findFolderInDataStore follows the same convention.
    const lastSeg = node.isCompacted && node.compactedSegments?.length
      ? node.compactedSegments[node.compactedSegments.length - 1]
      : null;
    const folderFullPath = lastSeg ? lastSeg.fullPath : node.fullPath;
    const displayPath = node.isCompacted && node.compactedSegments
      ? node.compactedSegments.map(s => s.name).join('/')
      : (node.relativePath || node.name);
    console.log('[MdTreeComponent] generateTocWithAI - folderFullPath:', folderFullPath);

    // Mostra il progress dialog
    this.tocProgressService.showProgress(displayPath);

    this.tocService.generateToc(folderFullPath).subscribe({
      next: (result) => {
        console.log('[MdTreeComponent] TOC generation result:', result);
        
        // SEMPRE chiudi il progress dialog quando riceviamo una risposta
        this.tocProgressService.hideProgress();
        
        if (result.success) {
          this.snackBar.open(this.translate.instant('MD_TREE.TOC_GENERATED'), 'OK', { duration: 3000 });

          // The folder now owns a TOC file: surface the clickable document icon
          // right away, without waiting for a full tree reload.
          node.hasToc = true;
          this.changeDetectorRef.markForCheck();

          if (navigateAfter) {
            // Naviga al file TOC generato
            setTimeout(() => {
              this.navigateToTocFile(node);
            }, 500);
          }
        } else {
          this.snackBar.open(
            result.message || this.translate.instant('MD_TREE.TOC_GENERATED'),
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
        let errorMessage = this.translate.instant('MD_TREE.TOC_UPDATE_ERROR');
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
    // For compact folders node.name/relativePath/fullPath all freeze at the
    // FIRST segment of the chain (compactSingleNode never reassigns them).
    // The backend names the TOC file after the LAST segment (Path.GetFileName
    // of the absolute folder), so the frontend must match or it would point at
    // a stale <firstSegment>.md.directory.
    const lastSeg = node.isCompacted && node.compactedSegments?.length
      ? node.compactedSegments[node.compactedSegments.length - 1]
      : null;
    const directoryName = lastSeg ? lastSeg.name : node.name;
    const folderRelativePath = node.isCompacted && node.compactedSegments
      ? node.compactedSegments.map(s => s.name).join('/')
      : node.relativePath;
    const folderFullPath = lastSeg ? lastSeg.fullPath : node.fullPath;
    const relativePath = folderRelativePath
      ? `${folderRelativePath}/${directoryName}.md.directory`
      : `${directoryName}.md.directory`;

    // Crea un oggetto MdFile per il file .md.directory
    const tocFile: MdFile = {
      name: `${directoryName}.md.directory`,
      relativePath: relativePath,
      fullPath: folderFullPath ? `${folderFullPath}/${directoryName}.md.directory` : `${directoryName}.md.directory`,
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
      this.snackBar.open(this.translate.instant('MD_TREE.TOC_DIR_ERROR'), 'OK', { duration: 3000 });
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

  /**
   * Copy the mdexplorer:// URL for this document to the clipboard.
   * Format: mdexplorer://opendocument/<project-name>/<relative-path>
   */
  copyMdExplorerLink(node: MdFile) {
    const currentProject = this.projectsService.currentProjects$.value;
    if (!currentProject) {
      this.snackBar.open(this.translate.instant('MD_TREE.NO_PROJECT_OPEN'), 'OK', { duration: 3000 });
      return;
    }

    // Build the relative path (use forward slashes)
    let relativePath = node.relativePath.replace(/\\/g, '/');
    // Remove leading slash if present
    if (relativePath.startsWith('/')) {
      relativePath = relativePath.substring(1);
    }

    // Build the mdexplorer:// URL
    // Format: mdexplorer://opendocument/<project-name>/<relative-path>
    // Encode each path segment separately (to preserve slashes but encode spaces/special chars)
    const encodedPath = relativePath.split('/').map(segment => encodeURIComponent(segment)).join('/');
    const mdExplorerUrl = `mdexplorer://opendocument/${encodeURIComponent(currentProject.name)}/${encodedPath}`;

    this.clipboard.copy(mdExplorerUrl);
    this.snackBar.open(this.translate.instant('MD_TREE.MDE_LINK_COPIED'), 'OK', { duration: 2000 });
  }

  /**
   * Share a file via P2P (WebTorrent)
   * Creates a torrent and provides a magnet link
   */
  shareViaP2P(node: MdFile): void {
    if (!this.isP2PAvailable) {
      this.snackBar.open(this.translate.instant('MD_TREE.P2P_NOT_AVAILABLE'), 'OK', { duration: 3000 });
      return;
    }

    const filePath = node.fullPath;
    this.snackBar.open(this.translate.instant('MD_TREE.CREATING_P2P_SHARE'), '', { duration: 0 });

    this.p2pService.shareFile(filePath, node.name).subscribe({
      next: (result) => {
        if (result.success && result.magnetUri) {
          // Copy magnet link to clipboard
          this.clipboard.copy(result.magnetUri);
          this.snackBar.open(this.translate.instant('MD_TREE.MAGNET_COPIED_SHARING'), 'OK', { duration: 5000 });
        } else {
          this.snackBar.open(this.translate.instant('MD_TREE.SHARE_ERROR', { error: result.error || 'Unknown error' }), 'OK', { duration: 5000 });
        }
      },
      error: (err) => {
        console.error('[MdTree] P2P share error:', err);
        this.snackBar.open(this.translate.instant('MD_TREE.SHARE_ERROR', { error: err.message }), 'OK', { duration: 5000 });
      }
    });
  }

  /**
   * Add an external file to the project and share it via P2P.
   * Opens a file picker, copies the selected file to .p2pshare/files/,
   * starts seeding it, and appends a P2P link to the current markdown document.
   * @param node The markdown file where the P2P link will be appended
   */
  addFileToShareViaP2P(node: MdFile): void {
    if (!this.isP2PAvailable) {
      this.snackBar.open(this.translate.instant('MD_TREE.P2P_NOT_AVAILABLE'), 'OK', { duration: 3000 });
      return;
    }

    const documentPath = node.fullPath;

    // Open file picker dialog
    const data = new ShowFileMetadata();
    data.title = 'Select file to share via P2P';
    data.typeOfSelection = 'FoldersAndFiles';
    data.buttonText = 'Add and Share';
    data.start = 'root'; // Start from root to allow selection from anywhere

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '900px',
      height: '700px',
      data: data
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.data) {
        const sourcePath = result.data;

        // Show progress snackbar
        const snackbarRef = this.snackBar.open(this.translate.instant('MD_TREE.COPYING_SHARING'), '', { duration: 0 });

        this.p2pService.copyAndShareFile(sourcePath, documentPath).subscribe({
          next: (shareResult) => {
            snackbarRef.dismiss();

            // Show success message with option to copy magnet link
            const successSnackbarRef = this.snackBar.open(
              'File shared! Link added to document.',
              'Copy Magnet',
              { duration: 10000 }
            );

            successSnackbarRef.onAction().subscribe(() => {
              if (shareResult.magnetUri) {
                this.clipboard.copy(shareResult.magnetUri);
                this.snackBar.open(this.translate.instant('MD_TREE.MAGNET_COPIED'), 'OK', { duration: 2000 });
              }
            });

            // Refresh the document to show the new link
            // Trigger a reload of the current document
            this.mdFileService.setSelectedMdFileFromSideNav(node);
          },
          error: (err) => {
            snackbarRef.dismiss();
            console.error('[MdTree] P2P copy-and-share error:', err);
            const errorMessage = err.error?.error || err.message || 'Unknown error';
            this.snackBar.open(this.translate.instant('MD_TREE.SHARE_ERROR', { error: errorMessage }), 'OK', { duration: 5000 });
          }
        });
      }
    });
  }

  indexFileForRag(node: MdFile): void {
    const project = this.projectsService.currentProjects$.value;
    if (!project) return;

    // Notify title bar: indexing started
    this.mdServerMessages.ragIndexingProgress$.next({
      status: 'processing', processed: 0, total: 1,
      message: `Indexing ${node.name}...`
    });

    this.projectSettingsService.indexRagFile(node.fullPath, project.path).subscribe({
      next: (result) => {
        const msg = result.skipped
          ? (result.message || 'File already up to date')
          : (result.message || `Indexed ${result.chunksEmbedded} chunks`);
        this.snackBar.open(msg, 'OK', { duration: 3000 });
        // Notify title bar: done
        this.mdServerMessages.ragIndexingProgress$.next({
          status: 'completed', processed: 1, total: 1, message: msg
        });
      },
      error: (err) => {
        console.error('[MdTree] RAG index file error:', err);
        const errMsg = err.error?.error || err.message;
        this.snackBar.open(this.translate.instant('COMMON.ERROR') + ': ' + errMsg, 'OK', { duration: 5000 });
        this.mdServerMessages.ragIndexingProgress$.next({
          status: 'error', processed: 0, total: 1, message: 'Error: ' + errMsg
        });
      }
    });
  }

  indexDirectoryForRag(node: MdFile): void {
    const project = this.projectsService.currentProjects$.value;
    if (!project) return;

    this.snackBar.open(this.translate.instant('MD_TREE.DIR_RAG_STARTING'), '', { duration: 3000 });
    this.projectSettingsService.indexRagDirectory(node.fullPath, project.path).subscribe({
      next: (result) => {
        if (result.started) {
          this.snackBar.open(this.translate.instant('MD_TREE.DIR_RAG_STARTED'), 'OK', { duration: 3000 });
        }
      },
      error: (err) => {
        console.error('[MdTree] RAG index directory error:', err);
        if (err.status === 409) {
          this.snackBar.open(this.translate.instant('MD_TREE.RAG_ALREADY_IN_PROGRESS'), 'OK', { duration: 5000 });
        } else {
          this.snackBar.open(this.translate.instant('COMMON.ERROR') + ': ' + (err.error?.error || err.message), 'OK', { duration: 5000 });
        }
      }
    });
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

    // Setup sticky scroll listener
    setTimeout(() => this.setupStickyScroll(), 200);

    // Registra i listener per TOC Generation progress
    this.mdServerMessages.addTocGenerationProgressListener((data, objectThis) => {
      objectThis.tocProgressService.updateProgress(data);
    }, this);

    this.mdServerMessages.addTocGenerationCompleteListener((data, objectThis) => {
      objectThis.tocProgressService.hideProgress();
      objectThis.snackBar.open(objectThis.translate.instant('MD_TREE.TOC_GENERATED_SUCCESS'), 'OK', { duration: 3000 });
    }, this);

    // Registra i listener per Bulk Export progress
    this.mdServerMessages.addBulkExportProgressListener((data, objectThis) => {
      objectThis.bulkExportProgressService.updateProgress(data);
    }, this);

    this.mdServerMessages.addBulkExportCompleteListener((data, objectThis) => {
      objectThis.bulkExportProgressService.complete(data);
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
    // SOLO match per fullPath esatto (case-insensitive, compact-aware).
    // Il vecchio fallback "cerca per nome file" restituiva il primo omonimo
    // ovunque nell'albero: sotto raffiche di eventi (path nuovi non ancora nel
    // tree) faceva agganciare/cancellare il nodo SBAGLIATO. I rename veri
    // arrivano con oldFullPath esplicito — non serve indovinare per nome.
    return this.searchInNodes(this.dataSource.data as MdFile[], path);
  }

  private searchInNodes(nodes: MdFile[], targetPath: string): MdFile | null {
    const normalizedTarget = targetPath?.toLowerCase();
    for (const node of nodes) {
      // Match diretto (case-insensitive)
      if (node.fullPath?.toLowerCase() === normalizedTarget) {
        return node;
      }
      // Match su segmenti compattati (per nodi VS Code-style compact)
      if (node.isCompacted && node.compactedSegments) {
        const matchesSegment = node.compactedSegments.some(
          seg => seg.fullPath?.toLowerCase() === normalizedTarget
        );
        if (matchesSegment) return node;
      }
      if (node.childrens && node.childrens.length > 0) {
        const found = this.searchInNodes(node.childrens, targetPath);
        if (found) return found;
      }
    }
    return null;
  }
  
  /**
   * Dirname cross-separator: gestisce sia '\\' (Windows) sia '/' (Linux).
   * Sostituisce i vari `fullPath.substring(0, fullPath.lastIndexOf('\\'))`
   * sparsi che su Linux restituivano '' rompendo l'aggancio al parent.
   */
  private getParentDirPath(fullPath: string): string {
    const sep = Math.max(fullPath.lastIndexOf('\\'), fullPath.lastIndexOf('/'));
    return sep > 0 ? fullPath.substring(0, sep) : '';
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
  
  // TrackBy function per il rendering dell'albero.
  // DEVE restituire una chiave STABILE e UNICA per nodo: il fullPath identifica
  // univocamente ogni nodo dell'albero (verificato: i fullPath sono tutti distinti).
  // NON includere `index`: lo renderebbe dipendente dalla posizione, e tra
  // un'emissione di dataSource.data e la successiva Material non riconcilierebbe
  // le righe → lascerebbe nel DOM le righe vecchie e ne appenderebbe di nuove,
  // duplicando l'intera struttura (copia "morta" + copia "viva").
  trackByPath(index: number, node: MdFile): string {
    return node.fullPath || `${node.path || ''}_${node.level || 0}`;
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
        // Match SOLO per path: il vecchio criterio `node.name === ...` espandeva
        // la prima cartella omonima ovunque fosse nell'albero.
        const treeNode = this.treeControl.dataNodes.find(node =>
          node.path === folderToExpand.path ||
          node.fullPath === folderToExpand.fullPath ||
          node.relativePath === folderToExpand.relativePath
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
  /**
   * Apre la snackbar custom "Building knowledge" in basso a destra.
   * No-op se ne esiste già una. Il contenuto (IndexingProgressSnackComponent)
   * si abbevera da IndexingProgressService — non vanno chiamati update qui:
   * la progress avanza via setProgress() / setComplete() del service.
   *
   * Stile: la classe `.indexing-progress-snackbar` in styles.scss controlla
   * margine, width e padding del contenitore Material.
   */
  private openIndexingSnackbar(): void {
    if (this.currentSnackbarRef) return;

    this.currentSnackbarRef = this.snackBar.openFromComponent(
      IndexingProgressSnackComponent,
      {
        duration: 0, // Vive finché parsingProjectStop non innesca dismiss
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        panelClass: ['indexing-progress-snackbar']
      }
    );

    this.currentSnackbarRef.afterDismissed().subscribe(() => {
      this.currentSnackbarRef = null;
    });
  }

  // RIMOSSI: showIndexingSnackbar e updateSnackbarContent.
  // Prima venivano chiamati per ogni folderIndexingComplete (1 snackbar nuova
  // o un update con dismiss+riapri di fallback) → flicker visivo.
  // Adesso la snackbar è UNA sola, aperta a parsingProjectStart, aggiornata
  // tramite IndexingProgressService dal knowledgeProgress event.

  // ── Event Queue with debounce + batching ──

  private enqueueEvent(handler: () => void, label: string = ''): void {
    this.eventQueue.push({ handler, label });

    // Reset debounce timer: wait for more events before processing
    if (this.batchTimer) {
      clearTimeout(this.batchTimer);
    }
    this.batchTimer = setTimeout(() => {
      this.batchTimer = null;
      this.processEventBatch();
    }, this.BATCH_DEBOUNCE_MS);
  }

  private processEventBatch(): void {
    if (this.isProcessingQueue || this.eventQueue.length === 0) return;
    this.isProcessingQueue = true;

    try {
      // Take all pending events
      const batch = this.eventQueue.splice(0);

      // Process all events incrementally — no loadAll() fallback
      for (const item of batch) {
        try {
          item.handler();
        } catch (err) {
          console.error('Error processing queued event:', err);
        }
      }
      // Single change detection cycle for the whole batch
      this.changeDetectorRef.markForCheck();
    } finally {
      // Without the finally an exception here left isProcessingQueue=true
      // forever and the queue stopped being processed for the session.
      this.isProcessingQueue = false;
    }
  }

  private clearEventQueue(): void {
    this.eventQueue.length = 0;
    if (this.batchTimer) {
      clearTimeout(this.batchTimer);
      this.batchTimer = null;
    }
  }

  /**
   * Processes any queued events RIGHT NOW instead of discarding them.
   * Used when a fileSystemStorm batch arrives: the pre-threshold individual
   * events sitting in the queue are NOT part of the storm payload (the backend
   * batches only post-threshold ones), so dropping them lost changes.
   */
  private flushEventQueue(): void {
    if (this.batchTimer) {
      clearTimeout(this.batchTimer);
      this.batchTimer = null;
    }
    this.processEventBatch();
  }

  // ── Storm batch processing ──

  private processStormChanges(changes: any[]): void {
    for (const change of changes) {
      try {
        const action = change.action; // 'created', 'deleted', 'changed', 'renamed'
        const isDir = change.isDirectory;

        switch (action) {
          case 'created':
            if (isDir) {
              this.handleFolderCreated(change);
            } else {
              this.handleNewMarkdownFileCreated(change);
            }
            break;

          case 'deleted':
            if (isDir) {
              this.handleFolderDeleted(change);
            } else {
              this.handleMarkdownFileDeleted(change);
            }
            break;

          case 'changed':
            // Content change — no tree structure update needed.
            // The markdownfileischanged event handles editor reload if the file is open.
            break;

          case 'renamed':
            if (isDir) {
              const oldPath = change.oldFullPath || change.OldFullPath;
              const newPath = change.fullPath || change.FullPath;
              if (oldPath && newPath) {
                this.mdFileService.renameFolderInDataStore(oldPath, newPath);
              }
            } else {
              // File renamed = delete old + create new
              if (change.oldFullPath) {
                this.handleMarkdownFileDeleted({ fullPath: change.oldFullPath, name: '' });
              }
              this.handleNewMarkdownFileCreated(change);
            }
            break;
        }
      } catch (err) {
        console.error('Error processing storm change:', change, err);
      }
    }
    this.changeDetectorRef.markForCheck();
  }

  // ── SignalR event handlers ──

  // Gestisce la creazione di un nuovo file markdown
  private handleNewMarkdownFileCreated(fileData: any): void {
    // Skip if this event is from our own DnD move (FSW buffered event leak)
    if (this.dndMovingPaths && fileData.fullPath?.toLowerCase() === this.dndMovingPaths.newPath?.toLowerCase()) {
      console.log('[DnD] Suppressing FSW-leaked markdownFileCreated for:', fileData.fullPath);
      return;
    }

    console.log('🆕 [handleNewMarkdownFileCreated] INIZIO - fileData ricevuto:', JSON.stringify(fileData, null, 2));

    // STEP 1: Controlla se il file esiste già (caso rinominazione)
    const existingFile = this.findNodeByPath(fileData.fullPath);
    console.log('🔍 [STEP 1] existingFile trovato:', existingFile);

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

    // STEP 2: Converte i dati ricevuti in un oggetto MdFile
    const newMdFile = {
      name: fileData.name,
      fullPath: fileData.fullPath,
      path: fileData.path,
      relativePath: fileData.relativePath,
      type: fileData.type,
      level: fileData.level,
      expandable: fileData.expandable,
      isIndexed: fileData.isIndexed ?? true,
      indexingStatus: fileData.indexingStatus ?? 'completed',
      childrens: []
    };

    // STEP 3: Inserimento diretto nel parent folder (gestisce compact folders)
    const parentDirPath = this.getParentDirPath(fileData.fullPath);
    const added = this.mdFileService.addFileToParent(newMdFile as any, parentDirPath);
    console.log('📂 [STEP 3] addFileToParent result:', added, 'parentDirPath:', parentDirPath);

    if (!added) {
      // addFileToParent now handles compact folder breaks and missing folder hierarchy.
      // If it still fails, don't navigate to a file not in the tree (phantom state).
      console.warn('📂 [STEP 3b] addFileToParent failed even after compact-break + hierarchy-create. Skipping navigation.');
      return;
    }

    // STEP 5: Aggiungi il file al Set di tracking (già indicizzato)
    const currentSet = this.indexedFilesSubject.value;
    const newSet = new Set(currentSet);
    newSet.add(newMdFile.fullPath);
    this.indexedFilesSubject.next(newSet);

    // STEP 6: Forza change detection per aggiornare il tree
    this.changeDetectorRef.detectChanges();

    // STEP 7: Crea un MdFile valido per la navigazione
    const mdFileForNavigation: MdFile = {
      name: newMdFile.name,
      path: newMdFile.path,
      relativePath: newMdFile.relativePath,
      fullPath: newMdFile.fullPath,
      fullDirectoryPath: this.getParentDirPath(newMdFile.fullPath),
      type: 'mdFile',
      level: newMdFile.level,
      expandable: false,
      isLoading: false,
      childrens: [],
      index: 0,
      isIndexed: true,
      indexingStatus: 'completed'
    };

    // STEP 8: Espandi il tree fino al file (triggera il subscriber che espande i nodi)
    this.mdFileService.setSelectedMdFileFromServer(mdFileForNavigation);

    // STEP 9: Seleziona il file nel tree
    this.activeNode = mdFileForNavigation;
    this.selectedNode = mdFileForNavigation;
    this.changeDetectorRef.markForCheck();

    // STEP 10: Naviga al documento e imposta il file selezionato
    this.mdFileService.setSelectedMdFileFromSideNav(mdFileForNavigation);
    this.navService.setNewNavigation(mdFileForNavigation);
    this.router.navigate(['/main/navigation/document']);

    // STEP 11: Mostra notifica di successo (dopo la navigazione)
    this.snackBar.open(
      this.translate.instant('MD_TREE.NEW_FILE_CREATED', { name: newMdFile.name }),
      this.translate.instant('COMMON.CLOSE'),
      {
        duration: 3000,
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        panelClass: ['success-snackbar']
      }
    );
  }

  // Costruisce la gerarchia completa per un file
  private buildFileHierarchy(newFile: any): any[] {
    const hierarchy = [];

    // Se il file è nella root, ritorna solo il file
    if (newFile.level === 0) {
      return [newFile];
    }

    // Calcola il base path (project root) sottraendo il relativePath dal fullPath
    // Questo è più robusto che usare indexOf che potrebbe fallire con nomi duplicati
    const relativePath = newFile.relativePath || '';
    let basePath = newFile.fullPath.substring(0, newFile.fullPath.length - relativePath.length);

    // Rimuovi il backslash finale dal basePath per evitare doppi backslash
    if (basePath.endsWith('\\')) {
      basePath = basePath.substring(0, basePath.length - 1);
    }

    // Estrai le parti del path relativo
    const pathParts = relativePath.split('\\').filter(part => part.length > 0);
    let currentRelativePath = '';

    // Aggiungi le cartelle parent (escludi l'ultimo elemento che è il file)
    for (let i = 0; i < pathParts.length - 1; i++) {
      currentRelativePath += '\\' + pathParts[i];
      const folderNode = {
        name: pathParts[i],
        fullPath: basePath + currentRelativePath,
        path: currentRelativePath,
        relativePath: currentRelativePath,
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
    // Skip if this event is from our own DnD move (FSW buffered event leak)
    if (this.dndMovingPaths && fileData.fullPath?.toLowerCase() === this.dndMovingPaths.oldPath?.toLowerCase()) {
      console.log('[DnD] Suppressing FSW-leaked markdownFileDeleted for:', fileData.fullPath);
      return;
    }

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
    
    // Notify other components (e.g., main-content) that this file was deleted
    this.fileEventsService.emitFileDeleted({
      fullPath: fileData.fullPath,
      name: fileData.name || nodeToDelete.name
    });

    // Forza il refresh del componente
    this.changeDetectorRef.markForCheck();

    console.log('✅ [Handler] File rimosso dal tree:', fileData.name);
  }
  
  // Handler per la creazione di una nuova cartella
  private handleFolderCreated(folderData: any): void {
    const fullPath = folderData.fullPath || folderData.FullPath;
    const name = folderData.name || folderData.Name;
    const relativePath = folderData.relativePath || folderData.RelativePath || '';
    const level = folderData.level ?? folderData.Level ?? 0;

    console.log('📁 [handleFolderCreated] Cartella creata:', fullPath);

    // Duplicate check
    const existing = this.findNodeByPath(fullPath);
    if (existing) {
      console.log('⚠️ [handleFolderCreated] Nodo già esistente nel tree:', fullPath);
      return;
    }

    const newFolder: any = {
      name, fullPath, path: relativePath, relativePath,
      type: 'folder', level, expandable: true, childrens: [],
      isIndexed: true, indexingStatus: 'completed'
    };

    const parentPath = this.getParentDirPath(fullPath);
    const added = this.mdFileService.addFileToParent(newFolder, parentPath);

    if (!added) {
      // Parent non è nel tree (cartella mai espansa). Il nodo apparirà quando l'utente espande.
      console.log('📂 [handleFolderCreated] Parent non espanso, la cartella apparirà on-demand:', parentPath);
      return;
    }

    console.log('✅ [handleFolderCreated] Cartella aggiunta al tree:', name);
    this.changeDetectorRef.markForCheck();
  }

  // Handler per la cancellazione di una cartella
  private handleFolderDeleted(folderData: any): void {
    const fullPath = folderData.fullPath || folderData.FullPath;

    console.log('🗑️ [handleFolderDeleted] Cartella eliminata:', fullPath);

    const nodeToDelete = this.findNodeByPath(fullPath);

    if (!nodeToDelete) {
      console.log('⚠️ [handleFolderDeleted] Nodo non trovato nel tree:', fullPath);
      return;
    }

    this.mdFileService.recursiveDeleteFileFromDataStore(nodeToDelete);
    console.log('✅ [handleFolderDeleted] Cartella rimossa dal tree:', nodeToDelete.name);
    this.changeDetectorRef.markForCheck();
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

  /**
   * Capture current expansion state of the tree
   * Returns a Set of fullPath strings for all expanded nodes
   */
  private captureExpansionState(): Set<string> {
    const expandedPaths = new Set<string>();

    if (this.treeControl && this.treeControl.dataNodes) {
      this.treeControl.dataNodes.forEach(node => {
        if (this.treeControl.isExpanded(node)) {
          expandedPaths.add(node.fullPath);
        }
      });
    }

    return expandedPaths;
  }

  /**
   * Restore expansion state from a previous capture
   * Expands all nodes whose fullPath is in the provided Set
   */
  private restoreExpansionState(expandedPaths: Set<string>): void {
    // SYNCHRONOUS, inside the same render transaction as the dataSource.data
    // assignment: MatTreeFlatDataSource flattens synchronously, so dataNodes is
    // already up to date here. The old setTimeout(100) + detectChanges() raced
    // with subsequent renders (and with the user's own expansions) and the
    // synchronous detectChanges was the reentrant-render pattern that corrupts
    // the MatTree differ. markForCheck is enough: the caller schedules CD.
    if (this.treeControl && this.treeControl.dataNodes) {
      this.treeControl.dataNodes.forEach(node => {
        // If this node was expanded before AND still exists, re-expand it
        if (expandedPaths.has(node.fullPath)) {
          this.treeControl.expand(node);
        }
      });
      this.changeDetectorRef.markForCheck();
      console.log('✅ Expansion state restored');
    }
  }

  // ========== Skeleton Loader Helper Methods ==========

  /**
   * Restituisce l'indentazione per l'elemento skeleton (simula gerarchia ad albero)
   */
  getSkeletonIndent(index: number): number {
    // Pattern: 0, 1, 2, 2, 1, 2, 1, 0, 1, 2 (simula struttura ad albero)
    const indentPattern = [0, 1, 2, 2, 1, 2, 1, 0, 1, 2];
    return indentPattern[(index - 1) % indentPattern.length];
  }

  /**
   * Restituisce la larghezza per l'elemento skeleton (variazione per aspetto naturale)
   */
  getSkeletonWidth(index: number): number {
    // Larghezze variabili tra 40% e 80%
    const widthPattern = [65, 55, 45, 70, 60, 40, 75, 50, 80, 55];
    return widthPattern[(index - 1) % widthPattern.length];
  }

  /**
   * Reveal a file in the tree: expand parents, select it, scroll into view with highlight
   */
  private revealAndScrollToNode(targetFile: MdFile): void {
    if (!this.treeControl.dataNodes || this.treeControl.dataNodes.length === 0) {
      this.snackBar.open(this.translate.instant('MD_TREE.TREE_NOT_LOADED'), '', { duration: 2000 });
      return;
    }

    // Find the node in the flat tree (case-insensitive)
    let targetNode = this.treeControl.dataNodes.find(
      n => n.fullPath?.toLowerCase() === targetFile.fullPath?.toLowerCase()
    );

    if (!targetNode) {
      // Defense-in-depth: file might be in dataSource but not yet flattened into dataNodes.
      // Try expanding parents first, then retry.
      const searchResult = this.mdFileService.searchMdFileIntoDataStore(
        this.dataSource.data as MdFile[], targetFile
      );
      if (searchResult && searchResult.length > 0) {
        this.expandToLandingPage(targetFile);
        this.changeDetectorRef.detectChanges();
        // Retry after expansion
        targetNode = this.treeControl.dataNodes.find(
          n => n.fullPath?.toLowerCase() === targetFile.fullPath?.toLowerCase()
        );
      }
      if (!targetNode) {
        this.snackBar.open(this.translate.instant('MD_TREE.FILE_NOT_FOUND'), '', { duration: 3000 });
        return;
      }
    }

    // Expand all parent folders using the existing helper
    this.expandToLandingPage(targetFile);

    // Select the node
    this.activeNode = targetNode;
    this.selectedNode = targetNode as any;
    this.changeDetectorRef.detectChanges();

    // Scroll into view after DOM update
    setTimeout(() => {
      this.scrollNodeIntoView(targetNode);
    }, 100);
  }

  /**
   * Scroll a tree node into view and flash-highlight it
   */
  private scrollNodeIntoView(node: IFileInfoNode): void {
    // Escape the fullPath for use in CSS selector (handle backslashes and special chars)
    const escapedPath = CSS.escape(node.fullPath);
    const el = document.querySelector(`mat-tree-node[data-fullpath="${escapedPath}"]`);

    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' });
      el.classList.add('reveal-highlight');
      setTimeout(() => {
        el.classList.remove('reveal-highlight');
      }, 1500);
    }
  }

  private loadStickyScrollSetting(): void {
    this.projectSettingsService.getStickyScrollSetting().subscribe({
      next: r => { this.stickyScrollEnabled = r.enabled; },
      error: () => { this.stickyScrollEnabled = true; }
    });
  }

  private setupStickyScroll(): void {
    const wrapper = document.querySelector('.tree-scroll-wrapper') as HTMLElement;
    if (!wrapper) return;

    this.scrollSub?.unsubscribe();
    this.scrollSub = fromEvent(wrapper, 'scroll')
      .pipe(auditTime(16))
      .subscribe(() => this.updateStickyAncestors(wrapper));
  }

  private updateStickyAncestors(wrapper: HTMLElement): void {
    if (!this.stickyScrollEnabled) {
      if (this.stickyAncestors.length > 0) {
        this.stickyAncestors = [];
        this.changeDetectorRef.markForCheck();
      }
      return;
    }

    const wrapperRect = wrapper.getBoundingClientRect();
    const effectiveTop = wrapperRect.top;

    const nodeEls = wrapper.querySelectorAll('mat-tree-node[data-fullpath]');
    let firstVisible: Element | null = null;
    for (const el of Array.from(nodeEls)) {
      if (el.getBoundingClientRect().bottom > effectiveTop) {
        firstVisible = el;
        break;
      }
    }

    if (!firstVisible) {
      if (this.stickyAncestors.length > 0) {
        this.stickyAncestors = [];
        this.changeDetectorRef.markForCheck();
      }
      return;
    }

    const fullPath = firstVisible.getAttribute('data-fullpath');
    const node = this.treeControl.dataNodes.find(n => n.fullPath === fullPath);

    if (!node || node.level === 0) {
      if (this.stickyAncestors.length > 0) {
        this.stickyAncestors = [];
        this.changeDetectorRef.markForCheck();
      }
      return;
    }

    // Walk backwards through the flat list to find ancestors at decreasing levels
    const nodeIdx = this.treeControl.dataNodes.indexOf(node);
    const ancestors: IFileInfoNode[] = [];
    let targetLevel = node.level - 1;
    for (let i = nodeIdx - 1; i >= 0 && targetLevel >= 0; i--) {
      const c = this.treeControl.dataNodes[i];
      if (c.level === targetLevel && c.type === 'folder') {
        ancestors.unshift(c);
        targetLevel--;
      }
    }

    // Only trigger markForCheck if ancestors actually changed
    const same = ancestors.length === this.stickyAncestors.length &&
      ancestors.every((a, i) => a.fullPath === this.stickyAncestors[i]?.fullPath);
    if (!same) {
      this.stickyAncestors = ancestors;
      this.changeDetectorRef.markForCheck();
    }
  }

  ngOnDestroy(): void {
    // Tear down EVERY takeUntil(destroy$) subscription (SignalR Subjects, services).
    // Without this, dead component instances kept processing file events.
    this.destroy$.next();
    this.destroy$.complete();

    // Pulisci il timer se esiste
    if (this.updateTimer) {
      clearTimeout(this.updateTimer);
    }

    // Pulisci event queue e batch timer
    this.clearEventQueue();

    // Chiudi snackbar attiva
    if (this.currentSnackbarRef) {
      this.currentSnackbarRef.dismiss();
    }

    // Unsubscribe scroll listener
    this.scrollSub?.unsubscribe();
  }

  // ========== Development Tags Methods ==========

  /**
   * Verifica se un nodo ha un tag specifico
   */
  hasTag(node: MdFile, tag: string): boolean {
    return node.developmentTags?.includes(tag) ?? false;
  }

  /**
   * Verifica se una cartella è marcata come "program"
   */
  isProgramFolder(node: MdFile): boolean {
    return this.hasTag(node, 'program');
  }

  /**
   * Toggle di un tag specifico per una cartella
   */
  toggleTag(node: MdFile, tag: string) {
    console.log('toggleTag called', { node, tag, fullPath: node.fullPath, currentTags: node.developmentTags });
    const currentTags = node.developmentTags ?? [];
    const newTags = currentTags.includes(tag)
      ? currentTags.filter(t => t !== tag)
      : [...currentTags, tag];

    // Get project root from the tree data source
    const projectRoot = this.getProjectRoot();
    if (!projectRoot) {
      console.error('Could not determine project root');
      this.snackBar.open(this.translate.instant('MD_TREE.CANNOT_DETERMINE_ROOT'), 'OK', { duration: 3000 });
      return;
    }

    console.log('Calling setDevelopmentTags', { fullPath: node.fullPath, projectRoot, newTags });
    this.mdFileService.setDevelopmentTags(node, projectRoot, newTags).subscribe({
      next: (response) => {
        console.log('setDevelopmentTags success', response);
        node.developmentTags = newTags;
        const action = newTags.includes(tag) ? 'added' : 'removed';
        this.snackBar.open(this.translate.instant('MD_TREE.TAG_TOGGLED', { tag, action }), 'OK', { duration: 2000 });
        this.changeDetectorRef.markForCheck();
      },
      error: (error) => {
        console.error('setDevelopmentTags error', error);
        this.snackBar.open(this.translate.instant('COMMON.ERROR') + ': ' + error.message, 'OK', { duration: 3000 });
      }
    });
  }

  /**
   * Get the project root from the tree data source
   */
  private getProjectRoot(): string | null {
    const data = this.dataSource.data;
    if (data && data.length > 0) {
      // Look for the node with name 'root'
      const rootNode = data.find(node => node.name === 'root');
      if (rootNode && rootNode.fullPath) {
        console.log('Found root node:', rootNode.fullPath);
        return rootNode.fullPath;
      }
      // Fallback to first node if 'root' not found
      const firstNode = data[0];
      if (firstNode && firstNode.fullPath) {
        console.log('Using first node as root:', firstNode.fullPath);
        return firstNode.fullPath;
      }
    }
    return null;
  }

}
