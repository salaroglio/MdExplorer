import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, NgZone, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';
import { DomSanitizer } from '@angular/platform-browser';
import { BehaviorSubject, Observable, Subject, timer, combineLatest } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged, map, filter } from 'rxjs/operators';

import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { MatDialog } from '@angular/material/dialog';
import { IndexingStateService } from '../../services/indexing-state.service';
import { FileEventsService } from '../../services/file-events.service';

// Content state interface for managing loading, error, and success states
interface ContentState {
  status: 'idle' | 'loading' | 'loaded' | 'error' | 'indexing';
  currentPath?: string;
  errorMessage?: string;
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
  @ViewChild('iframe') iframe: ElementRef<HTMLIFrameElement>;
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
    private fileEventsService: FileEventsService
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
   * Enhanced file loading with state management and error handling
   */
  private loadMarkdownFile(file: MdFile): void {
    if (!file?.relativePath) {
      console.warn('⚠️ [MainContent] Invalid file provided for loading');
      this.updateState({ 
        status: 'error', 
        errorMessage: 'File non valido selezionato',
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
   * Legacy method maintained for backward compatibility
   */
  private callMdExplorerController(node: MdFile): void {
    if (node?.relativePath) {
      const dateTime = new Date().getTime() / 1000;
      const newHtmlSource = `../api/mdexplorer/${node.relativePath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}`;
      
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
        this.htmlSource = `../api/mdexplorer/${currentState.currentPath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}&retry=${currentState.retryCount + 1}`;
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
    this.htmlSource = `../api/mdexplorer/${newPath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}`;
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
      this.htmlSource = `../api/mdexplorer/${currentPath}?time=${dateTime}&connectionId=${this.monitorMDService.connectionId}&refreshed=true`;
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



}
