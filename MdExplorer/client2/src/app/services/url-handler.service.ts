import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { MdFileService } from '../md-explorer/services/md-file.service';
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
    private mdFileService: MdFileService
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
   * Handle open document command
   */
  private handleOpenDocument(data: any): void {
    console.log('[UrlHandler] ========== handleOpenDocument called ==========');
    console.log('[UrlHandler] Data received:', JSON.stringify(data));

    // Set flag to skip landing page - we're opening a specific document
    this.skipLandingPage = true;
    console.log('[UrlHandler] skipLandingPage set to true');

    // Data structure from backend:
    // {
    //   projectId: string,
    //   projectName: string,
    //   projectPath: string,
    //   filePath: string (relative),
    //   fullPath: string (absolute),
    //   section: string (optional anchor)
    // }

    // Normalize paths for comparison (replace backslashes with forward slashes)
    const normalizedProjectPath = data.projectPath?.replace(/\\/g, '/').toLowerCase();

    // Set up listener for indexing complete BEFORE setting the project
    // This ensures we catch the event even if indexing is fast
    let indexingCompleteReceived = false;
    const indexingCompleteHandler = (indexData: any, _: any) => {
      console.log('[UrlHandler] folderIndexingComplete received:', indexData);
      indexingCompleteReceived = true;
    };
    this.mdServerMessages.addFolderIndexingCompleteListener(indexingCompleteHandler, this);

    // First, set the project as current
    console.log('[UrlHandler] Setting project:', data.projectPath);
    this.projectsService.setNewFolderProject(data.projectPath);

    // Wait for project to be set, then navigate to the document
    console.log('[UrlHandler] Subscribing to currentProjects$...');
    const subscription = this.projectsService.currentProjects$.subscribe(project => {
      console.log('[UrlHandler] currentProjects$ emitted:', project ? project.path : 'null');

      if (project) {
        const normalizedCurrentPath = project.path?.replace(/\\/g, '/').toLowerCase();
        console.log('[UrlHandler] Comparing paths:');
        console.log('[UrlHandler]   - Current project path (normalized):', normalizedCurrentPath);
        console.log('[UrlHandler]   - Expected project path (normalized):', normalizedProjectPath);
        console.log('[UrlHandler]   - Match:', normalizedCurrentPath === normalizedProjectPath);

        if (normalizedCurrentPath === normalizedProjectPath) {
          console.log('[UrlHandler] Project matched! Navigating to document view...');
          subscription.unsubscribe();
          console.log('[UrlHandler] Unsubscribed from currentProjects$');

          // Navigate to the document view
          this.router.navigate(['/main/navigation/document']).then(() => {
            console.log('[UrlHandler] Navigation complete, waiting for indexing to complete...');

            // Wait for indexing to complete before selecting the file
            this.waitForIndexingComplete(indexingCompleteReceived, () => {
              console.log('[UrlHandler] Indexing complete, selecting file...');
              this.selectFile(data.fullPath, data.filePath, data.section);
            });
          });
        }
      }
    });

    this.snackBar.open(`Opening ${data.filePath}...`, 'OK', {
      duration: 3000
    });
  }

  /**
   * Wait for indexing to complete, with timeout fallback
   */
  private waitForIndexingComplete(alreadyComplete: boolean, callback: () => void): void {
    if (alreadyComplete) {
      console.log('[UrlHandler] Indexing already complete');
      setTimeout(callback, 500); // Small delay to ensure UI is ready
      return;
    }

    console.log('[UrlHandler] Waiting for folderIndexingComplete event...');
    let completed = false;

    // Set up one-time listener for indexing complete
    const handler = (_data: any, _: any) => {
      if (!completed) {
        completed = true;
        console.log('[UrlHandler] folderIndexingComplete event received');
        setTimeout(callback, 500); // Small delay to ensure UI is ready
      }
    };
    this.mdServerMessages.addFolderIndexingCompleteListener(handler, this);

    // Timeout fallback after 10 seconds
    setTimeout(() => {
      if (!completed) {
        completed = true;
        console.log('[UrlHandler] Timeout waiting for indexing, proceeding anyway...');
        callback();
      }
    }, 10000);
  }

  /**
   * Select a file in the tree and optionally scroll to a section
   */
  private selectFile(fullPath: string, relativePath: string, section?: string): void {
    console.log('[UrlHandler] Selecting file:', fullPath, 'relativePath:', relativePath, 'section:', section);

    // Create a minimal MdFile object to search for in the dataStore
    const searchFile: MdFile = {
      fullPath: fullPath,
      path: fullPath,
      name: fullPath.split(/[/\\]/).pop() || '',
      relativePath: relativePath,
      level: 0,
      expandable: false,
      type: 'mdFile',
      childrens: [],
      index: 0,
      isLoading: false,
      fullDirectoryPath: fullPath.substring(0, Math.max(fullPath.lastIndexOf('/'), fullPath.lastIndexOf('\\')))
    };

    // Try to find the file in the dataStore to get the complete MdFile object
    const foundFile = this.mdFileService.getMdFileFromDataStore(searchFile);

    if (foundFile) {
      console.log('[UrlHandler] Found file in dataStore:', foundFile.fullPath);
      // Set the file as selected - this will trigger the document to load
      this.mdFileService.setSelectedMdFileFromSideNav(foundFile);
      this.mdFileService.setSelectedMdFileFromServer(foundFile);
    } else {
      console.log('[UrlHandler] File not found in dataStore, using search file with relativePath:', relativePath);
      // Fallback: use the minimal object with the relativePath from the URL
      this.mdFileService.setSelectedMdFileFromSideNav(searchFile);
      this.mdFileService.setSelectedMdFileFromServer(searchFile);
    }

    // Reset skipLandingPage flag after file selection
    this.skipLandingPage = false;
    console.log('[UrlHandler] skipLandingPage reset to false');

    // If there's a section anchor, scroll to it after the document loads
    if (section) {
      setTimeout(() => {
        this.scrollToSection(section);
      }, 1000); // Give time for the document to render
    }
  }

  /**
   * Scroll to a section anchor in the document
   */
  private scrollToSection(section: string): void {
    console.log('[UrlHandler] Scrolling to section:', section);

    // Try to find the element by id
    const element = document.getElementById(section);
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
