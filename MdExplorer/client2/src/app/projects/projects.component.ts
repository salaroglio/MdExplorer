import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Clipboard } from '@angular/cdk/clipboard';
import { HttpClient } from '@angular/common/http';
import { MdProject } from '../md-explorer/models/md-project';
import { MdFileService } from '../md-explorer/services/md-file.service';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { GITService } from '../git/services/gitservice.service';
import { P2PService } from '../services/p2p.service';
import { NewProjectComponent } from './new-project/new-project.component';
import { ShowFileSystemComponent } from '../commons/components/show-file-system/show-file-system.component';
import { ModernCloneProjectComponent } from './dialogs/modern-clone-project/modern-clone-project.component';
import { ProjectCreateConfigDialogComponent } from './dialogs/project-create-config/project-create-config-dialog.component';
import { ProjectEditDialogComponent, ProjectEditDialogResult } from './dialogs/project-edit/project-edit-dialog.component';
import { ProjectSettingsComponent } from './project-settings/project-settings.component';
import { P2PManagerComponent } from './dialogs/p2p-manager/p2p-manager.component';
import { NgDialogAnimationService } from '../shared/NgDialogAnimationService';
import { UnifiedSettingsDialogComponent } from '../components/unified-settings-dialog/unified-settings-dialog.component';
import { ShowFileMetadata } from '../commons/components/show-file-system/show-file-metadata';
import { versionInfo } from '../../environments/version'; // Importa la versione
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit, OnDestroy {

  public appVersion = versionInfo.version; // Rendi la versione disponibile nel template
  public buildTime = versionInfo.buildTime; // Rendi il timestamp di build disponibile nel template
  public recentProjects: Observable<MdProject[]>;
  public searchQuery: string = '';
  public lastOpenedProjectId: string = null;
  public isP2PAvailable: boolean = false;
  // Current git user email (lowercase) — used to hide "me" from the gem strip
  public currentUserEmail: string | null = null;

  // Flag to prevent multiple clicks when opening a project
  private isOpeningProject = false;

  // Cache for remote URL status per project path
  private remoteUrlCache: Map<string, { hasRemote: boolean; remoteUrl?: string; loading?: boolean }> = new Map();

  constructor(private projectService: ProjectsService,
    public dialog: MatDialog,
    private router: Router,
    private signalRService: MdServerMessagesService,
    private dialogAn: NgDialogAnimationService,
    private gitService: GITService,
    private clipboard: Clipboard,
    private snackBar: MatSnackBar,
    private http: HttpClient,
    private p2pService: P2PService,
    private translate: TranslateService
  ) { }

    ngOnDestroy(): void {
      console.log('ProjectsComponent destroyed!');
    }

  public dataSource: Observable<MdProject[]>
  public dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }]

  ngOnInit(): void {
    // Check P2P availability
    this.p2pService.isAvailable$.subscribe(available => {
      this.isP2PAvailable = available;
    });
    this.p2pService.checkAvailability();

    // Fetch the current git user's email so the team gems can hide "me".
    // Fallback: no path → backend returns the global git config.
    this.projectService.getCurrentGitUser(null).subscribe({
      next: user => {
        this.currentUserEmail = user?.email ? user.email.toLowerCase() : null;
      },
      error: () => {
        this.currentUserEmail = null;
      }
    });

    // Load recent projects and sort by lastUpdate descending (most recent first)
    this.projectService.fetchProjects();
    this.recentProjects = this.projectService.mdProjects.pipe(
      map(projects => {
        if (!projects || projects.length === 0) return [];

        const sorted = projects.sort((a, b) => {
          const dateA = a.lastUpdate ? new Date(a.lastUpdate).getTime() : 0;
          const dateB = b.lastUpdate ? new Date(b.lastUpdate).getTime() : 0;
          return dateB - dateA; // Descending order (most recent first)
        });

        // Identify the last opened project (first in sorted list)
        if (sorted.length > 0 && !this.lastOpenedProjectId) {
          this.lastOpenedProjectId = sorted[0].id;
        }

        // Apply search filter
        if (this.searchQuery && this.searchQuery.trim() !== '') {
          const query = this.searchQuery.toLowerCase();
          return sorted.filter(p =>
            p.name.toLowerCase().includes(query) ||
            p.path.toLowerCase().includes(query)
          );
        }

        return sorted;
      })
    );

    this.projectService.currentProjects$.subscribe(_ => {
      if (_ != null && _!= undefined) {
        this.router.navigate(['/main/navigation/document']);
      }
    });
  }

  onSearchChange(): void {
    // Trigger the observable to re-filter
    this.projectService.fetchProjects();
  }

  isLastOpened(project: MdProject): boolean {
    return project.id === this.lastOpenedProjectId;
  }

  getIconUrl(project: MdProject): string {
    return this.projectService.getProjectIconUrl(project.id, project.iconUpdatedAt);
  }

  openProject(path: string): void {
    // Prevent multiple clicks while project is opening
    if (this.isOpeningProject) {
      console.log('[Projects] Ignoring click - project opening already in progress');
      return;
    }

    this.isOpeningProject = true;
    console.log('[Projects] Opening project:', path);

    this.projectService.setNewFolderProject(path);

    // Reset flag after a timeout (in case navigation doesn't happen)
    setTimeout(() => {
      this.isOpeningProject = false;
    }, 10000); // 10 second safety timeout
  }

  deleteProject(project: MdProject): void {
    this.projectService.deleteProject(project, () => {
      this.projectService.fetchProjects();
    }, this);
  }

  openProjectSettings(project: MdProject): void {
    const dialogRef = this.dialog.open(ProjectSettingsComponent, {
      width: '600px',
      data: {
        projectId: project.id,
        projectName: project.name,
        projectPath: project.path
      }
    });
  }

  openProjectEdit(project: MdProject): void {
    const dialogRef = this.dialog.open<ProjectEditDialogComponent, any, ProjectEditDialogResult>(ProjectEditDialogComponent, {
      width: '620px',
      maxHeight: '90vh',
      data: {
        id: project.id,
        name: project.name,
        description: project.description,
        path: project.path,
        hasCustomIcon: !!project.hasCustomIcon,
        iconUpdatedAt: project.iconUpdatedAt
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }
      // Sequential saves: first name/description (UserDB + .development.yml),
      // then participants (.development.yml), then the optional icon change.
      // Icon is last because it's the heaviest payload and the only one that
      // can be skipped (iconAction === 'none').
      this.projectService.updateProject({
        id: result.id,
        name: result.name,
        description: result.description
      }).subscribe({
        next: () => {
          this.projectService.saveParticipants(result.path, result.participants || []).subscribe({
            next: () => {
              this.applyIconChange(result, () => {
                this.projectService.fetchProjects();
                this.snackBar.open(this.translate.instant('PROJECTS.PROJECT_UPDATED'), 'OK', { duration: 2500 });
              });
            },
            error: (err) => {
              console.error('[Projects] Error saving participants:', err);
              this.projectService.fetchProjects();
              this.snackBar.open(this.translate.instant('PROJECTS.ERROR_SAVING_PARTICIPANTS'), 'OK', { duration: 4000 });
            }
          });
        },
        error: (err) => {
          console.error('[Projects] Error updating project:', err);
          this.snackBar.open(this.translate.instant('PROJECTS.ERROR_UPDATING_PROJECT'), 'OK', { duration: 4000 });
        }
      });
    });
  }

  /**
   * Persists a pending icon change (set / remove). No-op when iconAction is 'none'.
   * Errors are surfaced via the snackbar but do not block the rest of the save flow,
   * because at this point name/description/participants are already persisted.
   */
  private applyIconChange(result: ProjectEditDialogResult, done: () => void): void {
    console.debug('[Projects] applyIconChange', { iconAction: result.iconAction,
      pngLen: result.iconPngBase64?.length ?? 0, projectId: result.id });
    if (result.iconAction === 'set' && result.iconPngBase64) {
      this.projectService.setProjectIcon(result.id, result.iconPngBase64).subscribe({
        next: () => done(),
        error: (err) => {
          console.error('[Projects] Error saving project icon:', err);
          this.snackBar.open(this.translate.instant('PROJECTS.ERROR_SAVING_ICON'), 'OK', { duration: 4000 });
          done();
        }
      });
    } else if (result.iconAction === 'remove') {
      this.projectService.removeProjectIcon(result.id).subscribe({
        next: () => done(),
        error: (err) => {
          console.error('[Projects] Error removing project icon:', err);
          this.snackBar.open(this.translate.instant('PROJECTS.ERROR_SAVING_ICON'), 'OK', { duration: 4000 });
          done();
        }
      });
    } else {
      done();
    }
  }


  openRecent(): void {
    this.router.navigate(['/projects/openrecent']);
  }

  prepareToClone(): void {
    const dialogRef = this.dialog.open(ModernCloneProjectComponent, {
      width: '600px',
      maxHeight: '600px',
      data: null
    });
  }

  openNewFolder(): void {
    let data = new ShowFileMetadata();
    data.start = null;
    data.title = this.translate.instant('PROJECTS.SELECT_FOLDER');
    data.typeOfSelection = "Folders";
    data.buttonText = this.translate.instant('PROJECTS.SELECT_FOLDER_BTN');

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.data) {
        // Open configuration dialog after folder selection
        const configDialogRef = this.dialog.open(ProjectCreateConfigDialogComponent, {
          width: '500px',
          disableClose: true,
          data: { projectPath: result.data }
        });

        configDialogRef.afterClosed().subscribe(config => {
          if (config) {
            // Create project with configuration options
            this.projectService.createProjectWithConfig(config);
          }
        });
      }
    });

  }

  openSettings(): void {
    this.dialog.open(UnifiedSettingsDialogComponent, {
      width: '900px',
      maxHeight: '80vh',
      data: { initialTab: 'application' }
    });
  }

  /**
   * Check if project has a Git remote configured (cached)
   * Used to show/hide the Share button
   */
  projectHasRemote(project: MdProject): boolean {
    const cached = this.remoteUrlCache.get(project.path);

    if (cached !== undefined) {
      return cached.hasRemote;
    }

    // Mark as loading to prevent duplicate requests
    this.remoteUrlCache.set(project.path, { hasRemote: false, loading: true });

    // Fetch asynchronously and update cache
    this.gitService.getRemoteUrl(project.path).subscribe(result => {
      this.remoteUrlCache.set(project.path, {
        hasRemote: result.hasRemote,
        remoteUrl: result.remoteUrl,
        loading: false
      });
    });

    return false; // Return false initially, will update on next change detection
  }

  /**
   * Share project URL to clipboard
   * Generates: mdexplorer://configproject?repo=<url>&basePath=<parent-folder>
   */
  shareProject(project: MdProject, event: Event): void {
    event.stopPropagation(); // Prevent opening the project

    const cached = this.remoteUrlCache.get(project.path);
    if (!cached?.hasRemote || !cached?.remoteUrl) {
      this.snackBar.open(this.translate.instant('PROJECTS.NO_GIT_REMOTE'), 'OK', { duration: 3000 });
      return;
    }

    // Extract base path (parent folder of project)
    // e.g., C:\Progetti\myrepo -> C:\Progetti
    const lastSeparator = project.path.lastIndexOf('\\');
    const basePath = lastSeparator > 0 ? project.path.substring(0, lastSeparator) : project.path;

    // Build mdexplorer:// URL
    const shareUrl = `mdexplorer://configproject?repo=${encodeURIComponent(cached.remoteUrl)}&basePath=${encodeURIComponent(basePath)}`;

    // Copy to clipboard
    this.clipboard.copy(shareUrl);

    this.snackBar.open(this.translate.instant('PROJECTS.SHARE_URL_COPIED'), 'OK', { duration: 3000 });
    console.log('[Projects] Share URL copied:', shareUrl);
  }

  /**
   * Open the application log file (.NET backend)
   */
  openLog(): void {
    this.http.post<any>('../api/Diagnostics/OpenLog', {}).subscribe({
      next: (result) => {
        console.log('[Projects] Log opened:', result.path);
      },
      error: (err) => {
        console.error('[Projects] Error opening log:', err);
        this.snackBar.open(this.translate.instant('PROJECTS.ERROR_OPENING_LOG', { error: err.error?.error || err.message }), 'OK', { duration: 5000 });
      }
    });
  }

  /**
   * Open the Electron log file (only available in Electron environment)
   */
  openElectronLog(): void {
    const electronAPI = (window as any).electronAPI;
    if (electronAPI?.openElectronLog) {
      electronAPI.openElectronLog().then((result: any) => {
        if (result.success) {
          console.log('[Projects] Electron log opened:', result.path);
        } else {
          console.error('[Projects] Electron log not found:', result.error);
          this.snackBar.open(this.translate.instant('PROJECTS.ELECTRON_LOG_NOT_FOUND', { path: result.path }), 'OK', { duration: 5000 });
        }
      }).catch((err: any) => {
        console.error('[Projects] Error opening Electron log:', err);
        this.snackBar.open(this.translate.instant('PROJECTS.ERROR_OPENING_ELECTRON_LOG'), 'OK', { duration: 5000 });
      });
    } else {
      this.snackBar.open(this.translate.instant('PROJECTS.ELECTRON_LOG_DESKTOP_ONLY'), 'OK', { duration: 3000 });
    }
  }

  /**
   * Check if running in Electron environment
   */
  isElectron(): boolean {
    return !!(window as any).electronAPI;
  }

  /**
   * Open the P2P Manager dialog
   */
  openP2PManager(): void {
    const dialogRef = this.dialog.open(P2PManagerComponent, {
      width: '700px',
      maxHeight: '80vh',
      data: {}
    });
  }
}
