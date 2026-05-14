import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { RenameFileComponent } from '../refactoring/rename-file/rename-file.component';
import { MdFileService } from '../../services/md-file.service';
import { RulesComponent } from '../../../signalR/dialogs/rules/rules.component';
import { MdFile } from '../../models/md-file';
import { GITService } from '../../../git/services/gitservice.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { MatLegacyMenuTrigger as MatMenuTrigger } from '@angular/material/legacy-menu';
import { IBranch } from '../../../git/models/branch';
import { MatLegacyTabGroup as MatTabGroup } from '@angular/material/legacy-tabs';
import { ITag } from '../../../git/models/Tag';
import { ProjectsService } from '../../services/projects.service';
import { Router } from '@angular/router';
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { GitMessagesComponent } from '../../../git/components/git-messages/git-messages.component';
import { CommitMessageDialogComponent } from '../../../git/dialogs/commit-message-dialog/commit-message-dialog.component';
import { GitHistoryDialogComponent } from '../../../git/dialogs/git-history-dialog/git-history-dialog.component';
import { GitBranchDialogComponent } from '../../../git/dialogs/git-branch-dialog/git-branch-dialog.component';
import { GitSetupRemoteGenericDialogComponent } from '../../../git/dialogs/git-setup-remote-generic-dialog/git-setup-remote-generic-dialog.component';
import { GitAccountManagementDialogComponent } from '../../../git/dialogs/git-account-management-dialog/git-account-management-dialog.component';
import { BookmarksService } from '../../services/bookmarks.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { Bookmark } from '../../services/Types/Bookmark';
import { MdNavigationService } from '../../services/md-navigation.service';
import { Subscription, forkJoin } from 'rxjs';
import { FileNameAndAuthor } from '../../../git/models/DataToPull';
import { TocGenerationService } from '../../services/toc-generation.service';
import { TocProgressService } from '../../services/toc-progress.service';
import { GitChangedFile } from '../../../git/models/modern-git-models';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../commons/components/confirm-dialog/confirm-dialog.component';
import { TranslateService } from '@ngx-translate/core';
import _ from 'lodash';


@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements OnInit, OnDestroy {
  // Esponiamo console per il template
  public console = console;

  public currentBranch: string;
  @ViewChild('hoverMenu', { static: false }) hoverMenuTrigger: MatMenuTrigger;
  @ViewChild('branchMenuTrigger', { static: false }) matMenuTrigger: MatMenuTrigger;
  @ViewChild(MatTabGroup, { static: false }) tabGroup: MatTabGroup;

  TitleToShow: string;
  absolutePath: string;
  relativePath: string;
  connectionId: string;
  somethingIsChangedInTheBranch: boolean;
  somethingIsToPull: boolean;
  somethingIsToPush: boolean;
  howManyFilesAreToCommit: number;
  howManyCommitAreToPush: number;
  howManyFilesAreToPull: number;
  branches: IBranch[];
  taglist: ITag[];
  currentMdFile: MdFile
  public connectionIsActive: boolean = true;
  public isCheckingConnection: boolean = false;
  public filesAndAuthors: FileNameAndAuthor[];
  subscriptionserverSelectedMdFile: Subscription;
  public showMenu: boolean = false;
  public showCommitMenu: boolean = false;
  public changedFiles: GitChangedFile[] = [];
  public isLoadingChangedFiles: boolean = false;
  public hasRemoteConfigured: boolean = true; // Default true to hide menu initially
  public currentRemoteUrl: string = '';
  public isUsingNativeGit: boolean = false; // True when Git native credentials are working
  public needsRemoteManagement: boolean = false; // True when using app-stored credentials
  public isGitRepository: boolean = true; // False for non-Git projects (hides Git UI elements)
  public authenticationMissing: boolean = false; // True when no credentials are configured
  public authenticationFailed: boolean = false; // True when credentials exist but auth failed (VPN, token expired)
  public authenticationFailureReason: string = ''; // Detailed reason for auth failure

  //@Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    public dialog: MatDialog,
    private monitorMDService: MdServerMessagesService,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    public mdFileService: MdFileService,
    private gitservice: GITService,
    private appSettings: AppCurrentMetadataService,
    private projectService: ProjectsService,
    private router: Router,
    private waitingDialogService: WaitingDialogService,
    private bookmarksService: BookmarksService,
    private navService: MdNavigationService,
    private tocService: TocGenerationService,
    private tocProgressService: TocProgressService,
    private translate: TranslateService

  ) {
    this.TitleToShow = "MdExplorer";
    this.connectionIsActive = true;
  }

  ngOnInit(): void {
    // Get connectionId from SignalR service for export notifications
    this.connectionId = this.monitorMDService.connectionId;
    // If connectionId is not yet available, get it when ready
    if (!this.connectionId) {
      this.monitorMDService.getConnectionId((id) => {
        this.connectionId = id;
      }, this);
    }

    this.monitorMDService.addMdProcessedListener(this.markdownFileIsProcessed, this);
    this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this); //TODO: da spostare in SignalR
    this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this);//TODO: da spostare in SignalR
    this.monitorMDService.addYamlAutoGeneratedListener(this.showYamlAutoGenerated, this);
    // get current branch name and if the branch has something to commit
    this.gitservice.currentBranch$.subscribe(branch => {
      this.currentBranch = branch.name;
      this.somethingIsChangedInTheBranch = branch.somethingIsChangedInTheBranch;
      this.howManyFilesAreToCommit = branch.howManyFilesAreChanged;
      this.howManyCommitAreToPush = branch.howManyCommitAreToPush;
      this.connectionIsActive = true;
    });

    this.gitservice.commmitsToPull$.subscribe(_ => {
      this.somethingIsToPull = _.somethingIsToPull;
      this.somethingIsToPush = _.howManyCommitAreToPush > 0;
      this.howManyFilesAreToPull = _.howManyFilesAreToPull;
      this.howManyCommitAreToPush = _.howManyCommitAreToPush;
      this.connectionIsActive = _.connectionIsActive;
      this.isCheckingConnection = false;
      this.filesAndAuthors = _.whatFilesWillBeChanged;
    });
    
    // Set initial project path if available
    const currentProject = this.projectService.currentProjects$.value;
    if (currentProject && currentProject.path) {
      this.gitservice.setProjectPath(currentProject.path);
    }
    
    // Subscribe to project changes to update Git service
    this.projectService.currentProjects$.subscribe((project: any) => {
      if (project && project.path) {
        // Reset Git state immediately when switching projects
        this.resetGitState();
        // Then set new project path and trigger poll
        this.gitservice.setProjectPath(project.path);

        // Check if manual credentials are needed (auto-detection failed)
        // Only show dialog for non-OAuth providers - OAuth providers (GitHub, GitLab, Azure, Bitbucket)
        // are handled by GCM which opens browser for authentication
        const oauthProviders = ['github', 'gitlab', 'azure', 'bitbucket'];
        const isOAuthProvider = oauthProviders.includes(project.detectedProvider?.toLowerCase() || '');

        if (project.needsManualCredentials && project.remoteUrl && !isOAuthProvider) {
          console.log('[Toolbar] Manual credentials needed for:', project.remoteUrl, 'provider:', project.detectedProvider);
          // Open the credential setup dialog
          this.openCredentialSetupDialog(project.path, project.remoteUrl);
        } else if (project.needsManualCredentials && isOAuthProvider) {
          console.log('[Toolbar] OAuth provider detected, GCM will handle authentication via browser:', project.detectedProvider);
        }
      }
    });
    
    this.checkConnection();

    // manage resize fullscreen
    document.onfullscreenchange = (event) => {

      if (document.fullscreenElement) {
        this.screenType = "close_fullscreen";
      } else {
        this.screenType = "fullscreen";
      }
    };

    this.gitservice.getBranchList().subscribe(branches => {
      this.branches = branches;
    });



    // something is selected from treeview/sidenav
    this.mdFileService.selectedMdFileFromSideNav.subscribe(_ => {
      if (_ != null) {
        this.currentMdFile = _;
        this.mdFileService.navigationArray = [];
        this.absolutePath = _.fullPath;
        this.relativePath = _.relativePath;
      }
    });
    // something has changed on filesystem
    this.subscriptionserverSelectedMdFile = this.mdFileService.serverSelectedMdFile.subscribe(val => {

      var current = val[0];
      if (current != undefined) {
        let index = this.mdFileService.navigationArray.length;
        if (index > 0) {
          //if (current.fullPath == this.mdFileService.navigationArray[index - 1].fullPath) {
          if (current == this.mdFileService.navigationArray[index - 1]) {
            //return;
          }
        }

        this.navService.setNewNavigation(current);
        this.absolutePath = current.fullPath;
        this.relativePath = current.relativePath;
        this.currentMdFile = current;
      }

    });
  }


  ngOnDestroy(): void {
    console.log("ngOnDestroy toolbar");
    this.subscriptionserverSelectedMdFile.unsubscribe();
  }



  toggleSidenav() {
    let test = !this.appSettings.showSidenav.value;
    this.appSettings.showSidenav.next(test);
  }

  openRules(data: any): void {
    const dialogRef = this.dialog.open(RulesComponent, {
      width: 'auto',
      maxWidth: '90vw',
      disableClose: false,
      panelClass: 'subtle-dialog-panel',
      data: data
    });
    dialogRef.afterClosed().subscribe(_ => {
      if (_ && _.refactoringSourceActionId != undefined) {
        // User chose to apply suggestion
        this._snackBar.open(this.translate.instant('TOOLBAR.FILE_RENAMED'), '', {
          duration: 2500,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar']
        });
        
        this.dialog.open(RenameFileComponent, {
          width: '600px',
          data: _
        });
      } else if (_ === null) {
        // User chose to keep current filename
        this._snackBar.open(this.translate.instant('TOOLBAR.FILENAME_UNCHANGED'), '', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['subtle-snackbar']
        });
      }
    });
  }

  /**
   * Reset all Git-related state variables to initial/empty values.
   * Called when switching projects to ensure clean state.
   */
  private resetGitState(): void {
    // Reset branch and commit counters
    this.currentBranch = "";
    this.somethingIsChangedInTheBranch = false;
    this.somethingIsToPull = false;
    this.somethingIsToPush = false;
    this.howManyFilesAreToCommit = 0;
    this.howManyCommitAreToPush = 0;
    this.howManyFilesAreToPull = 0;

    // Reset arrays
    this.filesAndAuthors = [];
    this.branches = [];
    this.taglist = [];

    // Reset remote configuration flags
    this.hasRemoteConfigured = false;  // Will show "checking..." until poll completes
    this.currentRemoteUrl = '';
    this.isUsingNativeGit = false;
    this.needsRemoteManagement = false;
    this.authenticationMissing = false;
    this.authenticationFailed = false;
    this.authenticationFailureReason = '';

    // Reset connection state
    this.connectionIsActive = false;
    this.isCheckingConnection = true;

    // Assume Git repository by default (will be updated after check)
    this.isGitRepository = true;
  }

  checkConnection(): void {
    this.isCheckingConnection = true;

    const projectPath = this.getProjectPath();
    if (!projectPath) {
      this.isCheckingConnection = false;
      return;
    }

    // Update the Git service with current project path
    this.gitservice.setProjectPath(projectPath);

    // Check remote status first
    this.gitservice.checkRemoteStatus(projectPath).subscribe(
      remoteStatus => {
        console.log('Remote status:', remoteStatus);

        // Track authentication status details
        this.authenticationMissing = remoteStatus.authenticationMissing || false;
        this.authenticationFailed = remoteStatus.authenticationFailed || false;
        this.authenticationFailureReason = remoteStatus.authenticationFailureReason || '';

        // Remote is considered configured if it exists AND authentication works
        // IMPORTANT: Hide menu when native Git authentication is working
        // Native Git methods: 'Default', 'GitCredentialHelper', 'SystemCredentialStore'
        // Three states:
        // 1. No remote OR auth failed → Show "Setup Remote" (hasRemoteConfigured=false)
        // 2. Native Git working → Hide all menus (isUsingNativeGit=true, hasRemoteConfigured=true)
        // 3. App credentials working → Show "Manage Remote" (needsRemoteManagement=true)
        const nativeGitMethods = ['Default', 'GitCredentialHelper', 'SystemCredentialStore'];
        this.isUsingNativeGit = remoteStatus.authenticationMethod &&
                                nativeGitMethods.includes(remoteStatus.authenticationMethod);
        const needsRemoteSetup = !remoteStatus.hasRemote || !remoteStatus.canAuthenticate;
        this.needsRemoteManagement = remoteStatus.hasRemote && remoteStatus.canAuthenticate && !this.isUsingNativeGit;

        // Set hasRemoteConfigured to control "Setup Remote" visibility
        // false = show "Setup Remote", true = hide "Setup Remote"
        this.hasRemoteConfigured = remoteStatus.hasRemote && remoteStatus.canAuthenticate;
        this.currentRemoteUrl = remoteStatus.remoteUrl || '';

        // Log authentication status for debugging
        if (remoteStatus.hasRemote && this.authenticationMissing) {
          console.warn('⚠️ No credentials configured for remote. Show "Configure Git Account" button.');
          this.isCheckingConnection = false;
          this.connectionIsActive = false;
        } else if (remoteStatus.hasRemote && this.authenticationFailed) {
          console.warn('❌ Authentication failed (VPN/network issue). Show connection warning.');
          this.isCheckingConnection = false;
          this.connectionIsActive = false;
        } else if (remoteStatus.hasRemote && remoteStatus.canAuthenticate) {
          console.log('✅ Remote configured and authentication successful using:', remoteStatus.authenticationMethod);

          // Reset auth failure flags on successful connection
          this.authenticationMissing = false;
          this.authenticationFailed = false;
          this.authenticationFailureReason = '';
          this.connectionIsActive = true;

          if (this.isUsingNativeGit) {
            console.log('🎯 Native Git credentials working - hiding remote setup menu');
          } else {
            console.log('🔧 App-stored credentials active - showing "Manage Remote" menu');
          }
          console.log('🔑 Credentials are now cached - subsequent calls will not require authentication');

          // Authentication successful - credentials are now cached
          // Now fetch Git data (will use cached credentials, no auth request)
          if (remoteStatus.isGitRepository) {
            this.isGitRepository = true;
            forkJoin([
              this.gitservice.modernGetBranchStatus(projectPath),
              this.gitservice.modernGetDataToPull(projectPath)
            ]).subscribe(
              ([branch, pullData]) => {
                console.log('📊 Git data retrieved using cached credentials');
                // Update observables with the received data
                this.gitservice.currentBranch$.next(branch);
                this.gitservice.commmitsToPull$.next(pullData);
                this.isCheckingConnection = false;
              },
              error => {
                console.error('Error fetching Git data:', error);
                this.isCheckingConnection = false;
                this.connectionIsActive = false;
              }
            );
          } else {
            this.isCheckingConnection = false;
            this.connectionIsActive = false;
          }
        } else if (remoteStatus.isGitRepository && !remoteStatus.hasRemote) {
          // Git repository exists but no remote configured
          console.log('📁 Git repository without remote - showing "Setup Remote" menu');
          this.isGitRepository = true;
          this.isCheckingConnection = false;
          this.connectionIsActive = true;
        } else {
          // Not a Git repository - reset all Git state
          console.log('📁 Not a Git repository - resetting Git state');
          this.isGitRepository = false;
          this.isCheckingConnection = false;
          this.connectionIsActive = false;
          this.hasRemoteConfigured = true; // Hide setup menu if not Git repo
        }
      },
      error => {
        console.error('Error checking remote status:', error);
        // On error, treat as non-Git repository
        this.isGitRepository = false;
        this.isCheckingConnection = false;
        this.connectionIsActive = false;
        // On error, assume remote is configured to hide the menu
        this.hasRemoteConfigured = true;
      }
    );
  }

  private showRule1IsBroken(data: any, objectThis: ToolbarComponent) {
    objectThis.openRules(data);
  }

  private sendExportRequest(objectThis: ToolbarComponent) {
    const url = '../api/mdexport/' + objectThis.relativePath + '?ConnectionId=' + objectThis.connectionId;
    return objectThis.http.get(url)
      .subscribe(data => { console.log(data) });
  }

  private showPdfIsready(data: any, objectThis: ToolbarComponent) {
    let snackRef = objectThis._snackBar.open("seconds: " + data.executionTimeInSeconds, objectThis.translate.instant('TOOLBAR.OPEN_FOLDER'), { duration: 5000, verticalPosition: 'top' });
    snackRef.onAction().subscribe(() => {
      const url = '../api/AppSettings/OpenChromePdf?path=' + data.path;
      return objectThis.http.get(url)
        .subscribe(data => { console.log(data) });
    });
  }

  private showYamlAutoGenerated(data: any, objectThis: ToolbarComponent) {
    objectThis._snackBar.open(data.message, 'OK', {
      duration: 4000,
      verticalPosition: 'top',
      panelClass: ['warning-snackbar']
    });
  }

  private markdownFileIsProcessed(data: MdFile, objectThis: ToolbarComponent) {
    objectThis.currentMdFile = data;
    objectThis.mdFileService.navigationArray.push(data);
    objectThis.mdFileService.setSelectedMdFileFromServer(data);
  }

  OpenEditor() {
    const url = '../api/AppSettings/OpenFile?path=' + this.absolutePath;
    this.http.get<any>(url).subscribe(data => {
      // Docker mode: backend can't spawn the host's editor process, so it
      // returns a "vscode://file/..." (or jetbrains://) URL. Hand it to the
      // browser, which forwards it to the OS, which launches the editor on
      // the host. On native Windows/Linux this branch is never taken.
      if (data && data.openUrl) {
        window.location.href = data.openUrl;
        return;
      }
      console.log(data);
    });
  }

  Export() {
    if (!this.relativePath) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_DOC_FIRST'), 'OK', { duration: 3000, verticalPosition: 'top' });
      return;
    }
    this._snackBar.open(this.translate.instant('TOOLBAR.EXPORT_QUEUED'), null, { duration: 2000, verticalPosition: 'top' });
    this.sendExportRequest(this);
  }


  public screenType = "fullscreen";

  FullScreenToggle(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
    } else {
      document.exitFullscreen();
    }

  }

  pull(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    
    let info = new WaitingDialogInfo();
    info.message = "Please wait... Pulling branch"
    this.waitingDialogService.showMessageBox(info);
    
    console.log('[DEBUG] Pull operation started with projectPath:', projectPath);
    
    // Use modern Git service with native SSH authentication
    this.gitservice.modernPull(projectPath).subscribe(
      responseFromPull => {
        console.log('[DEBUG] Pull response received:', responseFromPull);
        this.handleGitResponse(responseFromPull, 'pull');
        this.waitingDialogService.closeMessageBox();
      },
      error => {
        console.error('[DEBUG] Pull error:', error);
        console.error('[DEBUG] Error status:', error.status);
        console.error('[DEBUG] Error message:', error.message);
        console.error('[DEBUG] Error response body:', error.error);
        
        this.waitingDialogService.closeMessageBox();
        
        // Show error to user
        const errorMessage = error.error?.errorMessage || error.message || '';
        this._snackBar.open(this.translate.instant('TOOLBAR.PULL_FAILED', { error: errorMessage }), 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    );
  }

  /**
   * Handles Git operation responses for both legacy and modern services
   */
  public handleGitResponse(responseFromPull: any, operation: string): void {
    // Handle connection issues
    if (responseFromPull.isConnectionMissing) {
      const dialogRef = this.dialog.open(GitMessagesComponent, {
        width: '300px',
        data: {
          message: 'Missing connection',
          description: 'Please verify your vpn or network settings'
        }
      });
      return;
    }

    // Note: Modern Git uses SSH authentication, no manual credentials needed

    // Handle conflicts
    if (responseFromPull.thereAreConflicts) {
      const dialogRef = this.dialog.open(GitMessagesComponent, {
        width: '300px',
        data: {
          message: 'Conflicts appear',
          description: responseFromPull.errorMessage
        }
      });
      return;
    }

    // Always refresh connection status after successful operations
    // Tree refresh is handled via SignalR gitPullRefreshed event in md-file.service + md-tree
    this.checkConnection();
  }

  /**
   * Validates and returns the current project path.
   * Shows error if no project is selected.
   */
  private getProjectPath(): string | null {
    const currentProject = this.projectService.currentProjects$.value;
    
    if (!currentProject || !currentProject.path) {
      console.error('No current project path available');
      this._snackBar.open(this.translate.instant('TOOLBAR.NO_PROJECT_SELECTED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top',
        panelClass: ['error-snackbar']
      });
      return null;
    }
    
    return currentProject.path;
  }

  commit(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    
    // Ask user for commit message using Material Dialog
    const dialogRef = this.dialog.open(CommitMessageDialogComponent, {
      width: '500px',
      data: { 
        defaultMessage: 'Update from MdExplorer',
        projectPath: projectPath
      }
    });

    dialogRef.afterClosed().subscribe(commitMessage => {
      if (commitMessage === null || commitMessage === undefined) {
        // User cancelled
        return;
      }
      
      let info = new WaitingDialogInfo();
      info.message = "Please wait... committing changes";
      this.waitingDialogService.showMessageBox(info);
      
      console.log('[DEBUG] Commit operation started with projectPath:', projectPath, 'and message:', commitMessage);
      
      // Use modern Git service with native SSH authentication (commit only)
      this.gitservice.modernCommit(projectPath, commitMessage).subscribe(
        response => {
          console.log('[DEBUG] Commit response received:', response);
          this.handleGitResponse(response, 'commit');
          this.waitingDialogService.closeMessageBox();
          this.matMenuTrigger?.closeMenu();
        },
        error => {
          console.error('[DEBUG] Commit error:', error);
          console.error('[DEBUG] Error status:', error.status);
          console.error('[DEBUG] Error message:', error.message);
          console.error('[DEBUG] Error response body:', error.error);
          
          this.waitingDialogService.closeMessageBox();
          
          // Show error to user
          const errorMessage = error.error?.errorMessage || error.message || '';
          this._snackBar.open(this.translate.instant('TOOLBAR.COMMIT_FAILED', { error: errorMessage }), 'OK', {
            duration: 5000,
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          });
        }
      );
    });
  }

  push(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    
    let info = new WaitingDialogInfo();
    info.message = "Please wait... pushing changes";
    this.waitingDialogService.showMessageBox(info);
    
    console.log('[DEBUG] Push operation started with projectPath:', projectPath);
    
    // Use modern Git service with native SSH authentication
    this.gitservice.modernPush(projectPath).subscribe(
      response => {
        console.log('[DEBUG] Push response received:', response);
        this.handleGitResponse(response, 'push');
        this.waitingDialogService.closeMessageBox();
        this.matMenuTrigger?.closeMenu();
      },
      error => {
        console.error('[DEBUG] Push error:', error);
        console.error('[DEBUG] Error status:', error.status);
        console.error('[DEBUG] Error message:', error.message);
        console.error('[DEBUG] Error response body:', error.error);
        
        this.waitingDialogService.closeMessageBox();
        
        // Show error to user
        const errorMessage = error.error?.errorMessage || error.message || '';
        this._snackBar.open(this.translate.instant('TOOLBAR.PUSH_FAILED', { error: errorMessage }), 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    );
  }

  commitAndPush(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    
    // Ask user for commit message using Material Dialog
    const dialogRef = this.dialog.open(CommitMessageDialogComponent, {
      width: '500px',
      data: { defaultMessage: 'Update from MdExplorer' }
    });

    dialogRef.afterClosed().subscribe(commitMessage => {
      if (commitMessage === null || commitMessage === undefined) {
        // User cancelled
        return;
      }
      
      let info = new WaitingDialogInfo();
      info.message = "Please wait... committing and pushing changes";
      this.waitingDialogService.showMessageBox(info);
      
      console.log('[DEBUG] Commit and push operation started with projectPath:', projectPath, 'and message:', commitMessage);
      
      // Use modern Git service with native SSH authentication (commit and push)
      this.gitservice.modernCommitAndPush(projectPath, commitMessage).subscribe(
        response => {
          console.log('[DEBUG] Commit and push response received:', response);
          this.handleGitResponse(response, 'commit and push');
          this.waitingDialogService.closeMessageBox();
          this.matMenuTrigger?.closeMenu();
        },
        error => {
          console.error('[DEBUG] Commit and push error:', error);
          console.error('[DEBUG] Error status:', error.status);
          console.error('[DEBUG] Error message:', error.message);
          console.error('[DEBUG] Error response body:', error.error);
          
          this.waitingDialogService.closeMessageBox();
          
          // Show error to user
          const errorMessage = error.error?.errorMessage || error.message || '';
          this._snackBar.open(this.translate.instant('TOOLBAR.COMMIT_PUSH_FAILED', { error: errorMessage }), 'OK', {
            duration: 5000,
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          });
        }
      );
    });
  }

  openBranch(branch: IBranch): void {
    this.gitservice.checkoutSelectedBranch(branch).subscribe(_ => {
      this.currentBranch = _.name;
      var mdFile = new MdFile("Welcome to MDExplorer", '/../welcome.html', 0, false);
      mdFile.relativePath = '/../../welcome.html';
      this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
      this.projectService.setNewFolderProject(_.fullPath);

    });
    this.matMenuTrigger?.closeMenu();
  }

  openHistory(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const currentProject = this.projectService.currentProjects$.value;
    const projectName = currentProject?.name || 'Current Project';

    const dialogRef = this.dialog.open(GitHistoryDialogComponent, {
      width: '900px',
      height: '700px',
      data: {
        projectPath: projectPath,
        projectName: projectName
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      // Handle any result if needed
      console.log('History dialog closed');
    });

    this.matMenuTrigger?.closeMenu();
  }

  openBranchDialog(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const currentProject = this.projectService.currentProjects$.value;
    const projectName = currentProject?.name || 'Current Project';

    const dialogRef = this.dialog.open(GitBranchDialogComponent, {
      width: '600px',
      data: {
        projectPath: projectPath,
        projectName: projectName
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      // Handle any result if needed
      console.log('Branch dialog closed');
    });

    this.matMenuTrigger?.closeMenu();
  }

  openSetupRemote(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const currentProject = this.projectService.currentProjects$.value;
    const projectName = projectPath.split(/[/\\]/).pop() || 'repository';

    const dialogRef = this.dialog.open(GitSetupRemoteGenericDialogComponent, {
      width: '650px',
      data: {
        projectPath: projectPath,
        projectName: projectName,
        prefilledRemoteUrl: this.currentRemoteUrl  // Pre-fill URL if available (from remote-status)
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // Remote was successfully configured, update status
        this.checkConnection();
      }
      console.log('Setup remote dialog closed');
    });

    this.matMenuTrigger?.closeMenu();
  }

  /**
   * Opens the credential setup dialog when auto-detection fails.
   * Called automatically when a project is opened and needs manual credentials.
   */
  openCredentialSetupDialog(projectPath: string, remoteUrl: string): void {
    const projectName = projectPath.split(/[/\\]/).pop() || 'repository';

    // Small delay to let the UI settle after project open
    setTimeout(() => {
      const dialogRef = this.dialog.open(GitSetupRemoteGenericDialogComponent, {
        width: '650px',
        data: {
          projectPath: projectPath,
          projectName: projectName,
          prefilledRemoteUrl: remoteUrl,
          isCredentialRecovery: true  // Flag to indicate this is a credential recovery flow
        }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result === true) {
          // Credentials were successfully configured
          console.log('[Toolbar] Credentials configured successfully');
          this.checkConnection();
          this._snackBar.open(this.translate.instant('TOOLBAR.CREDENTIALS_OK'), 'OK', {
            duration: 3000,
            verticalPosition: 'top'
          });
        } else {
          // User cancelled - show warning
          this._snackBar.open(this.translate.instant('TOOLBAR.CREDENTIALS_NOT_CONFIGURED'), 'OK', {
            duration: 5000,
            verticalPosition: 'top',
            panelClass: ['warning-snackbar']
          });
        }
      });
    }, 500);
  }

  openManageRemote(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const currentProject = this.projectService.currentProjects$.value;
    const projectName = currentProject?.name || 'Current Project';

    const dialogRef = this.dialog.open(GitAccountManagementDialogComponent, {
      width: '600px',
      data: {
        repositoryPath: projectPath,
        repositoryName: projectName
      }
    });

    dialogRef.afterClosed().subscribe(accountConfigured => {
      if (accountConfigured === true) {
        // Account was successfully configured or updated
        this.checkConnection();
      }
      console.log('Git account management dialog closed');
    });

    this.matMenuTrigger?.closeMenu();
  }

  openGitInitWizard(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) {
      this._snackBar.open(this.translate.instant('TOOLBAR.NO_PROJECT_PATH'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    // Import the component dynamically
    import('../../../git/dialogs/git-init-wizard/git-init-wizard-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.GitInitWizardDialogComponent, {
        width: '600px',
        data: {
          repositoryPath: projectPath
        }
      });

      dialogRef.afterClosed().subscribe(initialized => {
        if (initialized === true) {
          console.log('[Toolbar] Git repository initialized, refreshing status');
          // Refresh Git status after initialization
          this.checkConnection();
        }
      });
    });
  }

  bookmarkToggle(): void {
    if (!this.currentMdFile) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_FILE_FIRST'), 'OK', {
        duration: 2000,
        verticalPosition: 'top'
      });
      return;
    }
    
    const currentProject = this.projectService.currentProjects$.value;
    if (!currentProject || !currentProject.id) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_PROJECT_FIRST'), 'OK', {
        duration: 2000,
        verticalPosition: 'top'
      });
      return;
    }
    
    let bookmark: Bookmark = new Bookmark(this.currentMdFile);
    bookmark.projectId = currentProject.id;
    this.bookmarksService.toggleBookmark(bookmark);
  }

  openReactEditor(): void {
    // Navigate to the route defined in MdExplorerModule
    // Assumes MdExplorerModule is loaded under '/main' and 'navigation' is a parent route segment
    this.router.navigate(['/main/navigation/react-editor']);
  }

  openAiChat(): void {
    // Navigate to AI chat within the application
    this.router.navigate(['/main/navigation/ai-chat']);
  }

  /**
   * Load the list of changed files for the commit panel hover
   */
  loadChangedFiles(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    this.isLoadingChangedFiles = true;
    this.gitservice.getChangedFiles(projectPath).subscribe(
      response => {
        this.changedFiles = response.files || [];
        this.isLoadingChangedFiles = false;
      },
      error => {
        console.error('Error loading changed files:', error);
        this.changedFiles = [];
        this.isLoadingChangedFiles = false;
      }
    );
  }

  /**
   * Discard changes to a file (restore from HEAD) or delete new files from disk
   */
  discardFile(file: GitChangedFile): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    if (file.isNew) {
      // For new files, show dialog to confirm DELETE from disk
      const dialogData: ConfirmDialogData = {
        title: 'Elimina file',
        message: `Vuoi eliminare definitivamente il file "${file.fileName}"? Questa azione non può essere annullata.`,
        confirmText: 'Elimina',
        cancelText: 'Annulla'
      };

      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '400px',
        data: dialogData
      });

      dialogRef.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          this.gitservice.deleteFile(projectPath, file.relativePath).subscribe(
            response => {
              if (response.success) {
                this.changedFiles = this.changedFiles.filter(f => f.relativePath !== file.relativePath);
                this.checkConnection();
                this._snackBar.open(this.translate.instant('TOOLBAR.FILE_DELETED', { name: file.fileName }), 'OK', {
                  duration: 3000,
                  horizontalPosition: 'right',
                  verticalPosition: 'bottom'
                });
              } else {
                this._snackBar.open(`Errore: ${response.errorMessage}`, 'OK', {
                  duration: 5000,
                  horizontalPosition: 'right',
                  verticalPosition: 'bottom',
                  panelClass: ['error-snackbar']
                });
              }
            },
            error => {
              console.error('Error deleting file:', error);
              this._snackBar.open(`Errore: ${error.message}`, 'OK', {
                duration: 5000,
                horizontalPosition: 'right',
                verticalPosition: 'bottom',
                panelClass: ['error-snackbar']
              });
            }
          );
        }
      });
    } else {
      // For modified files, show dialog to confirm discard changes
      const dialogData: ConfirmDialogData = {
        title: 'Scarta modifiche',
        message: `Vuoi scartare le modifiche a "${file.fileName}"? Questa azione non può essere annullata.`,
        confirmText: 'Scarta',
        cancelText: 'Annulla'
      };

      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '400px',
        data: dialogData
      });

      dialogRef.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          this.gitservice.discardFile(projectPath, file.relativePath, false).subscribe(
            response => {
              if (response.success) {
                this.changedFiles = this.changedFiles.filter(f => f.relativePath !== file.relativePath);
                this.checkConnection();
                this._snackBar.open(this.translate.instant('TOOLBAR.FILE_RESTORED', { name: file.fileName }), 'OK', {
                  duration: 3000,
                  horizontalPosition: 'right',
                  verticalPosition: 'bottom'
                });
              } else {
                this._snackBar.open(`Errore: ${response.errorMessage}`, 'OK', {
                  duration: 5000,
                  horizontalPosition: 'right',
                  verticalPosition: 'bottom',
                  panelClass: ['error-snackbar']
                });
              }
            },
            error => {
              console.error('Error discarding file:', error);
              this._snackBar.open(`Errore: ${error.message}`, 'OK', {
                duration: 5000,
                horizontalPosition: 'right',
                verticalPosition: 'bottom',
                panelClass: ['error-snackbar']
              });
            }
          );
        }
      });
    }
  }

  isTocDirectoryFile(): boolean {
    return this.currentMdFile?.name?.endsWith('.md.directory') || false;
  }

  refreshTocDirectory(): void {
    if (!this.currentMdFile || !this.isTocDirectoryFile()) {
      return;
    }

    // Get the relative path of the TOC file
    let tocPath = this.currentMdFile.relativePath || '';
    
    // Remove leading backslash if present
    if (tocPath.startsWith('\\')) {
      tocPath = tocPath.substring(1);
    }

    // Show progress dialog
    const directoryPath = tocPath.substring(0, tocPath.lastIndexOf('/')) || tocPath.substring(0, tocPath.lastIndexOf('\\'));
    this.tocProgressService.showProgress(directoryPath);

    this.tocService.refreshToc(tocPath).subscribe({
      next: (result) => {
        // Progress dialog will be closed by SignalR event
        if (result.success) {
          this._snackBar.open(this.translate.instant('TOOLBAR.TOC_UPDATED'), 'OK', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'bottom' 
          });
          // Reload the current file to show updated content
          this.mdFileService.setSelectedMdFileFromSideNav(this.currentMdFile);
        } else {
          this.tocProgressService.hideProgress();
          this._snackBar.open(this.translate.instant('TOOLBAR.TOC_UPDATE_FAILED'), 'OK', {
            duration: 5000,
            horizontalPosition: 'right',
            verticalPosition: 'bottom'
          });
        }
      },
      error: (err) => {
        console.error('Error refreshing TOC:', err);
        this.tocProgressService.hideProgress();
        this._snackBar.open(this.translate.instant('TOOLBAR.TOC_UPDATE_ERROR'), 'OK', {
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom'
        });
      }
    });
  }
}
