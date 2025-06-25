import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { IBranch } from '../models/branch';
import { DataToPull } from '../models/DataToPull'
import { CloneInfo } from '../models/cloneRequest';
import { GitlabSetting } from '../models/gitlab-setting';
import { ITag } from '../models/Tag';
import { ResposneClone } from './responses/ResponseClone';
import { 
  ModernGitRequest, 
  ModernGitResponse, 
  ModernPullResponse, 
  ModernCommitResponse, 
  ModernBranchStatusResponse,
  ModernResponsePull 
} from '../models/modern-git-models';


@Injectable({
  providedIn: 'root'
})
export class GITService implements OnDestroy {
  private _Settings: BehaviorSubject<GitlabSetting[]>;
  private gitPollingInterval: any = null;
  private readonly ACTIVE_POLLING_INTERVAL = 60000; // 60 secondi quando attivo
  private readonly INACTIVE_POLLING_INTERVAL = 300000; // 5 minuti quando inattivo
  private currentProjectPath: string = null;
  
  public currentBranch$: BehaviorSubject<IBranch> = new BehaviorSubject<IBranch>(
    {
      id: "", name: "",
      somethingIsChangedInTheBranch: true,
      howManyFilesAreChanged: 0,            
      fullPath: "",
      howManyCommitAreToPush:0,
    });

  public commmitsToPull$: BehaviorSubject<DataToPull> = new BehaviorSubject<DataToPull>({
    howManyFilesAreToPull: 0,
    howManyCommitAreToPush:0,
    somethingIsToPull: false,
    somethingIsToPush:false,
    connectionIsActive: false,
    whatFilesWillBeChanged: []
  });

  constructor(private http: HttpClient) {
    this.initializeSmartPolling();
  }

  /**
   * Set the current project path for modern Git operations
   */
  setProjectPath(path: string): void {
    this.currentProjectPath = path;
    console.log('Git service project path set to:', path);
    // Trigger immediate poll with new path
    if (path) {
      this.performPoll();
    }
  }

  /**
   * Inizializza il polling intelligente che si adatta alla visibilità della finestra
   */
  private initializeSmartPolling(): void {
    // Polling iniziale immediato
    this.performPoll();
    
    // Avvia polling con intervallo attivo
    this.startPolling(this.ACTIVE_POLLING_INTERVAL);
    
    // Listener per cambio visibilità finestra
    document.addEventListener('visibilitychange', () => {
      this.handleVisibilityChange();
    });
    
    // Listener per focus/blur finestra (backup per browser che non supportano visibilitychange)
    window.addEventListener('focus', () => {
      this.handleWindowFocus();
    });
    
    window.addEventListener('blur', () => {
      this.handleWindowBlur();
    });
    
    console.log('🔄 Smart Git polling initialized');
  }

  /**
   * Perform polling based on current configuration
   */
  private performPoll(): void {
    if (this.currentProjectPath) {
      // Use modern endpoints with SSH support
      console.log('🔄 Performing modern Git poll for:', this.currentProjectPath);
      
      // Get branch status
      this.modernGetBranchStatus(this.currentProjectPath).subscribe(
        branch => {
          this.currentBranch$.next(branch);
        },
        error => {
          console.error('Error in modern branch status:', error);
          // Set default empty state on error
          this.currentBranch$.next({
            id: "", name: "unknown",
            somethingIsChangedInTheBranch: false,
            howManyFilesAreChanged: 0,
            fullPath: this.currentProjectPath,
            howManyCommitAreToPush: 0
          });
        }
      );
      
      // Get pull/push data
      this.modernGetDataToPull(this.currentProjectPath).subscribe(
        pullData => {
          this.commmitsToPull$.next(pullData);
        },
        error => {
          console.error('Error in modern data to pull:', error);
        }
      );
    }
  }

  /**
   * Gestisce il cambio di visibilità della finestra
   */
  private handleVisibilityChange(): void {
    if (document.visibilityState === 'visible') {
      this.handleWindowFocus();
    } else {
      this.handleWindowBlur();
    }
  }

  /**
   * Quando la finestra diventa attiva: polling più frequente
   */
  private handleWindowFocus(): void {
    console.log('🟢 Window focused - activating frequent Git polling (60s)');
    this.startPolling(this.ACTIVE_POLLING_INTERVAL);
    // Polling immediato quando torna in focus
    this.performPoll();
  }

  /**
   * Quando la finestra diventa inattiva: polling meno frequente
   */
  private handleWindowBlur(): void {
    console.log('🟡 Window blurred - reducing Git polling frequency (5min)');
    this.startPolling(this.INACTIVE_POLLING_INTERVAL);
  }

  /**
   * Avvia polling con intervallo specificato
   */
  private startPolling(interval: number): void {
    // Ferma polling esistente
    if (this.gitPollingInterval) {
      clearInterval(this.gitPollingInterval);
    }
    
    // Avvia nuovo polling
    this.gitPollingInterval = setInterval(() => {
      this.performPoll();
    }, interval);
  }

  /**
   * Ferma completamente il polling (per cleanup)
   */
  public stopPolling(): void {
    if (this.gitPollingInterval) {
      clearInterval(this.gitPollingInterval);
      this.gitPollingInterval = null;
      console.log('🔴 Git polling stopped');
    }
  }

  clone(request: CloneInfo): Observable<ResposneClone> {
    const url = '../api/gitfeatures/cloneRepository';
    return this.http.post<ResposneClone>(url, request);
  }


  getBranchList():Observable<IBranch[]> {
    const url = '../api/gitservice/branches';
    return this.http.get<IBranch[]>(url);
  }

  checkoutSelectedBranch(selected: IBranch) {
    const url = '../api/gitservice/branches/feat/checkoutBranch';
    return this.http.post<IBranch>(url, selected);
  }

  getTagList() {
    const url = '../api/gitservice/tags';
    return this.http.get<ITag[]>(url);
  }

  storeGitlabSettings(user: string, password: string, gitlabLink: string) {
    const url = '../api/gitservice/gitlabsettings';
    let setting = new GitlabSetting();    
    return this.http.post<GitlabSetting>(url, setting);
  }

  getGitlabSettings(): Observable<GitlabSetting[]> {
    const url = '../api/gitservice/gitlabsettings';
    return this.http.get<GitlabSetting[]>(url);
  }


  // ===== MODERN GIT METHODS WITH NATIVE AUTHENTICATION =====

  /**
   * Pull using modern Git service with native authentication
   */
  modernPull(projectPath: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { ProjectPath: projectPath };
    const url = '../api/ModernGitToolbar/pull';
    
    return this.http.post<ModernPullResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Commit using modern Git service with native authentication
   */
  modernCommit(projectPath: string, commitMessage?: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { 
      ProjectPath: projectPath, 
      CommitMessage: commitMessage 
    };
    const url = '../api/ModernGitToolbar/commit';
    
    return this.http.post<ModernCommitResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Commit and push using modern Git service with native authentication
   */
  modernCommitAndPush(projectPath: string, commitMessage?: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { 
      ProjectPath: projectPath, 
      CommitMessage: commitMessage 
    };
    const url = '../api/ModernGitToolbar/commit-and-push';
    
    console.log('[DEBUG] Sending commit request:', JSON.stringify(request, null, 2));
    
    return this.http.post<ModernCommitResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Push using modern Git service with native authentication
   */
  modernPush(projectPath: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { ProjectPath: projectPath };
    const url = '../api/ModernGitToolbar/push-v2';
    
    return this.http.post<ModernGitResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Get branch status using modern Git service
   */
  modernGetBranchStatus(projectPath: string): Observable<IBranch> {
    const url = `../api/ModernGitToolbar/branch-status?projectPath=${encodeURIComponent(projectPath)}`;
    
    return this.http.get<ModernBranchStatusResponse>(url).pipe(
      map(response => ({
        id: '',
        name: response.name,
        somethingIsChangedInTheBranch: response.somethingIsChangedInTheBranch,
        howManyFilesAreChanged: response.howManyFilesAreChanged,
        howManyCommitAreToPush: response.howManyCommitAreToPush,
        fullPath: response.fullPath
      }))
    );
  }

  /**
   * Get data to pull/push using modern Git service
   */
  modernGetDataToPull(projectPath: string): Observable<DataToPull> {
    const url = `../api/ModernGitToolbar/get-data-to-pull?projectPath=${encodeURIComponent(projectPath)}`;
    
    return this.http.get<DataToPull>(url).pipe(
      catchError(error => {
        console.error('Error in modernGetDataToPull:', error);
        // Return empty data on error
        return of({
          somethingIsToPull: false,
          somethingIsToPush: false,
          howManyFilesAreToPull: 0,
          howManyCommitAreToPush: 0,
          connectionIsActive: false,
          whatFilesWillBeChanged: []
        });
      })
    );
  }

  /**
   * Adapts modern Git response to legacy format for backward compatibility
   */
  private adaptModernResponseToLegacy(response: ModernGitResponse): ModernResponsePull {
    return {
      isConnectionMissing: false, // Modern service handles connection issues differently
      isAuthenticationMissing: false, // Modern service uses native authentication
      thereAreConflicts: response.thereAreConflicts,
      errorMessage: response.errorMessage,
      whatFilesWillBeChanged: response.changedFiles || []
    };
  }

  /**
   * Cleanup quando il service viene distrutto
   */
  ngOnDestroy(): void {
    this.stopPolling();
    
    // Rimuovi event listeners per evitare memory leak
    document.removeEventListener('visibilitychange', this.handleVisibilityChange);
    window.removeEventListener('focus', this.handleWindowFocus);
    window.removeEventListener('blur', this.handleWindowBlur);
    
    console.log('🧹 Git service cleanup completed');
  }

}
