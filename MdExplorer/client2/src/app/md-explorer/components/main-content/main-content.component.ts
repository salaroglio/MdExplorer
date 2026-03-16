import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, HostListener, Input, NgZone, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';
import { DomSanitizer } from '@angular/platform-browser';
import { BehaviorSubject, Observable, Subject, timer, combineLatest } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged, map, filter } from 'rxjs/operators';

import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { IndexingStateService } from '../../services/indexing-state.service';
import { FileEventsService } from '../../services/file-events.service';
import { P2PService, PeerStatus, P2PFileInfo } from '../../../services/p2p.service';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { ProjectsService } from '../../services/projects.service';
import { HttpClient } from '@angular/common/http';

// Content state interface for managing loading, error, and success states
interface ContentState {
  status: 'idle' | 'loading' | 'loaded' | 'error' | 'indexing' | 'deleted';
  currentPath?: string;
  errorMessage?: string;
  deletedFileName?: string;
  isIndexing?: boolean;
  loadingStartTime?: Date;
  retryCount?: number;
}


@Component({
  selector: 'app-main-content',
  templateUrl: './main-content.component.html',
  styleUrls: ['./main-content.component.scss']
})
export class MainContentComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('iframe', { static: false }) iframe: ElementRef<HTMLIFrameElement>;
  public classForContent: string = "hundredPercentContent";

  // Legacy properties for backward compatibility
  mdFile: MdFile;
  html: string;
  htmlSource: string = '../welcome.html';
  public _HideIFrame = false;

  // New state management properties
  private destroy$ = new Subject<void>();
  private contentState$ = new BehaviorSubject<ContentState>({
    status: 'idle',
    retryCount: 0
  });

  // Public observables for template binding
  public readonly isLoading$: Observable<boolean>;
  public readonly hasError$: Observable<boolean>;
  public readonly errorMessage$: Observable<string>;
  public readonly currentState$: Observable<ContentState>;

  // Performance tracking
  private loadingStartTime?: Date;
  private maxRetries = 3;
  private retryDelay = 1000; // Start with 1 second delay
  private iframeListenersAdded = false;
  
  // Layout tracking - RIMOSSO, usiamo solo CSS

  constructor(
    private service: MdFileService,
    private sanitizer: DomSanitizer,
    private monitorMDService: MdServerMessagesService,
    public dialog: MatDialog,
    private ref: ChangeDetectorRef,
    private indexingStateService: IndexingStateService,
    private fileEventsService: FileEventsService,
    private p2pService: P2PService,
    private snackBar: MatSnackBar,
    private projectsService: ProjectsService,
    private http: HttpClient
  ) {
    
    // Initialize observables from state
    this.currentState$ = this.contentState$.asObservable();
    this.isLoading$ = this.contentState$.pipe(
      map(state => {
        const isLoading = state.status === 'loading';
        return isLoading;
      }),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    );
    
    this.hasError$ = this.contentState$.pipe(
      map(state => {
        const hasError = state.status === 'error';
        return hasError;
      }),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    );
    
    this.errorMessage$ = this.contentState$.pipe(
      map(state => state.errorMessage || 'Errore di caricamento sconosciuto'),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    );

    this.monitorMDService.addMarkdownFileListener(this.markdownFileIsChanged, this);
  }

  ngOnInit(): void {
    // Initialize P2P message listener for iframe communication
    this.setupP2PMessageListener();

    // Enhanced subscription with loading state management
    this.service.selectedMdFileFromSideNav.pipe(
      takeUntil(this.destroy$),
      debounceTime(100) // Prevent rapid-fire selections
    ).subscribe(file => {
      if (file) {
        this.loadMarkdownFile(file);
      }
    });

    this.service.selectedMdFileFromToolbar.pipe(
      takeUntil(this.destroy$),
      debounceTime(100)
    ).subscribe(files => {
      const current = files?.[0];
      if (current) {
        this.loadMarkdownFile(current);
      }
    });

    // Integration with IndexingStateService for file indexing awareness
    this.indexingStateService.indexedFiles$.pipe(
      takeUntil(this.destroy$),
      filter(indexedFiles => {
        const currentPath = this.contentState$.value.currentPath;
        return !!currentPath && indexedFiles.has(currentPath);
      })
    ).subscribe(indexedFiles => {
      const currentPath = this.contentState$.value.currentPath;
      if (currentPath) {
        const indexingState = indexedFiles.get(currentPath);
        this.handleIndexingStateChange(currentPath, indexingState);
      }
    });

    // Listen to file indexed events
    this.fileEventsService.fileIndexed$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(event => {
      const currentPath = this.contentState$.value.currentPath;
      if (event.fullPath === currentPath) {
        // Refresh the view if the current file was just indexed
        this.handleFileIndexingComplete(event);
      }
    });

    // Listen to file renamed events
    this.fileEventsService.fileRenamed$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(event => {
      const currentPath = this.contentState$.value.currentPath;
      if (event.oldPath === currentPath) {
        this.handleFileRenamed(event.oldPath, event.newPath);
      }
    });

    // Listen to file deleted events
    this.fileEventsService.fileDeleted$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(event => {
      this.handleFileDeleted(event.fullPath, event.name);
    });
    
    // Subscribe to layout changes - RIMOSSO per usare solo CSS
    // this.layoutService.sidenavWidth$.pipe(
    //   takeUntil(this.destroy$)
    // ).subscribe(width => {
    //   this.sidenavWidth = width;
    //   this.adjustIframeWidth();
    // });
  }

  ngAfterViewInit(): void {
    // Set up iframe event listeners after view initialization
    // Use setTimeout to ensure iframe is in DOM after initial render
    setTimeout(() => {
      this.setupIframeEventListeners();
    }, 100);
    
    // Rimosso window resize listener - usiamo CSS flexbox invece
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Global keyboard shortcut handler - intercepts Ctrl+F to trigger iframe search
   */
  @HostListener('window:keydown', ['$event'])
  handleGlobalKeydown(event: KeyboardEvent): void {
    // IMPORTANTE: Exit immediato per tutti i tasti che non sono Ctrl+F
    // per evitare interferenze con l'editor React/CodeMirror
    if (!((event.ctrlKey || event.metaKey) && event.key === 'f')) {
      return;
    }

    // Solo Ctrl+F arriva qui
    event.preventDefault();
    event.stopPropagation();

    // Communicate with iframe to trigger search
    const iframeWindow = this.iframe?.nativeElement?.contentWindow;
    if (iframeWindow) {
      try {
        iframeWindow.postMessage({ action: 'toggleSearch' }, '*');
      } catch (error) {
        console.error('[MainContent] Error communicating with iframe:', error);
      }
    }
  }

  /**
   * Enhanced file loading with state management and error handling
   */
  private loadMarkdownFile(file: MdFile): void {
    // Reset deleted state when loading a new file
    this._HideIFrame = false;

    if (!file?.relativePath) {
      const fileInfo = file?.fullPath || file?.name || 'unknown';
      console.warn('⚠️ [MainContent] Invalid file provided for loading - missing relativePath. File info:', fileInfo);
      this.updateState({
        status: 'error',
        errorMessage: `Impossibile caricare il file: path relativo mancante. File: ${fileInfo}`,
        currentPath: undefined
      });
      return;
    }

    
    // Update state to loading
    this.loadingStartTime = new Date();
    this.updateState({
      status: 'loading',
      currentPath: file.relativePath,
      loadingStartTime: this.loadingStartTime,
      errorMessage: undefined
    });

    // Call the original controller method but with enhanced URL building
    this.callMdExplorerController(file);
  }

  /**
   * Remove leading slashes/backslashes from path to prevent double slash in URL
   */
  private cleanRelativePath(path: string): string {
    return path?.replace(/^[\/\\]+/, '') ?? '';
  }

  /**
   * Legacy method maintained for backward compatibility
   */
  private callMdExplorerController(node: MdFile): void {
    if (node?.relativePath) {
      const dateTime = new Date().getTime() / 1000;
      const cleanPath = this.cleanRelativePath(node.relativePath);
      const newHtmlSource = `../api/mdexplorer/${cleanPath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}&source=angular`;

      // Only update if URL actually changed to prevent unnecessary reloads
      if (this.htmlSource !== newHtmlSource) {
        this.htmlSource = newHtmlSource;
        
        // Force change detection to ensure iframe updates
        this.ref.detectChanges();
        
        // Set up event listeners after URL change and iframe becomes visible
        setTimeout(() => {
          this.setupIframeEventListeners();
        }, 200);
      }
    }
  }

  private markdownFileIsChanged(data: any, objectThis: MainContentComponent): void {
    // Client-side guard: skip automatic refresh (from FileSystemWatcher) if user disabled autoload.
    // User-initiated events (editor save, paste, screenshot) have no source field and are always allowed.
    const isWatcherEvent = data?.source === 'watcher' || data?.Source === 'watcher';
    if (isWatcherEvent && localStorage.getItem('mdexplorer_autoload_disabled') === 'true') {
      console.log('[MainContent] ⏸️ Autoload disabled by user, ignoring watcher file change event');
      return;
    }

    try {
      // Extract relative path with multiple fallbacks
      const relativePath = data.relativePath || data.RelativePath || data.path || data.Path;
      if (!relativePath) {
        console.error('❌ [MainContent] No relative path found in data:', data);
        objectThis.updateState({
          status: 'error',
          errorMessage: 'Percorso file non trovato nell\'evento ricevuto'
        });
        return;
      }

      // Update service state (legacy compatibility)
      objectThis.service.navigationArray = [];
      objectThis.service.setSelectedMdFileFromServer(data);
      objectThis.service.setSelectedMdFileFromSideNav(data);

      // Create file object and trigger loading
      const fileData: MdFile = {
        relativePath: relativePath.replace(/\\/g, '/'),
        // Add other properties from data if available
        ...data
      };

      objectThis.loadMarkdownFile(fileData);
      
    } catch (error) {
      console.error('💥 [MainContent] Error processing file change:', error);
      objectThis.updateState({
        status: 'error',
        errorMessage: `Errore durante l'aggiornamento del file: ${error.message}`
      });
    }
  }

  /**
   * Set up iframe event listeners for load/error detection
   */
  private setupIframeEventListeners(): void {
    if (!this.iframe?.nativeElement) {
      console.warn('⚠️ [MainContent] Iframe element not available for event setup');
      return;
    }

    // Avoid adding listeners multiple times
    if (this.iframeListenersAdded) {
      return;
    }

    const iframeElement = this.iframe.nativeElement;

    // Create bound functions to allow removal later
    const loadHandler = () => {
      const loadTime = this.loadingStartTime ? new Date().getTime() - this.loadingStartTime.getTime() : 0;
      
      this.updateState({
        status: 'loaded',
        errorMessage: undefined,
        retryCount: 0,
        isIndexing: false
      });
      
      // Rimosso adjustIframeWidth - usiamo CSS flexbox invece
    };

    const errorHandler = (event: Event) => {
      console.error('❌ [MainContent] Iframe failed to load:', event);
      
      this.updateState({
        status: 'error',
        errorMessage: 'Impossibile caricare il documento. Verifica la connessione e riprova.',
        isIndexing: false
      });
    };

    // Add event listeners
    iframeElement.addEventListener('load', loadHandler);
    iframeElement.addEventListener('error', errorHandler);

    this.iframeListenersAdded = true;

    // Clean up on destroy
    this.destroy$.subscribe(() => {
      if (this.iframe?.nativeElement) {
        this.iframe.nativeElement.removeEventListener('load', loadHandler);
        this.iframe.nativeElement.removeEventListener('error', errorHandler);
      }
    });
  }

  /**
   * Update component state and trigger change detection
   */
  private updateState(partialState: Partial<ContentState>): void {
    const currentState = this.contentState$.value;
    const newState = { ...currentState, ...partialState };
    
    
    this.contentState$.next(newState);
    this.ref.detectChanges();
  }

  /**
   * Public method to retry loading current file
   */
  public retry(): void {
    const currentState = this.contentState$.value;
    
    if (currentState.retryCount >= this.maxRetries) {
      console.warn('⚠️ [MainContent] Maximum retry attempts reached');
      this.updateState({
        status: 'error',
        errorMessage: 'Numero massimo di tentativi raggiunto. Controlla la connessione.'
      });
      return;
    }

    
    // Exponential backoff for retries
    const delay = this.retryDelay * Math.pow(2, currentState.retryCount);
    
    timer(delay).subscribe(() => {
      this.updateState({
        status: 'loading',
        retryCount: (currentState.retryCount || 0) + 1,
        errorMessage: undefined
      });

      // Force iframe reload by modifying URL
      if (currentState.currentPath) {
        const dateTime = new Date().getTime() / 1000;
        const cleanPath = this.cleanRelativePath(currentState.currentPath);
        this.htmlSource = `../api/mdexplorer/${cleanPath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}&source=angular&retry=${currentState.retryCount + 1}`;
      }
    });
  }

  /**
   * Handle indexing state changes for the current file
   */
  private handleIndexingStateChange(filePath: string, indexingState: any): void {
    
    if (indexingState?.indexingStatus === 'indexing') {
      // Show indexing state if file is currently being indexed
      this.updateState({
        status: 'indexing',
        isIndexing: true
      });
    } else if (indexingState?.indexingStatus === 'completed' && indexingState?.isIndexed) {
      // File indexing completed, refresh if currently showing loading
      const currentStatus = this.contentState$.value.status;
      if (currentStatus === 'indexing' || currentStatus === 'loading') {
        this.refreshCurrentFile();
      }
    }
  }

  /**
   * Handle file indexing completion event
   */
  private handleFileIndexingComplete(event: any): void {
    
    if (event.isIndexed) {
      // Mark file as indexed and refresh if needed
      const currentStatus = this.contentState$.value.status;
      if (currentStatus === 'indexing') {
        this.refreshCurrentFile();
      }
    }
  }

  /**
   * Handle file rename event for current file
   */
  private handleFileRenamed(oldPath: string, newPath: string): void {

    // Update current path in state
    this.updateState({
      currentPath: newPath
    });

    // Update the URL to point to the new path
    const dateTime = new Date().getTime() / 1000;
    const cleanPath = this.cleanRelativePath(newPath);
    this.htmlSource = `../api/mdexplorer/${cleanPath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}&source=angular`;
  }

  /**
   * Handle file deleted event — show "file deleted" message if the deleted file is currently being viewed
   */
  private handleFileDeleted(fullPath: string, name: string): void {
    const currentPath = this.contentState$.value.currentPath;
    if (!currentPath) return;

    // Compare: currentPath is relative, fullPath is absolute — normalize both for comparison
    const normalizedCurrent = currentPath.replace(/\\/g, '/').replace(/^\/+/, '').toLowerCase();
    const normalizedDeleted = fullPath.replace(/\\/g, '/').toLowerCase();

    if (normalizedDeleted.endsWith(normalizedCurrent) || normalizedCurrent.endsWith(normalizedDeleted)) {
      this._HideIFrame = true;
      this.updateState({
        status: 'deleted',
        deletedFileName: name || fullPath.split(/[/\\]/).pop() || 'Unknown file'
      });
    }
  }

  /**
   * Refresh the current file by reloading the iframe
   */
  private refreshCurrentFile(): void {
    const currentPath = this.contentState$.value.currentPath;
    if (currentPath) {

      this.updateState({
        status: 'loading',
        isIndexing: false
      });

      // Force reload with new timestamp
      const dateTime = new Date().getTime() / 1000;
      const cleanPath = this.cleanRelativePath(currentPath);
      this.htmlSource = `../api/mdexplorer/${cleanPath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}&source=angular&refreshed=true`;
    }
  }

  /**
   * Check if current file is being indexed
   */
  public isCurrentFileIndexing(): boolean {
    const currentPath = this.contentState$.value.currentPath;
    if (!currentPath) return false;

    const indexingState = this.indexingStateService.isFileIndexed(currentPath);
    return !indexingState; // If not indexed, it might be indexing
  }
  
  // RIMOSSI i metodi adjustIframeWidth e setupWindowResizeListener - usiamo solo CSS

  /**
   * Enhanced loading with indexing awareness
   */
  private loadMarkdownFileWithIndexingCheck(file: MdFile): void {
    const fullPath = file.relativePath;
    if (!fullPath) return;

    // Check indexing status before loading
    const isIndexed = this.indexingStateService.isFileIndexed(fullPath);
    const isWaiting = this.indexingStateService.isFileWaiting(fullPath, 'mdFile');

    console.log('Indexing status check:', {
      path: fullPath,
      isIndexed,
      isWaiting
    });

    if (isWaiting) {
      // File is waiting for indexing
      this.updateState({
        status: 'indexing',
        currentPath: fullPath,
        isIndexing: true,
        errorMessage: undefined
      });
    } else {
      // File is ready, proceed with normal loading
      this.loadMarkdownFile(file);
    }
  }

  /**
   * Debug method to manually test state transitions
   */
  public debugTestLoadingComplete(): void {
    this.updateState({
      status: 'loaded',
      errorMessage: undefined,
      retryCount: 0,
      isIndexing: false
    });
  }

  /**
   * Debug method to check current state
   */
  public debugCurrentState(): void {
    const currentState = this.contentState$.value;
  }

  /**
   * Setup listener for P2P messages from iframe
   */
  private setupP2PMessageListener(): void {
    console.log('[P2P Angular] Setting up P2P message listener');
    window.addEventListener('message', (event: MessageEvent) => {
      if (!event.data || !event.data.type) return;

      // Only log P2P messages
      if (event.data.type.startsWith('p2p-')) {
        console.log('[P2P Angular] Received message from iframe:', event.data.type, event.data);
      }

      switch (event.data.type) {
        case 'p2p-link-click':
          this.handleP2PLinkClick(event.data);
          break;
        case 'p2p-link-hover':
          this.handleP2PLinkHover(event.data);
          break;
        case 'md-navigate':
          this.handleMdNavigate(event.data);
          break;
      }
    });
  }

  /**
   * Handle click on a P2P link from iframe
   */
  private handleP2PLinkClick(data: { href: string; filename: string; projectPath: string }): void {
    const projectPath = this.projectsService.currentProjects$.getValue()?.path || data.projectPath;

    if (!projectPath) {
      console.error('[P2P] No project path available');
      this.snackBar.open('Errore: progetto non trovato', 'OK', { duration: 3000 });
      return;
    }

    // First, check if the file exists locally
    this.p2pService.checkFile(data.href, projectPath).subscribe({
      next: (result) => {
        if (result.exists) {
          // File exists locally, open it
          console.log('[P2P] File exists locally, opening:', result.fullPath);
          this.openLocalFile(result.fullPath);
        } else {
          // File doesn't exist, check P2P metadata and offer download
          this.handleP2PDownloadOffer(data.filename, projectPath);
        }
      },
      error: (err) => {
        console.error('[P2P] Error checking file:', err);
        this.snackBar.open('Errore nel verificare il file', 'OK', { duration: 3000 });
      }
    });
  }

  /**
   * Handle navigation request from iframe (YAML links in PlantUML diagrams).
   * Loads the document once via Angular, avoiding the double-load caused by window.location.href.
   */
  private handleMdNavigate(data: { relativePath: string; name: string }): void {
    const mdFile: MdFile = {
      name: data.name,
      path: data.relativePath,
      relativePath: data.relativePath,
      fullPath: '',
      fullDirectoryPath: '',
      level: 0,
      expandable: false,
      type: 'file',
      index: 0,
      isLoading: false,
      childrens: []
    };
    this.loadMarkdownFile(mdFile);
  }

  /**
   * Handle hover on a P2P link - send status back to iframe
   */
  private handleP2PLinkHover(data: { href: string; filename: string; projectPath: string; linkId: string }): void {
    const projectPath = this.projectsService.currentProjects$.getValue()?.path || data.projectPath;

    console.log('[P2P Angular] handleP2PLinkHover called:', { filename: data.filename, projectPath, linkId: data.linkId });

    if (!projectPath) {
      console.warn('[P2P Angular] No project path available, skipping');
      return;
    }

    // Get file info from metadata
    console.log('[P2P Angular] Calling getFileInfo API...');
    this.p2pService.getFileInfo(data.filename, projectPath).subscribe({
      next: (fileInfo) => {
        console.log('[P2P Angular] getFileInfo response:', fileInfo);
        if (fileInfo.found && fileInfo.infoHash) {
          // Get peer status for this torrent
          console.log('[P2P Angular] Calling getPeerStatus for:', fileInfo.infoHash);
          this.p2pService.getPeerStatus(fileInfo.infoHash).subscribe({
            next: (peerStatus) => {
              console.log('[P2P Angular] getPeerStatus response:', peerStatus);
              // Check if file exists locally
              this.p2pService.checkFile(data.href, projectPath).subscribe({
                next: (checkResult) => {
                  console.log('[P2P Angular] checkFile response:', checkResult);
                  // Determine the display state
                  const state = this.determineP2PState(checkResult.exists, peerStatus, fileInfo);

                  // Send status back to iframe
                  this.sendP2PStatusToIframe(data.linkId, {
                    state: state,
                    statusClass: state.replace('_', '-'),
                    numPeers: peerStatus.numPeers,
                    size: fileInfo.size || 0,
                    progress: peerStatus.progress || 0,
                    downloadSpeed: peerStatus.downloadSpeed || 0,
                    uploadSpeed: peerStatus.uploadSpeed || 0
                  });
                }
              });
            },
            error: () => {
              // Can't get peer status, assume unknown
              this.sendP2PStatusToIframe(data.linkId, {
                state: 'unknown',
                statusClass: 'unknown',
                numPeers: 0,
                size: fileInfo.size || 0
              });
            }
          });
        } else {
          // File not in metadata
          this.sendP2PStatusToIframe(data.linkId, {
            state: 'unknown',
            statusClass: 'unknown',
            numPeers: 0,
            size: 0
          });
        }
      },
      error: () => {
        // Error getting file info
        this.sendP2PStatusToIframe(data.linkId, {
          state: 'unknown',
          statusClass: 'unknown',
          numPeers: 0,
          size: 0
        });
      }
    });
  }

  /**
   * Determine the P2P state based on local existence and peer status
   */
  private determineP2PState(existsLocally: boolean, peerStatus: PeerStatus, fileInfo: P2PFileInfo): string {
    if (existsLocally) {
      if (peerStatus.found && peerStatus.status === 'seeding') {
        return 'seeding';
      }
      return 'local';
    } else {
      if (peerStatus.found) {
        if (peerStatus.status === 'downloading') {
          return 'downloading';
        }
        if (peerStatus.numPeers > 0) {
          return 'to_download';
        }
      }
      return 'no_peers';
    }
  }

  /**
   * Send P2P status back to iframe for tooltip update
   */
  private sendP2PStatusToIframe(linkId: string, status: any): void {
    console.log('[P2P Angular] Sending status to iframe:', { linkId, status });
    const iframeWindow = this.iframe?.nativeElement?.contentWindow;
    if (iframeWindow) {
      iframeWindow.postMessage({
        type: 'p2p-link-status',
        linkId: linkId,
        status: status
      }, '*');
      console.log('[P2P Angular] Status sent successfully');
    } else {
      console.warn('[P2P Angular] Could not send status - iframe not available');
    }
  }

  /**
   * Handle download offer for P2P file
   */
  private handleP2PDownloadOffer(filename: string, projectPath: string): void {
    // Get file info to show size and check peer availability
    this.p2pService.getFileInfo(filename, projectPath).subscribe({
      next: (fileInfo) => {
        if (!fileInfo.found || !fileInfo.magnetUri) {
          this.snackBar.open('File non trovato nei metadati P2P', 'OK', { duration: 3000 });
          return;
        }

        // Check peer availability
        if (fileInfo.infoHash) {
          this.p2pService.getPeerStatus(fileInfo.infoHash).subscribe({
            next: (peerStatus) => {
              this.showDownloadConfirmation(filename, fileInfo, peerStatus, projectPath);
            },
            error: () => {
              // Show anyway without peer info
              this.showDownloadConfirmation(filename, fileInfo, null, projectPath);
            }
          });
        } else {
          this.showDownloadConfirmation(filename, fileInfo, null, projectPath);
        }
      },
      error: (err) => {
        console.error('[P2P] Error getting file info:', err);
        this.snackBar.open('Errore nel recuperare le informazioni del file', 'OK', { duration: 3000 });
      }
    });
  }

  /**
   * Show download confirmation dialog/snackbar
   */
  private showDownloadConfirmation(filename: string, fileInfo: P2PFileInfo, peerStatus: PeerStatus | null, projectPath: string): void {
    const sizeStr = this.p2pService.formatBytes(fileInfo.size || 0);
    const peersStr = peerStatus?.numPeers ? `${peerStatus.numPeers} peer disponibili` : 'Peer sconosciuti';

    const snackRef = this.snackBar.open(
      `Scaricare "${filename}"? (${sizeStr}, ${peersStr})`,
      'Scarica',
      { duration: 10000 }
    );

    snackRef.onAction().subscribe(() => {
      this.startP2PDownload(filename, fileInfo, projectPath);
    });
  }

  /**
   * Start P2P download
   */
  private startP2PDownload(filename: string, fileInfo: P2PFileInfo, projectPath: string): void {
    if (!fileInfo.magnetUri) {
      this.snackBar.open('Magnet URI non disponibile', 'OK', { duration: 3000 });
      return;
    }

    // Destination path inside .p2pshare/received/
    const destPath = `${projectPath}/.p2pshare/received`;

    this.snackBar.open(`Avvio download di "${filename}"...`, '', { duration: 2000 });

    this.p2pService.download(fileInfo.magnetUri, destPath).subscribe({
      next: (result) => {
        if (result.success) {
          this.snackBar.open(`Download avviato per "${filename}"`, 'OK', { duration: 3000 });
          // TODO: Could show progress in P2P Manager or a dedicated component
        } else {
          this.snackBar.open(`Errore: ${result.error}`, 'OK', { duration: 5000 });
        }
      },
      error: (err) => {
        console.error('[P2P] Download error:', err);
        this.snackBar.open('Errore durante il download', 'OK', { duration: 5000 });
      }
    });
  }

  /**
   * Open a local file (e.g., using shell open)
   */
  private openLocalFile(fullPath: string): void {
    // For now, just log - actual implementation would use Electron shell.openPath
    // or a backend endpoint to open the file
    console.log('[P2P] Opening local file:', fullPath);
    this.snackBar.open(`Apertura file: ${fullPath}`, '', { duration: 2000 });

    // TODO: Implement actual file opening via backend or Electron IPC
    // For example: this.http.post('/api/System/OpenFile', { path: fullPath }).subscribe();
  }

}
