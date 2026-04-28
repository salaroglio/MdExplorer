import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MdProject } from '../models/md-project';
import { Participant, GitAuthor, CurrentGitUser } from '../models/participant';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { ProjectCreateConfigOptions } from '../../projects/dialogs/project-create-config/project-create-config.model';
import { CompatibilityMode } from '../../models/compatibility-mode.model';

interface ProjectOpenedResponse {
  success: boolean;
  roomId?: string;
  oderId?: string;
  projectUsersCount?: number;
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {

  private _mdProjects: BehaviorSubject<MdProject[]>;
  currentProjects$: BehaviorSubject<MdProject> = new BehaviorSubject<MdProject>(null);

  // RAG enabled status for current project
  ragEnabled$ = new BehaviorSubject<boolean>(false);

  // Copilot CLI auto-select state for the freshly opened project.
  // Emits the tuple from SetFolderProject response: { autoSelect, available, defaultModel }.
  // Consumers (ai-chat.component) decide whether to silently connect to Copilot CLI
  // or to disable the chat with a "not installed" banner.
  copilotCliAutoConfig$ = new BehaviorSubject<{ autoSelect: boolean; available: boolean; defaultModel: string | null } | null>(null);

  // Emette PRIMA che il progetto cambi (per mostrare skeleton loader)
  private projectChangingSubject = new Subject<void>();
  projectChanging$ = this.projectChangingSubject.asObservable();

  // Track current project's chat room info for cleanup
  private currentRoomId: string | null = null;
  private currentOderId: string | null = null;

  // Retry handle for the Copilot CLI availability re-check (see emitCopilotCliAutoConfig).
  private copilotCliRetryTimer: any = null;

  get mdProjects() {
    return this._mdProjects.asObservable();
  }

  constructor(
    private http: HttpClient,
    private injector: Injector
  ) {
    this.dataStore = { mdProjects: [] };
    this._mdProjects = new BehaviorSubject<MdProject[]>([]);
  }

  private dataStore: {

    mdProjects: MdProject[]
  }

  fetchProjects() {
    const url = '../api/MdProjects/GetProjects';
    this.http.get<MdProject[]>(url)
      .subscribe(data => {
        this.dataStore.mdProjects = data;
        this._mdProjects.next(Object.assign({}, this.dataStore).mdProjects);
      }, error => {
        console.log(error);
      });
  }

  async SetSideNavWidth(mdProject: MdProject) {
    const url = '../api/MdProjects/SetSideNavWidth';
    await this.http.post<any>(url, mdProject).toPromise();
  }

  setNewFolderProject(path: string):void {
    this.projectChangingSubject.next(); // Notifica cambio progetto in corso

    // Close previous project if any
    this.notifyProjectClosed();

    this.http.post<any>('../api/MdProjects/SetFolderProject', { path: path }).subscribe(async response => {
      this.currentProjects$.next(response);

      // Update window title for Electron taskbar preview
      this.updateWindowTitle(response.name);

      // Register project open for chat presence tracking
      this.notifyProjectOpened(path);

      // Refresh RAG enabled status
      this.refreshRagStatus();

      // Apply PlantUML dark-mode preference for this project
      this.applyPlantUmlKeepOriginalClass(path);

      // Emit Copilot CLI auto-select hint for ai-chat to consume
      this.emitCopilotCliAutoConfig(response);

      // Update compatibility mode from response
      if (response.compatibilityMode) {
        const mode = response.compatibilityMode === 'github' ? CompatibilityMode.GitHub :
                     response.compatibilityMode === 'commonmark' ? CompatibilityMode.CommonMark :
                     CompatibilityMode.MdExplorer;
        console.log('Setting compatibility mode from project open response:', mode);

        // Get CompatibilityModeService using dynamic import to avoid circular dependency
        const { CompatibilityModeService } = await import('../../services/compatibility-mode.service');
        const compatibilityService = this.injector.get(CompatibilityModeService);
        compatibilityService.updateMode(mode);
      }
    });

  }

  createProjectWithConfig(config: ProjectCreateConfigOptions): void {
    this.projectChangingSubject.next(); // Notifica cambio progetto in corso

    // Close previous project if any
    this.notifyProjectClosed();

    const request = {
      path: config.projectPath,
      initializeGit: config.initializeGit,
      addCopilotInstructions: config.addCopilotInstructions
    };

    this.http.post<any>('../api/MdProjects/SetFolderProject', request).subscribe(async response => {
      this.currentProjects$.next(response);

      // Update window title for Electron taskbar preview
      this.updateWindowTitle(response.name);

      // Register project open for chat presence tracking
      this.notifyProjectOpened(config.projectPath);

      // Refresh RAG enabled status
      this.refreshRagStatus();

      // Apply PlantUML dark-mode preference for this project
      this.applyPlantUmlKeepOriginalClass(config.projectPath);

      // Emit Copilot CLI auto-select hint for ai-chat to consume
      this.emitCopilotCliAutoConfig(response);

      // Update compatibility mode from response
      if (response.compatibilityMode) {
        const mode = response.compatibilityMode === 'github' ? CompatibilityMode.GitHub :
                     response.compatibilityMode === 'commonmark' ? CompatibilityMode.CommonMark :
                     CompatibilityMode.MdExplorer;
        console.log('Setting compatibility mode from project create response:', mode);

        // Get CompatibilityModeService using dynamic import to avoid circular dependency
        const { CompatibilityModeService } = await import('../../services/compatibility-mode.service');
        const compatibilityService = this.injector.get(CompatibilityModeService);
        compatibilityService.updateMode(mode);
      }
    }, error => {
      console.error('Error creating project with config:', error);
    });
  }

  //setNewFolderProject(path: string, callback: (data: any, objectThis: any) => any, objectThis: any) {
  //  const url = '../api/MdProjects/SetFolderProject';
  //  this.http.post<any>(url, { path: path }).subscribe(data => {
  //    callback(data, objectThis);
  //  });
  //}

  deleteProject(project: any, callback: (data: any, objectThis: any) => any, objectThis: any) {
    const url = '../api/MdProjects/DeleteProject';
    this.http.post<any>(url, project).subscribe(data => {
      callback(data, objectThis);
    });
  }

  updateProject(payload: { id: string; name: string; description?: string }): Observable<MdProject> {
    const url = '../api/MdProjects/UpdateProject';
    return this.http.post<MdProject>(url, payload);
  }

  getParticipants(projectPath: string): Observable<Participant[]> {
    const params = new HttpParams().set('path', projectPath);
    return this.http.get<Participant[]>('../api/MdProjects/GetParticipants', { params });
  }

  saveParticipants(projectPath: string, participants: Participant[]): Observable<Participant[]> {
    const params = new HttpParams().set('path', projectPath);
    return this.http.put<Participant[]>('../api/MdProjects/Participants', participants, { params });
  }

  getGitAuthors(projectPath: string): Observable<GitAuthor[]> {
    const params = new HttpParams().set('path', projectPath);
    return this.http.get<GitAuthor[]>('../api/MdProjects/GitAuthors', { params });
  }

  getCurrentGitUser(projectPath: string | null): Observable<CurrentGitUser> {
    let params = new HttpParams();
    if (projectPath) {
      params = params.set('path', projectPath);
    }
    return this.http.get<CurrentGitUser>('../api/MdProjects/CurrentGitUser', { params });
  }

  /**
   * Closes the current project and deallocates backend resources (FileSystemWatcher, database contexts).
   * Should be called when navigating back to the projects list.
   */
  closeCurrentProject(): Observable<any> {
    // Notify chat system that project is being closed
    this.notifyProjectClosed();
    // Reset window title
    this.updateWindowTitle(null);
    // Reset PlantUML dark-mode override (scoped to the closing project)
    document.body.classList.remove('plantuml-keep-original');
    return this.http.post<any>('../api/MdProjects/CloseProject', {});
  }

  /**
   * Re-registers the current project with the backend after a SignalR reconnection.
   * This is necessary because when the SignalR connection is lost, the backend
   * cleans up FileSystemWatcher and DatabaseContext for the old ConnectionId.
   * After reconnection with a new ConnectionId, we need to re-register.
   */
  reregisterCurrentProject(): void {
    const currentProject = this.currentProjects$.getValue();
    if (currentProject && currentProject.path) {
      console.log('[ProjectsService] Re-registering project after SignalR reconnection:', currentProject.path);

      this.http.post<any>('../api/MdProjects/SetFolderProject', { path: currentProject.path }).subscribe(
        response => {
          console.log('[ProjectsService] Project re-registered successfully');
          // Update the project in case any settings changed
          this.currentProjects$.next(response);
          // Update window title
          this.updateWindowTitle(response.name);
          // Re-apply PlantUML dark-mode preference
          this.applyPlantUmlKeepOriginalClass(currentProject.path);
        },
        error => {
          console.error('[ProjectsService] Failed to re-register project:', error);
        }
      );
    } else {
      console.log('[ProjectsService] No current project to re-register');
    }
  }

  /**
   * Emits Copilot CLI auto-select configuration from the SetFolderProject response
   * so that ai-chat can decide whether to silently connect or disable the chat.
   *
   * When the backend's availability cache is cold, SetFolderProject returns
   * available=false provisionally and warms the probe in background (~1s).
   * To recover from that race we schedule a short re-check against the
   * dedicated availability endpoint and re-emit the updated value if Copilot
   * turns out to be installed.
   */
  private emitCopilotCliAutoConfig(response: any): void {
    // Cancel any pending re-check from a previous project switch.
    if (this.copilotCliRetryTimer) {
      clearTimeout(this.copilotCliRetryTimer);
      this.copilotCliRetryTimer = null;
    }

    if (response == null) return;
    if (typeof response.copilotCliAutoSelect !== 'boolean') {
      this.copilotCliAutoConfig$.next(null);
      return;
    }

    const autoSelect = response.copilotCliAutoSelect === true;
    const available = response.copilotCliAvailable === true;
    const defaultModel = response.copilotCliDefaultModel ?? null;

    this.copilotCliAutoConfig$.next({ autoSelect, available, defaultModel });

    // If auto-select is on but the backend reported unavailable, it might just
    // be that the probe cache was cold. Re-check once after the background
    // warm-up has had time to finish.
    if (autoSelect && !available) {
      this.copilotCliRetryTimer = setTimeout(() => {
        this.copilotCliRetryTimer = null;
        this.http.get<{ configured: boolean }>('../api/CopilotCli/configured').subscribe({
          next: r => {
            if (r?.configured === true) {
              this.copilotCliAutoConfig$.next({ autoSelect: true, available: true, defaultModel });
            }
          },
          error: () => { /* silent — leave provisional unavailable state */ }
        });
      }, 2000);
    }
  }

  /**
   * Fetches RAG enabled status for the current project and updates ragEnabled$.
   */
  private refreshRagStatus(): void {
    this.http.get<any>('../api/Rag/status').subscribe(
      response => {
        this.ragEnabled$.next(response.enabled ?? false);
      },
      error => {
        console.warn('[ProjectsService] Failed to fetch RAG status:', error);
        this.ragEnabled$.next(false);
      }
    );
  }

  /**
   * Fetches the PlantUmlKeepOriginalColorsInDarkMode project setting and toggles
   * the body class used by dark-theme.css to suppress the dark-mode invert filter.
   */
  private applyPlantUmlKeepOriginalClass(projectPath: string): void {
    this.http.get<{enabled: boolean}>(
      '../api/ProjectSettings/GetPlantUmlKeepOriginalColorsSetting',
      { params: { projectPath } }
    ).subscribe(
      response => {
        document.body.classList.toggle('plantuml-keep-original', !!response?.enabled);
      },
      error => {
        console.warn('[ProjectsService] Failed to fetch PlantUML keep-original setting:', error);
        document.body.classList.remove('plantuml-keep-original');
      }
    );
  }

  /**
   * Notify the chat system that a project has been opened.
   * This registers the user in the project users count.
   */
  private notifyProjectOpened(projectPath: string): void {
    this.http.post<ProjectOpenedResponse>('../api/GitChat/project-opened', {
      repositoryPath: projectPath
    }).subscribe(
      response => {
        if (response.success) {
          this.currentRoomId = response.roomId || null;
          this.currentOderId = response.oderId || null;
          console.log('[ProjectsService] Project opened registered, roomId:', response.roomId,
            'oderId:', response.oderId, 'users:', response.projectUsersCount);
        } else {
          // Not a git repo or no remote - this is fine, just don't track
          console.log('[ProjectsService] Project opened but not tracking (no git remote):', response.error);
          this.currentRoomId = null;
          this.currentOderId = null;
        }
      },
      error => {
        console.warn('[ProjectsService] Failed to register project open:', error);
        this.currentRoomId = null;
        this.currentOderId = null;
      }
    );
  }

  /**
   * Notify the chat system that a project has been closed.
   * This unregisters the user from the project users count.
   */
  private notifyProjectClosed(): void {
    if (this.currentRoomId && this.currentOderId) {
      this.http.post<any>('../api/GitChat/project-closed', {
        roomId: this.currentRoomId,
        oderId: this.currentOderId
      }).subscribe(
        response => {
          console.log('[ProjectsService] Project closed registered');
        },
        error => {
          console.warn('[ProjectsService] Failed to register project close:', error);
        }
      );

      this.currentRoomId = null;
      this.currentOderId = null;
    }
  }

  /**
   * Updates the Electron window title to show the project name in taskbar preview.
   * Only works when running in Electron.
   */
  private updateWindowTitle(projectName: string | null): void {
    if ((window as any).electronAPI?.setWindowTitle) {
      const title = projectName ? `${projectName} - MdExplorer` : 'MdExplorer';
      (window as any).electronAPI.setWindowTitle(title);
    }
  }

}
