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
  ModernResponsePull,
  GitCommitInfo,
  GitHistoryRequest,
  GitHistoryResponse,
  RemoteStatus,
  SetupRemoteRequest,
  SetupRemoteResponse
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
    
  }

  /**
   * Perform polling based on current configuration
   * IMPORTANT: Checks remote status first to authenticate, then fetches Git data using cached credentials
   */
  private performPoll(): void {
    if (this.currentProjectPath) {
      // Step 1: Check remote status first (authenticates and caches credentials)
      this.checkRemoteStatus(this.currentProjectPath).subscribe(
        remoteStatus => {
          // Only proceed with Git operations if authentication is successful
          if (remoteStatus.hasRemote && remoteStatus.canAuthenticate) {
            // Step 2: Now fetch Git data (will use cached credentials, no additional auth)
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

            this.modernGetDataToPull(this.currentProjectPath).subscribe(
              pullData => {
                this.commmitsToPull$.next(pullData);
              },
              error => {
                console.error('Error in modern data to pull:', error);
              }
            );
          } else if (!remoteStatus.hasRemote) {
            // No remote configured - still get branch status but skip pull/push data
            this.modernGetBranchStatus(this.currentProjectPath).subscribe(
              branch => {
                this.currentBranch$.next(branch);
              },
              error => {
                console.error('Error in modern branch status:', error);
              }
            );
          }
        },
        error => {
          console.error('Error checking remote status in poll:', error);
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
    this.startPolling(this.ACTIVE_POLLING_INTERVAL);
    // Polling immediato quando torna in focus
    this.performPoll();
  }

  /**
   * Quando la finestra diventa inattiva: polling meno frequente
   */
  private handleWindowBlur(): void {
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
   * Clone repository using modern Git service with native authentication
   */
  modernClone(request: { url: string; localPath: string; branchName?: string }): Observable<{ success: boolean; error?: string }> {
    const url = '../api/ModernGit/clone';
    // Convert to PascalCase for C# backend
    const requestBody = {
      Url: request.url,
      LocalPath: request.localPath,
      BranchName: request.branchName || null
    };
    console.log('[GITService.modernClone] Sending to backend:', requestBody);
    return this.http.post<{ success: boolean; error?: string }>(url, requestBody).pipe(
      catchError(error => {
        console.error('[modernClone] Full error:', error);
        // Try to extract validation errors if present
        let errorMessage = 'Clone failed';
        if (error.error) {
          if (typeof error.error === 'string') {
            errorMessage = error.error;
          } else if (error.error.errors) {
            // Validation errors from ModelState
            const validationErrors = [];
            for (const field in error.error.errors) {
              validationErrors.push(`${field}: ${error.error.errors[field].join(', ')}`);
            }
            errorMessage = validationErrors.join('; ');
          } else if (error.error.message) {
            errorMessage = error.error.message;
          } else if (error.error.error) {
            errorMessage = error.error.error;
          }
        }
        return of({ success: false, error: errorMessage });
      })
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
   * Get commit history for a repository
   */
  getCommitHistory(projectPath: string, maxCommits?: number): Observable<GitCommitInfo[]> {
    const request: GitHistoryRequest = {
      repositoryPath: projectPath,
      maxCommits: maxCommits || 50
    };
    const url = '../api/ModernGit/history';

    return this.http.post<GitHistoryResponse>(url, request).pipe(
      map(response => {
        if (response.success && response.commits) {
          // Convert date strings to Date objects if needed
          return response.commits.map(commit => ({
            ...commit,
            date: typeof commit.date === 'string' ? new Date(commit.date) : commit.date,
            shortHash: commit.hash ? commit.hash.substring(0, 7) : '',
            isMerge: commit.parents && commit.parents.length > 1
          }));
        }
        return [];
      }),
      catchError(error => {
        console.error('Error getting commit history:', error);
        return of([]);
      })
    );
  }

  /**
   * Check if repository has remote configured
   */
  checkRemoteStatus(projectPath: string): Observable<RemoteStatus> {
    const url = `../api/ModernGit/remote-status?repositoryPath=${encodeURIComponent(projectPath)}`;

    return this.http.get<RemoteStatus>(url).pipe(
      catchError(error => {
        console.error('Error checking remote status:', error);
        return of({
          hasRemote: false,
          isGitRepository: false,
          errorMessage: error.message || 'Failed to check remote status',
          canAuthenticate: false
        });
      })
    );
  }

  /**
   * Remove a remote from the repository
   */
  removeRemote(projectPath: string, remoteName: string = 'origin'): Observable<any> {
    const url = `../api/ModernGit/remove-remote?repositoryPath=${encodeURIComponent(projectPath)}&remoteName=${encodeURIComponent(remoteName)}`;

    return this.http.delete<any>(url).pipe(
      catchError(error => {
        console.error('Error removing remote:', error);
        return of({
          success: false,
          error: error.error?.error || error.message || 'Failed to remove remote'
        });
      })
    );
  }

  /**
   * Setup GitHub remote for repository
   */
  setupGitHubRemote(projectPath: string, organization: string, repositoryName: string,
                    saveOrganization: boolean = true, pushAfterAdd: boolean = true,
                    repositoryDescription?: string, isPrivate?: boolean): Observable<SetupRemoteResponse> {
    const request: SetupRemoteRequest = {
      repositoryPath: projectPath,
      organization: organization,
      repositoryName: repositoryName,
      repositoryDescription: repositoryDescription,
      isPrivate: isPrivate !== undefined ? isPrivate : true,
      saveOrganization: saveOrganization,
      pushAfterAdd: pushAfterAdd
    };
    const url = '../api/ModernGit/setup-remote';

    return this.http.post<SetupRemoteResponse>(url, request).pipe(
      catchError(error => {
        console.error('Error setting up remote:', error);
        return of({
          success: false,
          error: error.error?.error || error.message || 'Failed to setup remote'
        });
      })
    );
  }

  /**
   * Get saved GitHub organization
   */
  getGitHubOrganization(): Observable<string> {
    const url = '../api/ModernGit/github-organization';

    return this.http.get<{ organization: string }>(url).pipe(
      map(response => response.organization || ''),
      catchError(error => {
        console.error('Error getting GitHub organization:', error);
        return of('');
      })
    );
  }

  /**
   * Sets the GitHub personal access token
   */
  setGitHubToken(token: string): Observable<any> {
    const url = '../api/ModernGit/github-token';

    return this.http.post<any>(url, { token: token }).pipe(
      map(response => response),
      catchError(error => {
        console.error('Error setting GitHub token:', error);
        return of({ success: false });
      })
    );
  }

  /**
   * Gets the GitHub token status (masked)
   */
  getGitHubToken(): Observable<any> {
    const url = '../api/ModernGit/github-token';

    return this.http.get<any>(url).pipe(
      catchError(error => {
        console.error('Error getting GitHub token:', error);
        return of({ hasToken: false, maskedToken: '', tokenValid: false });
      })
    );
  }

  /**
   * Tests the GitHub token validity
   */
  testGitHubToken(): Observable<any> {
    const url = '../api/ModernGit/test-github-token';

    return this.http.post<any>(url, {}).pipe(
      map(response => response),
      catchError(error => {
        console.error('Error testing GitHub token:', error);
        return of({ success: false, tokenValid: false });
      })
    );
  }

  /**
   * Save GitHub organization for future use
   */
  saveGitHubOrganization(organization: string): Observable<boolean> {
    const url = '../api/ModernGit/github-organization';

    return this.http.post<{ success: boolean }>(url, { organization: organization }).pipe(
      map(response => response.success),
      catchError(error => {
        console.error('Error saving GitHub organization:', error);
        return of(false);
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
    
  }

}
