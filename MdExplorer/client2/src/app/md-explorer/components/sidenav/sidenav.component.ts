
import { AfterViewInit, ChangeDetectorRef, Component, OnInit, OnDestroy, ViewChild, ChangeDetectionStrategy } from '@angular/core'; // Added ChangeDetectionStrategy
import { Router }                                          from '@angular/router';
import { BreakpointObserver, BreakpointState }   from '@angular/cdk/layout';
import { HttpClient } from '@angular/common/http';
import { MdFileService }      from '../../services/md-file.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { GITService } from '../../../git/services/gitservice.service';
import { MatSidenav } from '@angular/material/sidenav';
import { MdFile } from '../../models/md-file';
import { ProjectsService } from '../../services/projects.service';
import { MdNavigationService } from '../../services/md-navigation.service';
import { LayoutService } from '../../services/layout.service';
import { ClipboardPasteService } from '../../services/clipboard-paste.service';



const SMALL_WIDTH_BREAKPOINT = 720;

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss'],

})
export class SidenavComponent implements OnInit, OnDestroy {


  public showText:boolean = false;
  public sideNavWidth: string = "240px";
  public isScreenSmall: boolean;
  public hooked: boolean = false;
  public titleProject: string;
  public currentBranch: string = null;
  public hasRemote: boolean = false;
  public fileSystemWatcherEnabled: boolean = true;
  @ViewChild('sidenav', { static: false }) sidenav: MatSidenav;
  
  // Memory leak prevention
  private mouseMoveListener?: (event: MouseEvent) => void;
  private mouseUpListener?: (event: MouseEvent) => void;
  
  // Performance and validation
  private readonly MIN_WIDTH = 200;
  private readonly MAX_WIDTH = 800;
  private debounceTimer: any;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private mdFileService: MdFileService,
    private router: Router,
    private currentFolder: AppCurrentMetadataService,
    private gitService: GITService,
    private projectService: ProjectsService,
    public navService:MdNavigationService,
    private ref: ChangeDetectorRef, // Injected ChangeDetectorRef
    private layoutService: LayoutService,
    private http: HttpClient,
    private clipboardPasteService: ClipboardPasteService
  ) {
    this.setupResizeListeners();

    // Use ProjectsService as source of truth for project name
    this.projectService.currentProjects$.subscribe(project => {
      console.log('[Sidenav] currentProjects$ emitted:', project);
      if (project && project.name) {
        this.titleProject = project.name;
        // Check if project has remote for Team Chat
        console.log('[Sidenav] project.path:', project.path);
        this.checkRemoteStatus(project.path);
        // Load FileSystemWatcher status when project changes
        this.loadFileSystemWatcherStatus();
      } else {
        console.log('[Sidenav] No project or no name, setting hasRemote=false');
        this.hasRemote = false;
      }
    });

    this.gitService.currentBranch$.subscribe(_ => {
      this.currentBranch = _.name;
    });

    this.currentFolder.showSidenav.subscribe(_ => {
      if (this.sidenav != undefined ) {
        if (_) {
          this.sidenav.open();
        } else {
          this.sidenav.close();
        }
      }
    });

  }

  private setupResizeListeners(): void {
    this.mouseMoveListener = (event: MouseEvent) => {
      if (this.hooked) {
        const newWidth = this.validateWidth(event.clientX);
        this.sideNavWidth = newWidth + "px";
        this.layoutService.setSidenavWidth(newWidth);
      }
    };

    this.mouseUpListener = (event: MouseEvent) => {
      if (this.hooked) {
        this.hooked = false;
        // Restore normal cursor and text selection
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
        const finalWidth = this.validateWidth(event.clientX);
        this.saveWidthDebounced(finalWidth);

        // Notify iframe to refresh auto-fit for images
        this.notifyIframeAutoFitRefresh();
      }
    };

    document.addEventListener("mousemove", this.mouseMoveListener);
    document.addEventListener("mouseup", this.mouseUpListener);
  }

  ngOnDestroy(): void {
    // Remove event listeners to prevent memory leaks
    if (this.mouseMoveListener) {
      document.removeEventListener("mousemove", this.mouseMoveListener);
    }
    if (this.mouseUpListener) {
      document.removeEventListener("mouseup", this.mouseUpListener);
    }
    // Clear debounce timer
    clearTimeout(this.debounceTimer);
    // Cleanup clipboard paste service
    this.clipboardPasteService.destroy();
  }

  private validateWidth(width: number): number {
    return Math.min(Math.max(width, this.MIN_WIDTH), this.MAX_WIDTH);
  }

  private saveWidthDebounced(width: number): void {
    clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => {
      this.projectService.currentProjects$.value.sidenavWidth = width;
      this.projectService.SetSideNavWidth(this.projectService.currentProjects$.value);
    }, 500); // Save only after 500ms of inactivity
  }

  resizeWidth(): void {
    this.hooked = true;
    // Prevent text selection during drag
    document.body.style.userSelect = 'none';
    document.body.style.cursor = 'col-resize';
  }

  stopResizeWidth(): void {





  }

  openProject(): void {
    // Close current project resources on backend BEFORE navigating
    this.projectService.closeCurrentProject().subscribe({
      next: () => console.log('Project closed successfully'),
      error: (err) => console.error('Error closing project:', err),
      complete: () => {
        var mdFile = new MdFile("Welcome to MDExplorer", '/../welcome.html', 0, false);
        mdFile.relativePath = '/../../welcome.html';
        this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
        this.router.navigate(['/projects']);
        this.projectService.currentProjects$.next(null);
      }
    });
  }


  ngOnInit(): void {

    this.breakpointObserver.observe([`(max-width:${SMALL_WIDTH_BREAKPOINT}px)`])
      .subscribe((state: BreakpointState) => {
        this.isScreenSmall = state.matches;
      });

    this.projectService.currentProjects$.subscribe(_ => {
      if (_ != null && _ != undefined) {
        if (_.sidenavWidth != null && _.sidenavWidth != 0) {
          this.sideNavWidth = _.sidenavWidth + "px";
          this.layoutService.setSidenavWidth(_.sidenavWidth);
        }
      }
    });

    // Initialize global Ctrl+V listener for screenshot annotation
    this.clipboardPasteService.initialize();

  }

  forward(): void {

    let navToMdFile = this.navService.forward();
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);

  }

  backward(): void {

    let navToMdFile = this.navService.back();
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);

  }
  
  onSidenavToggle(isOpen: boolean): void {
    this.layoutService.setSidenavOpen(isOpen);

    // Notify iframe to refresh auto-fit after animation completes
    setTimeout(() => {
      this.notifyIframeAutoFitRefresh();
    }, 350);
  }

  /**
   * Notify the markdown iframe to refresh auto-fit for images
   * Called after sidenav resize is complete
   */
  private notifyIframeAutoFitRefresh(): void {
    const iframe = document.getElementById('mdIframe') as HTMLIFrameElement;
    if (iframe?.contentWindow) {
      try {
        iframe.contentWindow.postMessage({ action: 'refreshAutoFit' }, '*');
        console.log('[Sidenav] Sent refreshAutoFit message to iframe');
      } catch (error) {
        console.error('[Sidenav] Error sending message to iframe:', error);
      }
    }
  }

  /**
   * Check if the project has a remote origin configured
   * Used to show/hide Team Chat tab
   */
  private checkRemoteStatus(projectPath: string): void {
    if (!projectPath) {
      console.log('[Sidenav] checkRemoteStatus: no projectPath');
      this.hasRemote = false;
      return;
    }

    console.log('[Sidenav] checkRemoteStatus: checking', projectPath);

    this.http.get<{ hasRemote: boolean }>(`/api/GitChat/room-info?repositoryPath=${encodeURIComponent(projectPath)}`)
      .subscribe({
        next: (response) => {
          console.log('[Sidenav] checkRemoteStatus response:', response);
          this.hasRemote = response?.hasRemote || false;
          console.log('[Sidenav] hasRemote set to:', this.hasRemote);
        },
        error: (err) => {
          console.error('[Sidenav] checkRemoteStatus error:', err);
          this.hasRemote = false;
        }
      });
  }

  /**
   * Load FileSystemWatcher status from backend
   */
  private loadFileSystemWatcherStatus(): void {
    // Check if user had explicitly disabled autoload (survives SignalR reconnection)
    const wasDisabledByUser = localStorage.getItem('mdexplorer_autoload_disabled') === 'true';

    if (wasDisabledByUser) {
      // Re-apply the user's preference to the new watcher (created after reconnection)
      this.fileSystemWatcherEnabled = false;
      this.http.post<{ enabled: boolean }>(
        `/api/mdfiles/ToggleFileSystemWatcher?enabled=false`,
        {}
      ).subscribe({
        next: (response) => {
          console.log('[Sidenav] FileSystemWatcher re-disabled after reconnection:', response.enabled);
        },
        error: (err) => {
          console.error('[Sidenav] Error re-disabling FileSystemWatcher:', err);
        }
      });
      return;
    }

    this.http.get<{ enabled: boolean }>('/api/mdfiles/GetFileSystemWatcherStatus')
      .subscribe({
        next: (response) => {
          this.fileSystemWatcherEnabled = response?.enabled ?? true;
          console.log('[Sidenav] FileSystemWatcher status loaded:', this.fileSystemWatcherEnabled);
        },
        error: (err) => {
          console.error('[Sidenav] Error loading FileSystemWatcher status:', err);
          this.fileSystemWatcherEnabled = true; // Default to enabled
        }
      });
  }

  /**
   * Toggle FileSystemWatcher on/off
   * When disabled, file changes won't trigger auto-reload
   */
  onFileSystemWatcherToggle(): void {
    // Persist preference in localStorage for client-side guard and reconnection recovery
    localStorage.setItem('mdexplorer_autoload_disabled', (!this.fileSystemWatcherEnabled).toString());

    this.http.post<{ enabled: boolean }>(
      `/api/mdfiles/ToggleFileSystemWatcher?enabled=${this.fileSystemWatcherEnabled}`,
      {}
    ).subscribe({
      next: (response) => {
        console.log('[Sidenav] FileSystemWatcher set to:', response.enabled);
      },
      error: (err) => {
        console.error('[Sidenav] Error toggling FileSystemWatcher:', err);
        // Revert state and localStorage on error
        this.fileSystemWatcherEnabled = !this.fileSystemWatcherEnabled;
        localStorage.setItem('mdexplorer_autoload_disabled', (!this.fileSystemWatcherEnabled).toString());
      }
    });
  }

}
