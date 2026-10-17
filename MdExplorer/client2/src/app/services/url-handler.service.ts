import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { filter, take } from 'rxjs/operators';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { MdFileService } from '../md-explorer/services/md-file.service';
import { MdNavigationService } from '../md-explorer/services/md-navigation.service';
import { MdFile } from '../md-explorer/models/md-file';
import { ModernCloneProjectComponent } from '../projects/dialogs/modern-clone-project/modern-clone-project.component';

/**
 * Service to handle mdexplorer:// URL commands received via SignalR
 */
@Injectable({
  providedIn: 'root'
})
export class UrlHandlerService {

  private initialized = false;

  /**
   * Flag to indicate that a URL handler command is pending.
   * When true, the landing page should NOT be opened automatically.
   */
  public skipLandingPage = false;

  constructor(
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private mdServerMessages: MdServerMessagesService,
    private projectsService: ProjectsService,
    private mdFileService: MdFileService,
    private navService: MdNavigationService
  ) {}

  /**
   * Initialize the URL handler listeners. Should be called once on app startup.
   */
  public initialize(): void {
    if (this.initialized) {
      return;
    }

    console.log('[UrlHandler] Initializing URL handler service');

    // Check if Electron has a pending URL command - set skipLandingPage early
    // to prevent race condition where loadAll returns before SignalR event
    if ((window as any).electronAPI?.hasPendingUrlCommand) {
      (window as any).electronAPI.hasPendingUrlCommand().then((hasPending: boolean) => {
        if (hasPending) {
          console.log('[UrlHandler] Pending URL command detected - skipLandingPage = true');
          this.skipLandingPage = true;
        }
      });
    }

    // Listen for open document commands
    this.mdServerMessages.addUrlHandlerOpenDocumentListener((data, _) => {
      this.handleOpenDocument(data);
    }, this);

    // Listen for configproject dialog commands (formerly clone)
    this.mdServerMessages.addUrlHandlerOpenConfigProjectDialogListener((data, _) => {
      this.handleOpenConfigProjectDialog(data);
    }, this);

    // Listen for error messages
    this.mdServerMessages.addUrlHandlerErrorListener((data, _) => {
      this.handleError(data);
    }, this);

    this.initialized = true;
    console.log('[UrlHandler] URL handler service initialized');
  }

  /**
   * Handle open document command.
   *
   * Opens the project if it is not the current one, waits for the document view and for the
   * tree's first level (`mdFiles` with paths under that project), then selects the file the
   * same way an in-document link does (`main-content.handleMdNavigate`): a minimal MdFile with
   * the relative path, through the navigation history and the side-nav selection. Nothing is
   * looked up in the tree's dataStore: the tree is lazy, a nested file is not there yet.
   *
   * Before (seen 2026-10-08): the handler waited for `folderIndexingComplete`, which never comes
   * when the project is already indexed (10 s timeout, then "proceeding anyway"), searched the
   * dataStore (a nested file was never found), and read `subscription` inside a synchronous
   * BehaviorSubject emission, before it was assigned.
   */
  private handleOpenDocument(data: any): void {
    console.log('[UrlHandler] handleOpenDocument:', JSON.stringify(data));

    // Data structure from backend:
    // {
    //   projectId: string,
    //   projectName: string,
    //   projectPath: string,
    //   filePath: string (relative),
    //   fullPath: string (absolute),
    //   section: string (optional anchor)
    // }

    // The landing page must not replace the document we are about to open.
    this.skipLandingPage = true;

    const wantedProject = this.normalizePath(data.projectPath);
    const current = this.projectsService.currentProjects$.getValue();
    const alreadyOpen = !!current && this.normalizePath(current.path) === wantedProject;
    if (!alreadyOpen) {
      console.log('[UrlHandler] Opening project:', data.projectPath);
      this.projectsService.setNewFolderProject(data.projectPath);
    }

    this.projectsService.currentProjects$.pipe(
      filter(project => !!project && this.normalizePath(project.path) === wantedProject),
      take(1)
    ).subscribe(() => {
      this.router.navigate(['/main/navigation/document']).then(() => {
        // The tree has loaded the first level of THIS project: the view is ready for a selection.
        this.mdFileService.mdFiles.pipe(
          filter(files => files?.length > 0 && files.some(f => this.normalizePath(f.fullPath).startsWith(wantedProject))),
          take(1)
        ).subscribe(() => this.selectFile(data.filePath, data.fullPath, data.section));
      });
    });

    this.snackBar.open(`Opening ${data.filePath}...`, 'OK', {
      duration: 3000
    });
  }

  /** Forward slashes, lower case, no trailing slash: the same path written in two ways compares equal. */
  private normalizePath(path: string | undefined): string {
    return (path || '').replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
  }

  /**
   * Select a file by its path in the project (and optionally scroll to a section), the way
   * an in-document link does: whoever follows the selected file (the view, the tree, the
   * MarkAgent context) follows it too.
   */
  private selectFile(filePath: string, fullPath: string, section?: string): void {
    const relativePath = (filePath || '').replace(/\\/g, '/').replace(/^\/+/, '');
    console.log('[UrlHandler] Selecting file:', relativePath, 'section:', section || '');
    const mdFile: MdFile = {
      name: relativePath.split('/').pop() || relativePath,
      path: relativePath,
      relativePath: relativePath,
      fullPath: fullPath || relativePath,
      fullDirectoryPath: '',
      level: 0,
      expandable: false,
      type: 'file',
      index: 0,
      isLoading: false,
      childrens: []
    };
    this.navService.setNewNavigation(mdFile);
    this.mdFileService.setSelectedMdFileFromSideNav(mdFile);

    // The document is being opened: from now on the landing page may be opened as usual.
    this.skipLandingPage = false;

    if (section) {
      setTimeout(() => this.scrollToSection(section), 1500); // Give time for the document to render
    }
  }

  /**
   * Scroll to a section anchor in the document. The document is rendered inside the viewer's
   * iframe: the anchor is looked for there, then in the page itself.
   */
  private scrollToSection(section: string): void {
    console.log('[UrlHandler] Scrolling to section:', section);
    const documents: Document[] = [];
    document.querySelectorAll('iframe').forEach(frame => {
      try { if (frame.contentDocument) { documents.push(frame.contentDocument); } } catch { /* cross-origin: not ours */ }
    });
    documents.push(document);
    const element = documents.map(d => d.getElementById(section)).find(e => !!e);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    } else {
      console.log('[UrlHandler] Section not found:', section);
    }
  }

  /**
   * Handle configproject dialog command (formerly clone)
   * Opens the clone/configproject dialog with pre-filled data from the shared URL
   */
  private handleOpenConfigProjectDialog(data: any): void {
    console.log('[UrlHandler] Opening configproject dialog:', data);

    // Data structure from backend:
    // {
    //   repo: string (repository URL),
    //   branch: string (optional branch name),
    //   user: string (optional username),
    //   basePath: string (optional parent folder for clone destination)
    // }

    // Open the clone dialog with pre-filled data
    const dialogRef = this.dialog.open(ModernCloneProjectComponent, {
      width: '600px',
      data: {
        prefilledUrl: data.repo,
        prefilledBranch: data.branch,
        prefilledUser: data.user,
        prefilledBasePath: data.basePath
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('[UrlHandler] ConfigProject dialog closed with result:', result);
      }
    });
  }

  /**
   * Handle error messages from backend
   */
  private handleError(data: any): void {
    console.error('[UrlHandler] Error:', data);

    this.snackBar.open(data.error || 'An error occurred', 'OK', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}
