import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Clipboard } from '@angular/cdk/clipboard';
import { MdProject } from '../md-explorer/models/md-project';
import { MdFileService } from '../md-explorer/services/md-file.service';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { GITService } from '../git/services/gitservice.service';
import { NewProjectComponent } from './new-project/new-project.component';
import { ShowFileSystemComponent } from '../commons/components/show-file-system/show-file-system.component';
import { ModernCloneProjectComponent } from './dialogs/modern-clone-project/modern-clone-project.component';
import { ProjectCreateConfigDialogComponent } from './dialogs/project-create-config/project-create-config-dialog.component';
import { ProjectSettingsComponent } from './project-settings/project-settings.component';
import { NgDialogAnimationService } from '../shared/NgDialogAnimationService';
import { SettingsComponent } from '../md-explorer/components/dialogs/settings/settings.component';
import { ShowFileMetadata } from '../commons/components/show-file-system/show-file-metadata';
import { versionInfo } from '../../environments/version'; // Importa la versione

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
    private snackBar: MatSnackBar
  ) { }

    ngOnDestroy(): void {
      console.log('ProjectsComponent destroyed!');
    }

  public dataSource: Observable<MdProject[]>
  public dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }]

  ngOnInit(): void {
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
    data.title = "Select project folder";
    data.typeOfSelection = "Folders";
    data.buttonText = "Select folder"; // Testo personalizzato

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
    const dialogRef = this.dialogAn.open(SettingsComponent, {
      width: '600px',
      animation: {},
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
      this.snackBar.open('No Git remote configured for this project', 'OK', { duration: 3000 });
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

    this.snackBar.open('Share URL copied to clipboard!', 'OK', { duration: 3000 });
    console.log('[Projects] Share URL copied:', shareUrl);
  }
}
